namespace Surplus.Application.Reporting;

/// <summary>How much an occurrence asks of the player's attention.</summary>
public enum Importance
{
  /// <summary>The ordinary course of things, worth a sum and no more.</summary>
  Routine,

  /// <summary>Worth reading, but no reason to stop the clock.</summary>
  Notable,

  /// <summary>Something the player may want to act on before another day passes.</summary>
  Urgent
}
