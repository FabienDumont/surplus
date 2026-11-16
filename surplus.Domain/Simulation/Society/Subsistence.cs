using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;

namespace Surplus.Domain.Simulation.Society;

/// <summary>
/// What one person must have in one day to be there the next: the means of
/// subsistence whose value is the value of labour-power.
/// The basket is customary, not physiological. Marx is explicit that the
/// labourer's necessary wants "are themselves the product of historical
/// development", so that the value of labour-power contains "a historical and
/// moral element". That is why it is data handed to the world and not a
/// constant of the model: what a people will not go without is something it has
/// won, and can lose.
/// </summary>
public sealed record Subsistence
{
  #region Fields

  private readonly List<Stock> _perHead;

  #endregion

  #region Properties

  /// <summary>One head's share for one day.</summary>
  public IReadOnlyList<Stock> PerHead => _perHead;

  #endregion

  #region Ctors

  private Subsistence(List<Stock> perHead)
  {
    _perHead = perHead;
  }

  #endregion

  #region Methods

  public static Subsistence Of(params Stock[] perHead)
  {
    if (perHead.Length == 0)
    {
      throw new DomainException("A basket with nothing in it keeps no one alive.");
    }

    if (perHead.GroupBy(stock => stock.Commodity).Any(group => group.Count() > 1))
    {
      throw new DomainException("The same thing cannot be in the basket twice; put it in once, in the full amount.");
    }

    return new Subsistence([.. perHead]);
  }

  /// <summary>
  /// Reconstitutes a basket from a stored snapshot, asserting nothing. It
  /// exists for persistence and for test builders, which need a world whose
  /// people want for nothing.
  /// </summary>
  public static Subsistence Load(IEnumerable<Stock> perHead)
  {
    return new Subsistence([.. perHead]);
  }

  /// <summary>The basket so many times over.</summary>
  public IReadOnlyList<Stock> For(int heads)
  {
    if (heads < 0)
    {
      throw new DomainException("There is no such thing as a negative number of mouths to feed.");
    }

    return [.. _perHead.Select(stock => Stock.Of(stock.Commodity, stock.Quantity * heads))];
  }

  public bool Equals(Subsistence? other)
  {
    return other is not null && _perHead.SequenceEqual(other._perHead);
  }

  public override int GetHashCode()
  {
    return _perHead.Aggregate(0, HashCode.Combine);
  }

  #endregion
}
