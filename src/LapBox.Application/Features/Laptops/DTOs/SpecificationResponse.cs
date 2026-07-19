namespace LapBox.Application.Features.Laptops.DTOs;

public record SpecificationResponse(
    string Processor,
    string RAM,
    string Storage,
    string ScreenSize,
    string GraphicsCard);