namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>
/// A war ended, on the terms the side that proposed the peace could get. The
/// terms are told as a clause, since a cession names provinces and an
/// annexation names a state that is no more. For a white peace nobody won, and
/// the one who proposed it stands first.
/// </summary>
public sealed record PeaceWasMade(string Winner, string Loser, string Terms) : Occurrence
{
  #region Methods

  public override string ToString()
  {
    return $"{Winner} and {Loser} have made peace: {Terms}.";
  }

  #endregion
}
