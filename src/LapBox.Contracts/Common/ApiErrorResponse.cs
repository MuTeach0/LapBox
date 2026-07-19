using System.Text.Json.Serialization;

namespace LapBox.Contracts.Common;

/// <summary>
/// Standard error response returned for all non-success HTTP responses.
/// </summary>
public class ApiErrorResponse
{
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public int Status { get; init; }
    public string Detail { get; init; } = string.Empty;
    public string Instance { get; init; } = string.Empty;
    public string? Code { get; init; }
    public string? RequestId { get; init; }

    [JsonExtensionData]
    public Dictionary<string, object>? Extensions { get; init; }
}
