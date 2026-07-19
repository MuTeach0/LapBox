using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Reviews.DTOs;
using LapBox.Application.Features.Reviews.Mapper;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Reviews.Queries.GetLaptopReviews;

public sealed class GetLaptopReviewsQueryHandler(IReviewRepository reviewRepository) 
    : IRequestHandler<GetLaptopReviewsQuery, Result<IReadOnlyCollection<ReviewDTO>>>
{
    public async Task<Result<IReadOnlyCollection<ReviewDTO>>> Handle(GetLaptopReviewsQuery request, CancellationToken ct)
    {
       // 1️⃣ جلب قائمة التقييمات الخاصة باللابتوب من الـ Repository
        var reviews = await reviewRepository.GetByLaptopIdAsync(request.LaptopId, ct);

        return reviews.ToDTOs();
    }
}