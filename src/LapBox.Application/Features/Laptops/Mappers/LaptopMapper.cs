using LapBox.Application.Features.Laptops.DTOs;
using LapBox.Domain.Laptops;
using LapBox.Domain.Laptops.ValueObjects;

namespace LapBox.Application.Features.Laptops.Mappers;

public static class LaptopMapper
{
    // 🚀 تحويل الـ Laptop Entity إلى LaptopResponse DTO
    public static LaptopResponse ToResponse(this Laptop laptop)
    {
        if (laptop is null) return null!;

        return new LaptopResponse(
            laptop.Id,
            laptop.BrandId,
            laptop.Name,
            laptop.Description,
            laptop.BasePrice,
            laptop.InventoryQuantity,
            laptop.Specification.ToResponse() // استدعاء ماب الـ Value Object بالأسفل
        );
    }

    // 💻 تحويل الـ Specification Value Object إلى SpecificationResponse DTO
    public static SpecificationResponse ToResponse(this Specification spec)
    {
        if (spec is null) return null!;

        return new SpecificationResponse(
            spec.Processor,
            spec.RAM,
            spec.Storage,
            spec.ScreenSize,
            spec.GraphicsCard
        );
    }
}