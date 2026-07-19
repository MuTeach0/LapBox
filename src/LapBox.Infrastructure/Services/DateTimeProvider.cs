using LapBox.Application.Common.Interfaces.Services;

namespace LapBox.Infrastructure.Services;

/// <summary>
/// Provides current UTC datetime - useful for testing and consistency
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
