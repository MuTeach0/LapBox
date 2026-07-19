using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Catalog;

public static class CategoryErrors
{
    public static readonly Error NameRequired =
        Error.Validation("Category.Name", "Category name cannot be empty.");

    public static readonly Error AlreadyDeactivated =
        Error.Validation("Category.AlreadyDeactivated", "This category is already deactivated.");

    public static readonly Error CategoryNotFound =
        Error.NotFound("Category.NotFound", "The requested category was not found.");
}