using FluentValidation;
using LapBox.Application.Common.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace LapBox.API.Infrastructure;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
{

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred. TraceId: {TraceId}", httpContext.TraceIdentifier);

        // 1️⃣ ValidationException → 422
        if (exception is ValidationException validationException)
        {
            var problemDetails = new ValidationProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "One or more validation errors occurred.",
                Type = "https://lapbox.com/errors/validation",
                Errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())
            };
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        // 2️⃣ Application layer errors (Result failures)
        if (exception is ApplicationException appException)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = appException.Message,
                Type = "https://lapbox.com/errors/application"
            };
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        // 3️⃣ Catch-all → 500
        var internalProblemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An internal server error occurred.",
            Type = "https://lapbox.com/errors/internal",
            Detail = env.IsDevelopment() ? exception.Message : null
        };
        internalProblemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(internalProblemDetails, cancellationToken);
        return true;
    }
}
