namespace LapBox.Contracts.Reviews;

public sealed record ReviewResponse(
    Guid Id,
    Guid LaptopId,
    string UserName,
    int Rating,
    string Comment,
    DateTimeOffset CreatedOnUtc);

public sealed record AddReviewRequest(
    Guid LaptopId,
    int Rating,
    string Comment);

public sealed record GetLaptopReviewsRequest(Guid LaptopId);
