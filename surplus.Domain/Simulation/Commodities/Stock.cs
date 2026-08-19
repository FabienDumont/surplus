using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Domain.Simulation.Commodities;

/// <summary>
/// A mass of one commodity held somewhere: so many yards of linen, so many
/// quarters of wheat.
/// What the heap is worth is deliberately not stored here. Value is no property
/// of the mass but of the labour society currently needs to reproduce it, so
/// <see cref="ValuedAt" /> must ask the commodity rather than answer alone —
/// a granary is worth less the morning after a harvest that cost less labour,
/// without a single grain having changed.
/// Immutable: stock is not edited in place, it is accumulated or consumed into
/// another stock.
/// </summary>
public sealed record Stock
{
  #region Properties

  public CommodityId Commodity { get; }
  public Quantity Quantity { get; }

  public bool IsExhausted => Quantity.IsNone;

  #endregion

  #region Ctors

  private Stock(CommodityId commodity, Quantity quantity)
  {
    Commodity = commodity;
    Quantity = quantity;
  }

  #endregion

  #region Methods

  public static Stock Of(CommodityId commodity, Quantity quantity)
  {
    return new Stock(commodity, quantity);
  }

  /// <summary>An empty store, still counted in the unit its commodity is measured by.</summary>
  public static Stock EmptyOf(CommodityId commodity, UnitOfMeasure unit)
  {
    return new Stock(commodity, Quantity.NoneOf(unit));
  }

  /// <summary>What is laid up here on top of what was already held.</summary>
  public Stock Accumulated(Quantity quantity)
  {
    return new Stock(Commodity, Quantity + quantity);
  }

  /// <summary>
  /// What is taken out of the store, whether it goes into a new product or into
  /// a labourer's stomach. Nothing can be consumed that is not there.
  /// </summary>
  public Stock Consumed(Quantity quantity)
  {
    return new Stock(Commodity, Quantity - quantity);
  }

  /// <summary>
  /// The value this mass amounts to, at the labour its reproduction costs today.
  /// The commodity is asked for that figure because it is the only thing that
  /// holds it.
  /// </summary>
  public Value ValuedAt(Commodity commodity)
  {
    if (commodity.Id != Commodity)
    {
      throw new DomainException($"This stock is not made of {commodity.Name}.");
    }

    if (commodity.UseValue.Unit != Quantity.Unit)
    {
      throw new DomainException(
        $"{commodity.Name} is measured in {commodity.UseValue.Unit}, not in {Quantity.Unit}."
      );
    }

    return commodity.Value * Quantity.Amount;
  }

  public override string ToString()
  {
    return $"{Quantity} in stock";
  }

  #endregion
}
