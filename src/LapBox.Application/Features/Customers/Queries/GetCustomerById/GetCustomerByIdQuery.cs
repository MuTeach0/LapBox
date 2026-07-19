using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Features.Customers.DTOs;
using LapBox.Domain.Common.Results;

namespace LapBox.Application.Features.Customers.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid Id) : ICachedQuery<Result<CustomerDTO>>
{
    public string CacheKey => $"customer_{Id}";
    public string[] Tags => ["customers"];
    public TimeSpan Expiration => TimeSpan.FromMinutes(60);
}