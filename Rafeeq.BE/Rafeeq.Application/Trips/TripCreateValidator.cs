using FluentValidation;
using Rafeeq.Domain.Trips.DTOs;

namespace Rafeeq.Application.Trips;

public class TripCreateValidator : AbstractValidator<TripCreateDto>
{
    // v1 cost-share cap is a flat sanity ceiling (no distance model yet). When a per-km fare table
    // arrives, replace the constant with: pricePerSeat <= estimatedTripCost / seatsLimit.
    private const decimal MaxPricePerSeat = 2000m;

    public TripCreateValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0);
        RuleFor(x => x.OriginCityId).GreaterThan(0);
        RuleFor(x => x.DestinationCityId).GreaterThan(0)
            .NotEqual(x => x.OriginCityId).WithMessage("originAndDestinationMustDiffer");
        RuleFor(x => x.DepartureDateTime).GreaterThan(DateTime.UtcNow).WithMessage("departureMustBeInFuture");
        RuleFor(x => x.SeatsLimit).InclusiveBetween(1, 8);
        RuleFor(x => x.PickupPointText).NotEmpty().MaximumLength(200);

        When(x => !x.IsFree, () =>
        {
            RuleFor(x => x.PricePerSeat)
                .GreaterThan(0).WithMessage("priceRequiredForPaidTrip")
                .LessThanOrEqualTo(MaxPricePerSeat).WithMessage("priceExceedsCostShareCap");
        });
    }
}
