namespace Unify.People.Domain.Shared;

public sealed record DomainError(string Code, string Message);

public sealed record OperationResult(IReadOnlyCollection<DomainError> Errors)
{
    public bool IsSuccess => Errors.Count == 0;

    public static OperationResult Success() => new([]);

    public static OperationResult Failure(string code, string message) =>
        new([new DomainError(code, message)]);
}
