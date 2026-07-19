using LapBox.Application.Features.Reviews.DTOs;
using LapBox.Domain.Reviews;

namespace LapBox.Application.Features.Reviews.Mapper;

public static class ReviewMapper
{
    public static ReviewDTO ToDTO(this Review review) => new(
        review.Id,
        review.LaptopId,
        "User_" + review.UserId.ToString()[..4], // الـ Masking المؤقت لحد ربط الـ Identity
        review.Rating.Value,                     // فك الـ Value Object لـ int
        review.Comment,
        DateTimeOffset.UtcNow                    // تاريخ اللحظة الحالية (أو استبدله بـ review.CreatedOnUtc لو ضفته في الدومين)
    );

    public static List<ReviewDTO> ToDTOs(this IEnumerable<Review> reviews) => 
        [.. reviews.Select(r => r.ToDTO())];
}