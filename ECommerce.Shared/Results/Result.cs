namespace ECommerce.Shared.Results;

public class Result
{
    public bool Succeeded { get; }
    public string? Error { get; }
    public IReadOnlyList<string> Errors { get; }

    protected Result(bool succeeded, IEnumerable<string>? errors = null)
    {
        Succeeded = succeeded;
        Errors = errors?.ToList() ?? new List<string>();
        Error = Errors.FirstOrDefault();
    }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, new[] { error });
    public static Result Failure(IEnumerable<string> errors) => new(false, errors);
}

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool succeeded, T? data, IEnumerable<string>? errors = null)
        : base(succeeded, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, data);
    public new static Result<T> Failure(string error) => new(false, default, new[] { error });
    public new static Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);
}
