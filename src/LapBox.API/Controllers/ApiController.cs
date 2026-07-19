using LapBox.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LapBox.API.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    protected ActionResult Problem(List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return Problem();
        }

        // إذا كانت كل الأخطاء من نوع Validation، نرجع ValidationProblem
       // if (errors.All(error => error.Type.ToString() == "Validation")) // قم بتعديل "Validation" حسب الـ Enum الخاص بك
        if (errors.All(error => error.Type == ErrorKind.Validation))
        {
            return ValidationProblem(errors);
        }

        return Problem(errors[0]);
    }

    protected ObjectResult Problem(Error error)
    {
        // تحديد الـ Status Code بناءً على نوع الخطأ
        var statusCode = error.Type switch
        {
            ErrorKind.Conflict  => StatusCodes.Status409Conflict,
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Problem(
            statusCode: statusCode,
            title: error.Description,
            type: $"https://lapbox.com/errors/{error.Type}",
            extensions: new Dictionary<string, object?> { { "code", error.Code } }
        );
    }

    private ActionResult ValidationProblem(List<Error> errors)
    {
        var modelStateDictionary = new ModelStateDictionary();

        errors.ForEach(error => modelStateDictionary.AddModelError(error.Code, error.Description));

        return ValidationProblem(modelStateDictionary);
    }
}