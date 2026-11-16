namespace Surplus.Domain.Simulation.Commodities;

/// <summary>
/// What a commodity was worth before a period's production settled it, and
/// what it is worth after. The movement is the visible trace of a change in
/// the productive power of labour somewhere in the world, and it lands on
/// every heap of the commodity at once, whoever made it.
/// </summary>
public sealed record Revaluation(CommodityId Commodity, Value Before, Value After)
{
  #region Properties

  /// <summary>How far the value moved, against what it was: positive when it rose.</summary>
  public decimal RelativeChange => After.RatioTo(Before) - 1m;

  #endregion
}
