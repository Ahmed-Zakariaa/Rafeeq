using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Rafeeq.Domain.Common;
using Rafeeq.Api.Filters;
using Rafeeq.Application;
using Rafeeq.Infrastructure;
using Rafeeq.Infrastructure.Middleware;
using Rafeeq.Infrastructure.Persistence;
using Rafeeq.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// MVC + global FluentValidation filter.
// PascalCase JSON (PropertyNamingPolicy = null) so the response envelope matches the FE contract
// used in ECM: API_RES<T> = { Data, IsSuccess, Message, Total, PageNumber, PageSize }.
builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddOpenApi();

// Swagger UI (browsable API explorer at /swagger) with a JWT "Authorize" button.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Rafeeq API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT returned by Auth/Login (no 'Bearer ' prefix)."
    });
    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc)] = new List<string>()
    });
});

// CORS for the Angular dev origin
const string CorsPolicy = "RafeeqFE";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

// JWT auth
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

// Secrets: AES key for encrypted ids comes from config (Encryption:Key); dev falls back to a built-in key.
var encryptionKey = builder.Configuration["Encryption:Key"];
EncryptionHelper.Configure(encryptionKey ?? string.Empty);

// Fail fast: Production must not run on the committed dev/default secrets.
if (builder.Environment.IsProduction())
{
    if (string.IsNullOrWhiteSpace(jwt.SecretKey)
        || jwt.SecretKey.Contains("dev_only") || jwt.SecretKey.Contains("CHANGE_ME"))
        throw new InvalidOperationException("Set a real Jwt:SecretKey (env var Jwt__SecretKey) in Production.");
    if (string.IsNullOrWhiteSpace(encryptionKey))
        throw new InvalidOperationException("Set Encryption:Key (env var Encryption__Key) in Production.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
        };

        // Revoke access the moment a user is suspended/banned/deactivated (a still-valid token
        // alone isn't enough). One small DB check per authenticated request.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                var idValue = ctx.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(idValue, out var uid))
                {
                    ctx.Fail("invalidToken");
                    return;
                }

                var db = ctx.HttpContext.RequestServices.GetRequiredService<RafeeqDbContext>();
                var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == uid);
                if (user is null || !user.IsActivated || user.AccountStatus != AccountStatus.Active)
                    ctx.Fail("accountNotActive");
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Central error handling first
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rafeeq API v1");
        c.RoutePrefix = "swagger"; // UI served at /swagger
    });
}

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply migrations + seed reference data (roles, EG+KSA countries & cities) on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RafeeqDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<Rafeeq.Domain.Identity.IService.IPasswordHasher>();
    await DbSeeder.SeedAsync(db, hasher);
}

app.Run();
