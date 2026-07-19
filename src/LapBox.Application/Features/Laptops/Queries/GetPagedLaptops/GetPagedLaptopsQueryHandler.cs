using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Common.Models;
using LapBox.Application.Features.Laptops.DTOs;
using LapBox.Application.Features.Laptops.Mappers;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Laptops.Queries.GetPagedLaptops;

public class GetPagedLaptopsQueryHandler(
    ILogger<GetPagedLaptopsQueryHandler> logger,
    ILaptopRepository laptopRepository) : IRequestHandler<GetPagedLaptopsQuery, Result<PaginatedList<LaptopResponse>>>
{
    public async Task<Result<PaginatedList<LaptopResponse>>> Handle(GetPagedLaptopsQuery query, CancellationToken ct)
    {
        logger.LogInformation("Fetching paged laptops. Page: {Page}, Size: {PageSize}", query.Page, query.PageSize);

        // 1. استدعاء الميثود اللي لسه ضايفينها في الـ Repository فوق
        var (laptops, totalCount) = await laptopRepository.GetPagedAsync(
            query.SearchTerm,
            query.BrandId,
            query.Page,
            query.PageSize,
            ct);

        // 2. Mapping للـ DTOs
        var laptopResponses = laptops.Select(l => l.ToResponse()).ToList();

        // 3. 🚀 بناء الـ PaginatedList باستخدام الـ Properties بتاعتك بالظبط
        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        var paginatedResult = new PaginatedList<LaptopResponse>
        {
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Items = laptopResponses
        };

        return paginatedResult;
    }
}