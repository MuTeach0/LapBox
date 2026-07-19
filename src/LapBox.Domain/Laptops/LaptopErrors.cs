using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Laptops;
public static class LaptopErrors
{
    public static readonly Error BrandIdRequired =
        Error.Validation("Laptop.BrandId", "Brand ID is required.");

    public static readonly Error CategoryIdRequired =
        Error.Validation("Laptop.CategoryId", "Category ID is required.");

    public static readonly Error NameRequired =
        Error.Validation("Laptop.Name", "Name cannot be empty.");

    public static readonly Error PriceInvalid =
        Error.Validation("Laptop.Price", "Price must be greater than zero.");

    public static readonly Error InventoryNegative =
        Error.Validation("Laptop.Inventory", "Inventory cannot be negative.");

    public static readonly Error InsufficientInventory =
        Error.Conflict("Laptop.Inventory", "Insufficient inventory for this laptop.");

    // 🛠️ تم التعديل: إصلاح خطأ الـ Copy-Paste وتغييره لـ Validation
    public static readonly Error SpecificationRequired =
        Error.Validation("Laptop.Specification", "Specification details are required.");
    public static readonly Error SkuRequired =
        Error.Validation("Laptop.Sku", "SKU is required.");

    // 🚀 الأخطاء الناقصة: الخاصة بالـ Specification Value Object
    public static readonly Error ProcessorRequired =
        Error.Validation("Specification.Processor", "Processor specification cannot be empty.");

    public static readonly Error RamRequired =
        Error.Validation("Specification.RAM", "RAM specification cannot be empty.");

    public static readonly Error StorageRequired =
        Error.Validation("Specification.Storage", "Storage specification cannot be empty.");

    public static readonly Error ScreenSizeRequired =
        Error.Validation("Specification.ScreenSize", "Screen size specification cannot be empty.");

    public static readonly Error GraphicsCardRequired =
        Error.Validation("Specification.GraphicsCard", "Graphics card specification cannot be empty.");

    public static readonly Error AlreadyDeactivated =
        Error.Validation("Laptop.AlreadyDeactivated", "This laptop is already deactivated.");
}