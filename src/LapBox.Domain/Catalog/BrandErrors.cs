using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Catalog;

public static class BrandErrors
{
    public static readonly Error NameRequired =
        Error.Validation("Brand.Name", "Brand name cannot be empty.");

    public static readonly Error AlreadyDeactivated =
        Error.Validation("Brand.AlreadyDeactivated", "This brand is already deactivated.");

    public static readonly Error BrandNotFound =
        Error.NotFound("Brand.NotFound", "The requested brand was not found.");
}