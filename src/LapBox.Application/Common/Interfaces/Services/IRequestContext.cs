namespace LapBox.Application.Common.Interfaces.Services;

public interface IRequestContext
{
    Guid? UserId { get; }
    string? UserEmail { get; }
    string? IpAddress { get; }
}