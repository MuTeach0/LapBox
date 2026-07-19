using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Laptops.ValueObjects;

public record Specification
{
    public string Processor { get; private set; } = string.Empty;
    public string RAM { get; private set; } = string.Empty;
    public string Storage { get; private set; } = string.Empty;
    public string ScreenSize { get; private set; } = string.Empty;
    public string GraphicsCard { get; private set; } = string.Empty;

    private Specification() { } // For EF Core

    private Specification(string processor, string ram, string storage, string screenSize, string graphicsCard)
    {
        Processor = processor;
        RAM = ram;
        Storage = storage;
        ScreenSize = screenSize;
        GraphicsCard = graphicsCard;
    }

    // 🚀 الـ Factory Method الخاصة بالـ Value Object نفسه
    public static Result<Specification> Create(string processor, string ram, string storage, string screenSize, string graphicsCard)
    {
        if (string.IsNullOrWhiteSpace(processor)) return LaptopErrors.ProcessorRequired;
        if (string.IsNullOrWhiteSpace(ram)) return LaptopErrors.RamRequired;
        if (string.IsNullOrWhiteSpace(storage)) return LaptopErrors.StorageRequired;
        if (string.IsNullOrWhiteSpace(screenSize)) return LaptopErrors.ScreenSizeRequired;
        if (string.IsNullOrWhiteSpace(graphicsCard)) return LaptopErrors.GraphicsCardRequired;

        return new Specification(processor, ram, storage, screenSize, graphicsCard);
    }
}