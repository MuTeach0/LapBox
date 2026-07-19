using LapBox.Application.Features.Customers.DTOs;
using LapBox.Domain.Customers;
using LapBox.Domain.Customers.ValueObjects;

namespace LapBox.Application.Features.Customers.Mappers;

public static class CustomerMapper
{
    public static CustomerDTO ToDTO(this Customer customer) =>new(
            customer.Id,
            customer.Name!,
            customer.Email!,
            customer.PhoneNumber,
            customer.TotalOrdersCount,
            customer.IsActiveCustomer,
            customer.Addresses.ToDTOs()
        );
    public static List<CustomerDTO> ToDTOs(this IEnumerable<Customer> entities) => [.. entities.Select(e => e.ToDTO())];
    public static AddressDTO ToDTO(this Address address) => new(address.Street, address.City, address.Country);
    public static List<AddressDTO> ToDTOs(this IEnumerable<Address> address) => [.. address.Select(e => e.ToDTO())];

}