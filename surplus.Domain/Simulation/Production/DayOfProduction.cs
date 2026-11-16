namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// What a country's branches came to over one day: the yields of those that
/// worked, and the branches that could not, for want of the means to work
/// with. The yields are individual, each carrying what it cost the branch that
/// made it; what any of it is worth is settled afterwards, across the world.
/// </summary>
public sealed record DayOfProduction(IReadOnlyList<Yield> Yields, IReadOnlyList<IdleBranch> Idle)
{
  #region Methods

  public bool Equals(DayOfProduction? other)
  {
    return other is not null
           && Yields.SequenceEqual(other.Yields)
           && Idle.SequenceEqual(other.Idle);
  }

  public override int GetHashCode()
  {
    return Yields.Cast<object>().Concat(Idle).Aggregate(0, HashCode.Combine);
  }

  #endregion
}
