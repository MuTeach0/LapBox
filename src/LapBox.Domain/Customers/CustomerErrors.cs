using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Customers;

public static class CustomerErrors
{
    public static Error AddressRequired =>
        Error.Validation("Customer_Address_Required", "Address cannot be null.");

    public static Error AddressNotFound =>
        Error.Validation("Customer_Address_NotFound", "Address NotFound.");

    public static Error NameRequired =>
        Error.Validation("Customer_Name_Required", "Customer name is required");

    public static Error PhoneNumberRequired =>
        Error.Validation("Customer_Number_Required", "Phone number is required");

    public static Error EmailRequired =>
        Error.Validation("Customer_Email_Required", "Email is required");

    public static Error EmailInvalid =>
      Error.Validation("Customer_Email_Invalid", "Email is invalid");

    public static Error CustomerExists =>
        Error.Conflict("Customer_Email_Exists", "A customer with this email already exists.");

    public static readonly Error InvalidPhoneNumber =
        Error.Conflict("Customer.InvalidPhoneNumber", "Phone number must be 7–15 digits and may start with '+'.");

    public static readonly Error CannotDeleteCustomerWithOrders =
        Error.Conflict("Customer.CannotDelete", "Customer cannot be deleted due to existing orders.");
    public static Error DuplicateAddress =>
        Error.Conflict("Customer.DuplicateAddress", "This address already exists for the customer.");
    public static Error IdentityRequired =>
        Error.Validation("Customer.IdentityRequired", "IdentityId cannot be empty.");
}