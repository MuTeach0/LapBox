namespace LapBox.Application.Features.Reviews.DTOs;

public sealed record ReviewDTO(
    Guid Id,
    Guid LaptopId,
    string UserName,
    int Rating,
    string Comment,
    DateTimeOffset CreatedOnUtc);