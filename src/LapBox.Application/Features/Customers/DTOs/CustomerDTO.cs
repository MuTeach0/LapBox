namespace LapBox.Application.Features.Customers.DTOs;

public sealed record CustomerDTO(
    Guid Id,
    string Name,
    string Email,
    string? PhoneNumber,
    int TotalOrdersCount,
    bool IsActiveCustomer,
    List<AddressDTO> Addresses);