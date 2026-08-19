using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// What one period of production leaves behind: a heap of use-values, and an
/// account of the value that went into it.
/// The value carried here is the branch's <em>individual</em> value — what one
/// unit actually cost these producers, with these means, at this productivity.
/// It is emphatically not what the unit is worth. Worth is settled socially,
/// across every producer supplying the same market, and no single yield can
/// pronounce on it.
/// The gap between the two is the whole motor of the thing. Produce below the
/// social average and the difference falls to you as surplus profit; produce
/// above it and part of your labour counted for nothing. That is the goad that
/// puts machinery into a workshop, and the machinery, generalised, is what
/// lowers the social average again and takes the surplus profit back.
/// </summary>
public sealed record Yield
{
  #region Properties

  /// <summary>The product of the period, in its own natural unit.</summary>
  public Stock Product { get; }

  /// <summary>c + v + s, as this period actually decomposed.</summary>
  public ValueComposition Composition { get; }

  /// <summary>The labour one unit cost this branch — its individual value.</summary>
  public Value IndividualValue { get; }

  #endregion

  #region Ctors

  private Yield(Stock product, ValueComposition composition, Value individualValue)
  {
    Product = product;
    Composition = composition;
    IndividualValue = individualValue;
  }

  #endregion

  #region Methods

  public static Yield Of(Stock product, ValueComposition composition)
  {
    if (product.IsExhausted)
    {
      throw new DomainException("A period of production that leaves nothing behind has produced nothing.");
    }

    return new Yield(product, composition, composition.Product / product.Quantity.Amount);
  }

  public override string ToString()
  {
    return $"{Product.Quantity}, each holding {IndividualValue.Magnitude} ({Composition})";
  }

  #endregion
}
