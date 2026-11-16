namespace Surplus.Domain.Simulation.Society;

/// <summary>
/// The title a class's revenue gives it to the means of subsistence. Nobody in
/// a class society eats simply because they are hungry: they eat by a title,
/// and the title is the form their revenue takes.
/// </summary>
public enum Access
{
  /// <summary>Every head, served before anyone else: the surplus product is theirs first.</summary>
  First,

  /// <summary>Every head, whether or not the model has set it to work in a branch.</summary>
  Whole,

  /// <summary>One basket for each hand actually employed, and nothing for the rest.</summary>
  Employed,

  /// <summary>No title at all.</summary>
  None
}
