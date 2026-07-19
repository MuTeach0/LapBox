using LapBox.Domain.Catalog.Events.BrandEvents;
using LapBox.Domain.Common;
using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Catalog;

public sealed class Brand : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? LogoUrl { get; private set; } // اختياري لوجو الماركة

    public bool IsActive { get; private set; } = true;
    private Brand() { } // من أجل الـ EF Core

    private Brand(Guid id, string name, string description, string? logoUrl) : base(id)
    {
        Name = name;
        Description = description;
        LogoUrl = logoUrl;
        IsActive = true;
    }

    // Factory Method للإنشاء الآمن بالـ Result Pattern
    public static Result<Brand> Create(string name, string description, string? logoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BrandErrors.NameRequired;
        var brand = new Brand(Guid.NewGuid(), name, description, logoUrl);
        brand.AddDomainEvent(new BrandCreatedEvent(brand.Id));

        return brand;
    }

    // لتحديث بيانات الماركة لاحقاً
    public Result<Success> Update(string name, string description, string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BrandErrors.NameRequired;

        Name = name;
        Description = description;
        LogoUrl = logoUrl;

        AddDomainEvent(new BrandUpdatedEvent(Id));

        return Result.Success;
    }

    public Result<Success> Deactivate()
    {
        if (!IsActive)
            return BrandErrors.AlreadyDeactivated;

        IsActive = false;

        // 🚀 ارمي Event هنا برضه للمستقبل لو حابب تخفي لابتوباته تلقائياً
        AddDomainEvent(new BrandDeactivatedEvent(Id));

        return Result.Success;
    }
}