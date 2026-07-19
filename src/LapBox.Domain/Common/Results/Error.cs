namespace LapBox.Domain.Common.Results;

public readonly record struct Error
{
    private Error(string code, string description, ErrorKind type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }
    public string Description { get; }
    public ErrorKind Type { get; }

    public static Error Failure(string code = "Failure", string description = "General failure.")
        => new(code, description, ErrorKind.Failure);

    public static Error Unexpected(string code = "Unexpected", string description = "Unexpected error.")
        => new(code, description, ErrorKind.Unexpected);

    public static Error Validation(string code = "Validation", string description = "Validation error.")
        => new(code, description, ErrorKind.Validation);

    public static Error Conflict(string code = "Conflict", string description = "Conflict error.")
        => new(code, description, ErrorKind.Conflict);

    public static Error NotFound(string code = "NotFound", string description = "Not found error.")
        => new(code, description, ErrorKind.NotFound);

    public static Error Unauthorized(string code = "Unauthorized", string description = "Unauthorized error.")
        => new(code, description, ErrorKind.Unauthorized);

    public static Error Forbidden(string code = "Forbidden", string description = "Forbidden error.")
        => new(code, description, ErrorKind.Forbidden);

    public static Error Create(int type, string code, string description)
        => new(code, description, (ErrorKind)type);
}