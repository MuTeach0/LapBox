using LapBox.Domain.Common;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Laptops.Events;
using LapBox.Domain.Laptops.ValueObjects;

namespace LapBox.Domain.Laptops;

public sealed class Laptop : AggregateRoot
{
    public Guid BrandId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal BasePrice { get; private set; }
    public string Sku { get; private set; }
    public int InventoryQuantity { get; private set; }
    public Specification Specification { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private readonly List<LaptopImage> _images = [];
    public IReadOnlyCollection<LaptopImage> Images => _images.AsReadOnly();

    private Laptop() { } // For EF Core

    private Laptop(Guid id, Guid brandId, Guid categoryId, string name, string sku, string description, decimal basePrice, int inventoryQuantity, Specification specification) 
        : base(id)
    {
        BrandId = brandId;
        CategoryId = categoryId;
        Name = name;
        Sku = sku;
        Description = description;
        BasePrice = basePrice;
        InventoryQuantity = inventoryQuantity;
        Specification = specification;
        IsActive = true;
    }

   // Factory Method for Creation
    public static Result<Laptop> Create(Guid brandId, Guid categoryId, string name, string sku, string description, decimal basePrice, int inventoryQuantity, Specification specification)
    {
        if (brandId == Guid.Empty) return LaptopErrors.BrandIdRequired;
        if (categoryId == Guid.Empty) return LaptopErrors.CategoryIdRequired; // فحص الـ Category
        if (string.IsNullOrWhiteSpace(name)) return LaptopErrors.NameRequired;
        if (string.IsNullOrWhiteSpace(sku)) return LaptopErrors.SkuRequired; // 👈 فحص الـ Sku (تأكد من إضافة هذا الخطأ)
        if (basePrice <= 0) return LaptopErrors.PriceInvalid;
        if (inventoryQuantity < 0) return LaptopErrors.InventoryNegative;
        if (specification is null) return LaptopErrors.SpecificationRequired;
        return new Laptop(Guid.NewGuid(), brandId, categoryId, name, sku.ToUpper().Trim(), description, basePrice, inventoryQuantity, specification);
    }

    public Result<Success> Deactivate()
    {
        // بزنس رول: لو اللابتوب واقف فعلاً من قبل كده، مش محتاجين نوقفه تاني
        if (!IsActive)
            return LaptopErrors.AlreadyDeactivated; // تأكد من إضافة هذا الـ Error في كلاس الـ LaptopErrors عندك

        IsActive = false;

        // (اختياري): لو حابب ترمي Domain Event هنا عشان موديولات تانية تعرف إن المنتج وقف
        AddDomainEvent(new LaptopDeactivatedEvent(Id)); 

        return Result.Success;
    }

    // 🔥 الميثود الجديدة للـ Update
    public Result<Success> UpdateDetails(Guid brandId, Guid categoryId, string name, string description, Specification specification)
    {
        if (brandId == Guid.Empty) return LaptopErrors.BrandIdRequired;
        if (categoryId == Guid.Empty) return LaptopErrors.CategoryIdRequired;
        if (string.IsNullOrWhiteSpace(name)) return LaptopErrors.NameRequired;
        if (specification is null) return LaptopErrors.SpecificationRequired;
        BrandId = brandId;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Specification = specification;

        return Result.Success;
    }

    public void AddImage(string url, bool isMain, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        if (isMain)
        {
            // لو الصورة الجديدة هي الرئيسية، بنقلب أي صورة رئيسية قديمة لـ false
            _images.ConvertAll(img => img with { IsMain = false });
        }

        _images.Add(new LaptopImage(url, isMain, displayOrder));
    }

    public void RemoveImage(string url) => _images.RemoveAll(x => x.Url == url);

    public Result<Success> UpdateInventory(int quantity)
    {
        if (InventoryQuantity + quantity < 0) return LaptopErrors.InsufficientInventory;
        InventoryQuantity += quantity;
        return Result.Success;
    }

    public Result<Success> AdjustPrice(decimal newPrice)
    {
        if (newPrice <= 0) return LaptopErrors.PriceInvalid;

        var oldPrice = BasePrice;
        BasePrice = newPrice;

        AddDomainEvent(new LaptopPriceChangedEvent(Id, oldPrice, newPrice));
        return Result.Success;
    }
}