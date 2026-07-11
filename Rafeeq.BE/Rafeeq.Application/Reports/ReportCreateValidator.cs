using FluentValidation;
using Rafeeq.Domain.Reports.DTOs;

namespace Rafeeq.Application.Reports;

public class ReportCreateValidator : AbstractValidator<ReportCreateDto>
{
    public ReportCreateValidator()
    {
        RuleFor(x => x.BookingUniqueId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
