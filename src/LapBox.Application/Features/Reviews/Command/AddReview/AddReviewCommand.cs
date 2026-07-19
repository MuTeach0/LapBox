using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Reviews.Command.AddReview;

public sealed record AddReviewCommand(Guid LaptopId, int Rating, string Comment) : IRequest<Result<Guid>>;