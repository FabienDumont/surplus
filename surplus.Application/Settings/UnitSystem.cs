namespace Surplus.Application.Settings;

/// <summary>The system of measures a player reads quantities in.</summary>
public enum UnitSystem
{
  /// <summary>Metres, kilograms and tonnes: the units the simulation itself counts in.</summary>
  Metric,

  /// <summary>Yards, pounds and long tons, converted for display.</summary>
  Imperial
}
