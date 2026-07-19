using LapBox.Domain.Common.Results;

namespace LapBox.Application.Common.Errors;

public static class ApplicationErrors
{
    // ==========================================
    // 1. Not Found Errors (Entities)
    // ==========================================
    public static Error CustomerNotFound => Error.NotFound(
        "ApplicationErrors.Customer.NotFound",
        "Customer does not exist.");

    public static Error LaptopNotFound => Error.NotFound(
        "ApplicationErrors.Laptop.NotFound",
        "Laptop does not exist.");

    public static Error BrandNotFound => Error.NotFound(
        "ApplicationErrors.Brand.NotFound",
        "Brand does not exist.");

    public static Error CategoryNotFound => Error.NotFound(
        "ApplicationErrors.Category.NotFound",
        "Category does not exist.");

    public static Error OrderNotFound => Error.NotFound(
        "ApplicationErrors.Order.NotFound",
        "Order does not exist.");

    public static Error CartNotFound => Error.NotFound(
        "ApplicationErrors.Cart.NotFound",
        "Cart does not exist.");

    public static Error InvoiceNotFound => Error.NotFound(
        "ApplicationErrors.Invoice.NotFound",
        "Invoice does not exist.");

    public static Error PromotionNotFound => Error.NotFound(
        "ApplicationErrors.Promotion.NotFound",
        "Promotion does not exist.");

    public static Error CustomerHasOrders => Error.Conflict(
        "ApplicationErrors.Customer.HasOrders",
        "Cannot delete a customer with existing orders.");

    // ==========================================
    // 2. Application Business Rules & Conflicts
    // ==========================================
    public static Error CartIsEmpty => Error.Conflict(
        "ApplicationErrors.Cart.Empty",
        "Cannot proceed to checkout with an empty cart.");

    public static Error OrderMustBeDeliveredForReview => Error.Conflict(
        "ApplicationErrors.Review.InvalidOrderState",
        "You can only review a laptop after the order has been delivered.");

    public static Error PromotionNotEligible(string code) => Error.Conflict(
        "ApplicationErrors.Promotion.NotEligible",
        $"The promotion code '{code}' is expired, inactive, or not eligible.");

    public static Error InsufficientStockForCheckout(string laptopName) => Error.Conflict(
        "ApplicationErrors.Checkout.InsufficientStock",
        $"Not enough stock available for laptop: {laptopName}.");

    public static Error LaptopAlreadyExists(string Sku) => Error.Conflict(
        "ApplicationErrors.Laptop.SkuAlreadyExists",
        $"A laptop with SKU '{Sku}' already exists.");


    // 🎯 الأخطاء الجديدة الخاصة بالـ Brand الموحدة هنا:
    public static Error BrandNameAlreadyExists(string name) => Error.Conflict(
        "ApplicationErrors.Brand.DuplicateName",
        $"A brand with the name '{name}' already exists.");

    public static Error CategoryNameAlreadyExists(string name) => Error.Conflict(
        "ApplicationErrors.Category.DuplicateName",
        $"A category with the name '{name}' already exists.");

    public static Error BrandHasActiveLaptops => Error.Conflict(
        "ApplicationErrors.Brand.HasActiveLaptops",
        "Cannot deactivate or delete a brand that still has active laptops associated with it.");

    public static Error CategoryHasActiveLaptops => Error.Conflict(
        "ApplicationErrors.Category.HasActiveLaptops",
        "Cannot deactivate or delete a category that still has active laptops associated with it.");

    // ==========================================
    // 3. Authentication & Identity Errors
    // ==========================================
    public static Error InvalidRefreshToken => Error.Validation(
        "Auth.RefreshToken.InvalidExpiry",
        "Expiry must be in the future.");

    public static readonly Error ExpiredAccessTokenInvalid = Error.Conflict(
        code: "Auth.ExpiredAccessToken.Invalid",
        description: "Expired access token is not valid.");

    public static readonly Error UserIdClaimInvalid = Error.Conflict(
        code: "Auth.UserIdClaim.Invalid",
        description: "Invalid userId claim.");

    public static readonly Error RefreshTokenExpired = Error.Conflict(
        code: "Auth.RefreshToken.Expired",
        description: "Refresh token is invalid or has expired.");

    public static readonly Error UserNotFound = Error.NotFound(
        code: "Auth.User.NotFound",
        description: "User not found.");

    public static readonly Error TokenGenerationFailed = Error.Failure(
        code: "Auth.TokenGeneration.Failed",
        description: "Failed to generate new JWT token.");

    public static readonly Error InvalidCredentials = Error.Conflict(
        code: "Auth.Credentials.Invalid",
        description: "Invalid email or password.");

    // ==========================================
    // 4. Authorization Errors
    // ==========================================
    public static Error OrderUnauthorized => Error.Unauthorized(
        "ApplicationErrors.Order.Unauthorized",
        "You do not have permission to modify or cancel this order.");

    public static Error OrderCannotCancel => Error.Conflict(
        code: "ApplicationErrors.Order.CannotCancel",
        description: "The order cannot be cancelled in its current state or because the cancellation window has closed.");

    public static Error Unauthorized => Error.Unauthorized(
        "ApplicationErrors.Unauthorized",
        "You are not authorized to access this resource.");
}