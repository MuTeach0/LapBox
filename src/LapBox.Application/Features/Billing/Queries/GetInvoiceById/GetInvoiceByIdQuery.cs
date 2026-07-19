using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Features.Billing.DTOs;
using LapBox.Domain.Common.Results;

namespace LapBox.Application.Features.Billing.Queries.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : ICachedQuery<Result<InvoiceDTO>>
{
    public string CacheKey => $"invoice_{InvoiceId}";

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);

    public string[] Tags => ["invoice"];
}