namespace LapBox.Domain.Orders.ValueObjects;

public record ShippingAddress(string Street, string City, string State, string ZipCode, string Country);