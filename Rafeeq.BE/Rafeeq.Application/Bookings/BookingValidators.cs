using FluentValidation;
using Rafeeq.Domain.Bookings.DTOs;

namespace Rafeeq.Application.Bookings;

public class BookingRequestValidator : AbstractValidator<BookingRequestDto>
{
    public BookingRequestValidator()
    {
        RuleFor(x => x.TripUniqueId).NotEmpty();
        RuleFor(x => x.PickupNote).MaximumLength(200);
    }
}

public class BookingRespondValidator : AbstractValidator<BookingRespondDto>
{
    public BookingRespondValidator()
    {
        RuleFor(x => x.BookingUniqueId).NotEmpty();
    }
}
