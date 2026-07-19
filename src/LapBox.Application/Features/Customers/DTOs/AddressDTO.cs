namespace LapBox.Application.Features.Customers.DTOs;

public sealed record AddressDTO(
    string Street,
    string City,
    string Country);