using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces;
using LapBox.Application.Common.Interfaces.Identity;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Reviews;
using LapBox.Domain.Reviews.ValueObjects;
using MediatR;

namespace LapBox.Application.Features.Reviews.Command.AddReview;
public sealed class AddReviewCommandHandler(
    IUser currentUser,
    IOrderCheckService orderCheckService, 
    IReviewRepository reviewRepository) : IRequestHandler<AddReviewCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddReviewCommand command, CancellationToken ct)
    {
        // 1️⃣ التأكد من وجود الـ User ID في الـ Claims
        if (currentUser.Id is null) return ApplicationErrors.UserIdClaimInvalid;

        // 2️⃣ الـ Business Rule: هل العميل استلم اللابتوب ده فعلاً؟
        var hasDeliveredOrder = await orderCheckService.HasUserPurchasedLaptopAsync(currentUser.Id.Value, command.LaptopId, ct);
        if (!hasDeliveredOrder)
        {
            return ApplicationErrors.OrderMustBeDeliveredForReview; // 🎯 خطأك الموحد
        }

        // 3️⃣ إنشاء الـ Value Object (Rating) والتحقق من الـ Validation بتاعه
        var ratingResult = Rating.Create(command.Rating);
        if (ratingResult.IsError)
        {
            return ratingResult.Errors; // هيرجع خطأ الـ Rating (بين 1 و 5) لو بره الرينج
        }

        // 4️⃣ إنشاء الـ Aggregate Root (Review) بناءً على الـ Rating السليم
        var reviewResult = Review.Create(
            command.LaptopId, 
            currentUser.Id.Value, 
            ratingResult.Value, 
            command.Comment);

        if (reviewResult.IsError)
        {
            return reviewResult.Errors; // هيرجع خطأ الـ Comment لو فاضي
        }

        // 5️⃣ الحفظ في قاعدة البيانات عبر الـ Repository بتاعك
        await reviewRepository.AddAsync(reviewResult.Value, ct); 
        // 💡 ملحوظة: لو الـ Generic IRepository عندك ميثود الحفظ فيه اسمها Add بس، شيل Async.

        return reviewResult.Value.Id; // إرجاع الـ ID الخاص بالتقييم الجديد
    }
}