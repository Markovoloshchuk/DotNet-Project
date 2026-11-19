using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Catalog.Api.Filters;

public sealed class FluentValidationActionFilter : IAsyncActionFilter, IOrderedFilter
{
    private static readonly MethodInfo ValidateMethod =
        typeof(FluentValidationActionFilter).GetMethod(
            nameof(ValidateTypedAsync),
            BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException(
            "Validation method could not be found.");

    // ApiController's automatic ModelState filter runs earlier.
    public int Order => -1000;

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var serviceProvider = context.HttpContext.RequestServices;
        var cancellationToken = context.HttpContext.RequestAborted;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var argumentType = argument.GetType();

            var validatorType = typeof(IValidator<>)
                .MakeGenericType(argumentType);

            var validator = serviceProvider.GetService(validatorType);

            if (validator is null)
            {
                continue;
            }

            var validationTask = (Task<ValidationResult>)ValidateMethod
                .MakeGenericMethod(argumentType)
                .Invoke(
                    null,
                    new object?[]
                    {
                        validator,
                        argument,
                        cancellationToken
                    })!;

            var validationResult = await validationTask;

            foreach (var error in validationResult.Errors)
            {
                context.ModelState.AddModelError(
                    error.PropertyName,
                    error.ErrorMessage);
            }
        }

        if (!context.ModelState.IsValid)
        {
            var problemDetails = new ValidationProblemDetails(
                context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path
            };

            context.Result = new BadRequestObjectResult(problemDetails);
            return;
        }

        await next();
    }

    private static Task<ValidationResult> ValidateTypedAsync<T>(
        object validator,
        object model,
        CancellationToken cancellationToken)
    {
        var typedValidator = (IValidator<T>)validator;

        return typedValidator.ValidateAsync(
            (T)model,
            cancellationToken);
    }
}