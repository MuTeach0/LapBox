using FluentValidation;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Common.Results.Abstractions;
using MediatR;

namespace LapBox.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // لو مفيش أي Validator للملف ده، عدي للـ Handler علطول
        if (!validators.Any())
        {
            return await next(ct);
        }

        // تشغيل كل الـ Validators المربوطة بالـ Request في نفس الوقت
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct))
        );

        // تجميع كل الأخطاء من كل الـ Validators واستبعاد السليم
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(ct);
        }

        // تحويل أخطاء FluentValidation إلى الـ Domain Error Object بتاعك
        var errors = failures.ConvertAll(error => Error.Validation(
            code: error.PropertyName,
            description: error.ErrorMessage));

        // 💡 التحديث 2: الـ dynamic هنا مقبولة وذكية، ولكن تأكد 100% 
        // إن الـ Result والـ Result<T> عندِك فيهم implicit operator لـ List<Error>
        return (dynamic)errors;
    }
}
// public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
//     : IPipelineBehavior<TRequest, TResponse>
//         where TRequest : IRequest<TResponse>
//         where TResponse : IResult
// {
//     private readonly IValidator<TRequest>? _validator = validator;

//     public async Task<TResponse> Handle(
//         TRequest request,
//         RequestHandlerDelegate<TResponse> next,
//         CancellationToken ct)
//     {
//         if (_validator is null)
//         {
//             return await next(ct);
//         }

//         var validationResult = await _validator.ValidateAsync(request, ct);

//         if (validationResult.IsValid)
//         {
//             return await next(ct);
//         }

//         var errors = validationResult.Errors
//             .ConvertAll(error => Error.Validation(
//                 code: error.PropertyName,
//                 description: error.ErrorMessage));

//         return (dynamic)errors;
//     }
// }