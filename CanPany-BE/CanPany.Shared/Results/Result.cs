namespace CanPany.Shared.Results;

/// <summary>
/// Result pattern for functional error handling
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }
    
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Successful result cannot have an error");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failed result must have an error");
        
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(Error error) => new(default!, false, error);
}

public class Result<T> : Result
{
    public T Value { get; }
    
    internal Result(T value, bool isSuccess, Error? error) : base(isSuccess, error)
    {
        Value = value;
    }
    
    public static implicit operator Result<T>(T value) => Success(value);
}

public class Error
{
    public string Code { get; }
    public string Message { get; }
    
    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }
    
    public static Error None => new(string.Empty, string.Empty);
    public static Error NotFound(string entity) => new("NOT_FOUND", $"{entity} not found");
    public static Error Validation(string message) => new("VALIDATION_ERROR", message);
    public static Error Unauthorized() => new("UNAUTHORIZED", "Unauthorized access");
    public static Error Forbidden() => new("FORBIDDEN", "Forbidden access");
}

