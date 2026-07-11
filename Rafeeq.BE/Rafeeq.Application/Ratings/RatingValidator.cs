using FluentValidation;
using Rafeeq.Domain.Ratings.DTOs;

namespace Rafeeq.Application.Ratings;

public class RatingValidator : AbstractValidator<RateDto>
{
    public RatingValidator()
    {
        RuleFor(x => x.BookingUniqueId).NotEmpty();
        RuleFor(x => x.Stars).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}
