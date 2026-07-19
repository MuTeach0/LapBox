using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Common.Models;
using LapBox.Application.Features.Customers.DTOs;
using LapBox.Domain.Common.Results;

namespace LapBox.Application.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery(int Page, int PageSize)
: ICachedQuery<Result<PaginatedList<CustomerDTO>>>
{
    public string CacheKey => $"customers_page_{Page}_size_{PageSize}";

    public string[] Tags => ["customers"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
};