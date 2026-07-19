using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Common.Models;
using LapBox.Application.Features.Customers.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;
using LapBox.Application.Features.Customers.Mappers;

namespace LapBox.Application.Features.Customers.Queries.GetCustomers;
public class GetCustomersQueryHandler(ICustomerRepository repository)
    : IRequestHandler<GetCustomersQuery, Result<PaginatedList<CustomerDTO>>>
{
    public async Task<Result<PaginatedList<CustomerDTO>>> Handle(GetCustomersQuery query, CancellationToken ct)
    {
        // جلب البيانات من الـ Repository (مع الـ Pagination)
        var (customers, totalCount) = await repository.GetAllPaginatedAsync(query.Page, query.PageSize, ct);

        // التعديل هنا: استخدم ToDTOs() لأنك تتعامل مع قائمة
        var dtos = customers.ToDTOs();

        var result = new PaginatedList<CustomerDTO>
        {
            Items = dtos,
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return result;
    }
}