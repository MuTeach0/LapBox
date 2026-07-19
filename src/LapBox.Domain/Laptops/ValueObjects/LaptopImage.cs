namespace LapBox.Domain.Laptops.ValueObjects;

public sealed record LaptopImage(string Url, bool IsMain, int DisplayOrder);