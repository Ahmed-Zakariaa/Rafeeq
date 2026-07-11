namespace Rafeeq.Domain.Common;

public class ErrorMessageDto
{
    public string? PropertyName { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Thrown for handled/expected business errors. The exception middleware turns it into a clean
/// response. The message is a localization key (the FE/localizer resolves it).
/// </summary>
public class BusinessException : Exception
{
    public List<ErrorMessageDto> Errors { get; }

    public BusinessException(string messageKey, string? propertyName = null) : base(messageKey)
    {
        Errors = new() { new ErrorMessageDto { ErrorMessage = messageKey, PropertyName = propertyName } };
    }

    public BusinessException(List<ErrorMessageDto> errors)
        : base(errors.FirstOrDefault()?.ErrorMessage ?? "businessError")
    {
        Errors = errors;
    }
}
