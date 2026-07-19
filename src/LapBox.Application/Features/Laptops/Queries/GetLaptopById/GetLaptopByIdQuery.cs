using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Features.Laptops.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Laptops.Queries.GetLaptopById;

public record GetLaptopByIdQuery(Guid LaptopId) : ICachedQuery<Result<LaptopResponse>>
{
    public string CacheKey => $"laptops:{LaptopId}";

    public string[] Tags => ["laptops"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(60); // نفس مدة الـ Customers أو حسب البزنس   
}