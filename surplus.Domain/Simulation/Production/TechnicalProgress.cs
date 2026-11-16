using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// How hard each mode of production drives the development of technique. In
/// Marx nobody researches technology: the productive forces develop out of the
/// labour process, and the relations it is carried on under decide how hard it
/// is pushed. An hour worked under capital counts for more toward the next
/// technique than an hour worked on the demesne, because capital is compelled to
/// cheapen its product and the lord is not.
/// These are tuned figures, not measurements, and only their ratios matter.
/// Kept as one table, like <see cref="ProductiveForms" />.
/// </summary>
public static class TechnicalProgress
{
  #region Fields

  private static readonly Dictionary<ModeOfProduction, decimal> Drives = new()
  {
    // Production for use and by custom: what works is handed down, not pushed.
    [ModeOfProduction.PrimitiveCommunal] = 0.02m,

    // The producer has no stake in the instrument, so tools are made crude
    // enough to survive the hands that are forced to use them.
    [ModeOfProduction.Slave] = 0.05m,

    // Rent is fixed by custom and taken by force; the lord gains little from
    // changing how the corvée is worked.
    [ModeOfProduction.Feudal] = 0.1m,

    // Competition compels each capital to cut the labour time its product
    // costs, or be undersold: the whole movement of relative surplus-value.
    [ModeOfProduction.Capitalist] = 1m,

    // Set equal to capital's until there is a model of planning to replace them.
    [ModeOfProduction.Socialist] = 1m,
    [ModeOfProduction.Communist] = 1m
  };

  #endregion

  #region Methods

  /// <summary>How much an hour worked under this mode counts toward finding the next technique.</summary>
  public static decimal Drive(this ModeOfProduction mode)
  {
    return Drives.TryGetValue(mode, out var drive)
      ? drive
      : throw new DomainException($"No drive is registered for {mode}.");
  }

  #endregion
}
