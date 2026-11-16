using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Application.Settings;

/// <summary>
/// Translates the units the simulation counts in into the ones a player reads.
/// The simulation counts in metric units, one natural unit per commodity, and
/// never converts: cloth is woven, stored and consumed in metres whoever is
/// watching. Imperial units are a matter of display only. They are applied on
/// the way to the screen and taken off on the way back, so the world and its
/// save files are the same in every system of measures.
/// A quantity is divided by the factor and a value per unit multiplied by it,
/// so a heap is worth the same number of hours whichever way it is read. Units
/// with no imperial counterpart (a machine is a machine) are shown as they are.
/// </summary>
public static class Measures
{
  #region Fields

  /// <summary>How much of the metric unit one imperial unit makes.</summary>
  private static readonly Dictionary<string, (decimal Factor, UnitOfMeasure Imperial)> ToImperial = new()
  {
    ["m"] = (0.9144m, UnitOfMeasure.Of("yd")),
    ["kg"] = (0.45359237m, UnitOfMeasure.Of("lb")),
    // The long ton: 2,240 pounds.
    ["t"] = (1.0160469088m, UnitOfMeasure.Of("long ton"))
  };

  #endregion

  #region Methods

  /// <summary>The unit a quantity counted in <paramref name="unit" /> is shown in.</summary>
  public static UnitOfMeasure Shown(UnitOfMeasure unit, UnitSystem system)
  {
    return Conversion(unit, system)?.Imperial ?? unit;
  }

  /// <summary>A quantity as the player reads it.</summary>
  public static Quantity Shown(Quantity quantity, UnitSystem system)
  {
    return Conversion(quantity.Unit, system) is { } conversion
      ? Quantity.Of(quantity.Amount / conversion.Factor, conversion.Imperial)
      : quantity;
  }

  /// <summary>What one unit of the shown kind is worth, given what one simulation unit is worth.</summary>
  public static Value Shown(Value perUnit, UnitOfMeasure unit, UnitSystem system)
  {
    return Conversion(unit, system) is { } conversion ? perUnit * conversion.Factor : perUnit;
  }

  /// <summary>
  /// A quantity the player typed, in the units they read, taken back into the
  /// unit the simulation counts <paramref name="unit" /> in.
  /// </summary>
  public static Quantity Entered(decimal amount, UnitOfMeasure unit, UnitSystem system)
  {
    return Conversion(unit, system) is { } conversion
      ? Quantity.Of(amount * conversion.Factor, unit)
      : Quantity.Of(amount, unit);
  }

  private static (decimal Factor, UnitOfMeasure Imperial)? Conversion(UnitOfMeasure unit, UnitSystem system)
  {
    return system is UnitSystem.Imperial && ToImperial.TryGetValue(unit.Name, out var conversion) ? conversion : null;
  }

  #endregion
}
