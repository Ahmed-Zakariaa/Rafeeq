# ── Build ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Rafeeq.BE/ Rafeeq.BE/
RUN dotnet restore Rafeeq.BE/Rafeeq.Api/Rafeeq.Api.csproj
RUN dotnet publish Rafeeq.BE/Rafeeq.Api/Rafeeq.Api.csproj -c Release -o /app --no-restore

# ── Runtime ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
# Render (and most PaaS) inject $PORT; bind ASP.NET to it. Shell form so $PORT expands at runtime.
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet Rafeeq.Api.dll"]
