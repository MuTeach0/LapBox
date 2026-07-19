namespace LapBox.Contracts.Customers;

public sealed record AddressResponse(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country);

public sealed record CustomerResponse(
    Guid Id,
    string Name,
    string Email,
    string? PhoneNumber,
    int TotalOrdersCount,
    bool IsActiveCustomer,
    IReadOnlyList<AddressResponse> Addresses);

public sealed record CreateCustomerRequest(
    Guid IdentityId,
    string Name,
    string Email,
    string? PhoneNumber,
    AddressRequest? Address);

public sealed record UpdateCustomerRequest(
    string Name,
    string Email,
    string? PhoneNumber);

public sealed record UpdateCustomerAddressRequest(
    AddressRequest Address);

public sealed record AddressRequest(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country);
