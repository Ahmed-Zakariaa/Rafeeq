using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Rafeeq.Application.Bookings;
using Rafeeq.Application.Identity;
using Rafeeq.Application.Notifications;
using Rafeeq.Application.Ratings;
using Rafeeq.Application.Reports;
using Rafeeq.Application.Trips;
using Rafeeq.Application.Vehicles;
using Rafeeq.Application.Verification;
using Rafeeq.Domain.Bookings.IService;
using Rafeeq.Domain.Identity.IService;
using Rafeeq.Domain.Notifications.IService;
using Rafeeq.Domain.Ratings.IService;
using Rafeeq.Domain.Reports.IService;
using Rafeeq.Domain.Trips.IService;
using Rafeeq.Domain.Vehicles.IService;
using Rafeeq.Domain.Verification.IService;

namespace Rafeeq.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IVerificationService, VerificationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAdminRoleService, AdminRoleService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
