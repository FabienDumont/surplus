namespace Surplus.Domain.SharedKernel.StrongIds;

/// <summary>Marker interface for the tag types that distinguish strong ids from one another.</summary>
public interface IStrongIdTag;

/// <summary>
/// A strongly-typed identifier backed by a Guid. The tag type gives each
/// aggregate its own incompatible id type, so a Game id can never be passed
/// where a Commodity id is expected.
/// </summary>
/// <typeparam name="TTag">Marker type associating the identifier with one aggregate.</typeparam>
public readonly record struct Id<TTag>(Guid Value) where TTag : IStrongIdTag
{
  #region Methods

  public static Id<TTag> New()
  {
    return new Id<TTag>(Guid.CreateVersion7());
  }

  public static Id<TTag> From(Guid value)
  {
    return value == Guid.Empty
      ? throw new DomainException($"{typeof(TTag).Name} cannot be empty.")
      : new Id<TTag>(value);
  }

  public override string ToString()
  {
    return Value.ToString();
  }

  #endregion
}
