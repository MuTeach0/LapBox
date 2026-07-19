using LapBox.Domain.Catalog.Events.CategoryEvents;
using LapBox.Domain.Common;
using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Catalog;

public sealed class Category : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private Category() { } // من أجل الـ EF Core

    private Category(Guid id, string name, string description) : base(id)
    {
        Name = name;
        Description = description;
        IsActive = true;
    }

    // Factory Method للإنشاء الآمن المتوافق مع الـ Result Pattern بتاعك
    public static Result<Category> Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return CategoryErrors.NameRequired;

        var category = new Category(Guid.NewGuid(), name, description);
        category.AddDomainEvent(new CategoryCreatedEvent(category.Id));

        return category;
    }

    // لتحديث بيانات التصنيف لاحقاً
    public Result<Success> Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return CategoryErrors.NameRequired;

        Name = name;
        Description = description;

        AddDomainEvent(new CategoryUpdatedEvent(Id));

        return Result.Success;
    }

    // لتعطيل التصنيف (مثال: لو مش عاوزين نعرض لابات الـ Gaming مؤقتاً في الموقع)
    public Result<Success> Deactivate()
    {
        if (!IsActive)
            return CategoryErrors.AlreadyDeactivated;

        IsActive = false;

        AddDomainEvent(new CategoryDeactivatedEvent(Id));

        return Result.Success;
    }
}