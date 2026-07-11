using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using Rafeeq.Domain.Common;

namespace Rafeeq.Api.Filters;

/// <summary>
/// Runs any registered FluentValidation validator for each action argument before the action.
/// On failure it throws BusinessException, which the ExceptionMiddleware turns into a clean 400.
/// (Avoids the deprecated FluentValidation.AspNetCore auto-validation package.)
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _sp;
    public ValidationFilter(IServiceProvider sp) => _sp = sp;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            if (_sp.GetService(validatorType) is IValidator validator)
            {
                var result = await validator.ValidateAsync(new ValidationContext<object>(arg));
                if (!result.IsValid)
                {
                    var errors = result.Errors
                        .Select(e => new ErrorMessageDto { PropertyName = e.PropertyName, ErrorMessage = e.ErrorMessage })
                        .ToList();
                    throw new BusinessException(errors);
                }
            }
        }

        await next();
    }
}