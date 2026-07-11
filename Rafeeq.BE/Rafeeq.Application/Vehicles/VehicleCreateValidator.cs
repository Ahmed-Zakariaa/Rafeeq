using FluentValidation;
using Rafeeq.Domain.Vehicles.DTOs;

namespace Rafeeq.Application.Vehicles;

public class VehicleCreateValidator : AbstractValidator<VehicleCreateDto>
{
    public VehicleCreateValidator()
    {
        RuleFor(x => x.Make).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Color).NotEmpty().MaximumLength(30);
        RuleFor(x => x.PlateNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.SeatsCapacity).InclusiveBetween(1, 8);
    }
}
