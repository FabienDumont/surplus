namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>
/// A province had goods to send where they were wanted, and not the carts to
/// send them: the want stayed where it was for lack of carriage, not of goods.
/// </summary>
public sealed record CarriageFellShort(string Country, string Province, string Commodity) : Occurrence
{
  #region Methods

  public override string ToString()
  {
    return $"{Province} had not the carts to send {Commodity} where it was wanted ({Country}).";
  }

  #endregion
}
