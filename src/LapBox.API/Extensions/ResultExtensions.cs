using LapBox.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace LapBox.API.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Maps a Result{T} to an appropriate HTTP response.
    /// Success → the value (200/201/204)
    /// Failure → ProblemDetails with appropriate status code (and ValidationProblemDetails for validation errors)
    /// </summary>
    public static IActionResult ToHttpResponse<T>(this Result<T> result, ControllerBase controller, bool created = false)
    {
        // 1. معالجة حالة النجاح
        if (result.IsSuccess)
        {
            return result.Value switch
            {
                null => controller.NoContent(),
                _ => created
                    ? controller.CreatedAtAction(string.Empty, result.Value)
                    : controller.Ok(result.Value)
            };
        }

        var (statusCode, type, title) = MapErrorKindToHttpStatus(result.TopError.Type);

        // 2. معالجة أخطاء الـ Validation بشكل خاص (لدعم الأخطاء المتعددة)
        if (result.TopError.Type == ErrorKind.Validation)
        {
            // بناء قاموس بالأخطاء (بافتراض أن كائن Result يحتوي على خاصية Errors)
            // إذا لم يكن يحتوي عليها، سيأخذ الـ TopError كعنصر وحيد
            var validationErrors = result.Errors?.Any() == true
                ? result.Errors.GroupBy(e => e.Code).ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray())
                : new Dictionary<string, string[]> { { result.TopError.Code, new[] { result.TopError.Description } } };

            var validationProblemDetails = new ValidationProblemDetails(validationErrors)
            {
                Type = $"https://lapbox.com/errors/{type}",
                Title = title,
                Status = statusCode,
                Detail = "One or more validation errors occurred."
            };

            return controller.UnprocessableEntity(validationProblemDetails);
        }

        // 3. معالجة باقي أنواع الأخطاء العادية
        var problemDetails = new ProblemDetails
        {
            Type = $"https://lapbox.com/errors/{type}",
            Title = title,
            Status = statusCode,
            Detail = result.TopError.Description,
            Extensions = { ["code"] = result.TopError.Code }
        };

        return statusCode switch
        {
            StatusCodes.Status401Unauthorized => controller.Unauthorized(problemDetails),
            StatusCodes.Status403Forbidden => controller.Forbid(),
            StatusCodes.Status404NotFound => controller.NotFound(problemDetails),
            StatusCodes.Status409Conflict => controller.Conflict(problemDetails),
            StatusCodes.Status423Locked => controller.StatusCode(StatusCodes.Status423Locked, problemDetails),
            _ => controller.StatusCode(statusCode, problemDetails)
        };
    }

    public static (int StatusCode, string Type, string Title) MapErrorKindToHttpStatus(ErrorKind kind) => kind switch
    {
        ErrorKind.Validation => (StatusCodes.Status422UnprocessableEntity, "validation", "Validation error."),
        ErrorKind.NotFound => (StatusCodes.Status404NotFound, "not-found", "Resource not found."),
        ErrorKind.Conflict => (StatusCodes.Status409Conflict, "conflict", "Conflict error."),
        ErrorKind.Unauthorized => (StatusCodes.Status401Unauthorized, "unauthorized", "Unauthorized."),
        ErrorKind.Forbidden => (StatusCodes.Status403Forbidden, "forbidden", "Forbidden."),
        ErrorKind.Unexpected => (StatusCodes.Status500InternalServerError, "internal", "An unexpected error occurred."),
        ErrorKind.Failure => (StatusCodes.Status400BadRequest, "failure", "Request failed."),
        _ => (StatusCodes.Status500InternalServerError, "internal", "An unexpected error occurred.")
    };
}