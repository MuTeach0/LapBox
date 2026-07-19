using LapBox.Application.Features.Reviews.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Reviews.Queries.GetLaptopReviews;

public sealed record GetLaptopReviewsQuery(Guid LaptopId) : IRequest<Result<IReadOnlyCollection<ReviewDTO>>>;