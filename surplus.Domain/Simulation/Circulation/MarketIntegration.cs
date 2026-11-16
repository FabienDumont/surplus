using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Circulation;

/// <summary>
/// How freely each mode of production lets go of its surplus: the share of
/// what a province holds beyond its own needs that it sends across a border in
/// a day. These are tuned figures, not measurements, and only their ratios
/// matter. Kept as one table, like <see cref="Production.ProductiveForms" />.
/// </summary>
public static class MarketIntegration
{
  #region Fields

  private static readonly Dictionary<ModeOfProduction, decimal> Shares = new()
  {
    // Exchange begins where communities end, at their edges, and not inside them.
    [ModeOfProduction.PrimitiveCommunal] = 0.01m,

    // Once slave production is drawn into a world market dominated by capital,
    // the plantation is overworked for export (Capital I, ch. 10 §2).
    [ModeOfProduction.Slave] = 0.3m,

    // The manor lives largely on its own produce; what leaves it leaves as rent in kind.
    [ModeOfProduction.Feudal] = 0.05m,

    // Production for sale: every good goes wherever it is wanted. Capital is
    // what welds provinces into one market.
    [ModeOfProduction.Capitalist] = 0.5m,

    // Set equal to capital's until there is a model of planning to replace them.
    [ModeOfProduction.Socialist] = 0.5m,
    [ModeOfProduction.Communist] = 0.5m
  };

  #endregion

  #region Methods

  /// <summary>The share of its surplus a province under this mode sends across a border in a day.</summary>
  public static decimal Integration(this ModeOfProduction mode)
  {
    return Shares.TryGetValue(mode, out var share)
      ? share
      : throw new DomainException($"No integration is registered for {mode}.");
  }

  #endregion
}
