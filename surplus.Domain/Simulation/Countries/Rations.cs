namespace Surplus.Domain.Simulation.Countries;

/// <summary>
/// A country's day of provisioning, class by class. <see cref="StoreShare" />
/// is the share of the claims after the appropriators' that the store could
/// meet; below one, the store itself fell short.
/// </summary>
public sealed record Rations(IReadOnlyList<Ration> Each, decimal StoreShare)
{
  #region Properties

  /// <summary>Those with no title of their own, relieved or not.</summary>
  public int Workless => Each.Sum(ration => ration.Workless);

  public bool StoreFellShort => StoreShare < 1m;

  #endregion

  #region Methods

  public bool Equals(Rations? other)
  {
    return other is not null && StoreShare == other.StoreShare && Each.SequenceEqual(other.Each);
  }

  public override int GetHashCode()
  {
    return Each.Aggregate(StoreShare.GetHashCode(), HashCode.Combine);
  }

  #endregion
}
