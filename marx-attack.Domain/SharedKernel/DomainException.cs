namespace MarxAttack.Domain.SharedKernel;

/// <summary>
/// Raised when a domain invariant is violated.
/// </summary>
public class DomainException(string message) : Exception(message);
