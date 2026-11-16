using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Commodities;

/// <summary>
/// Everything a country has laid up: the products of past labour lying ready to
/// be consumed, whether productively, as the material and instruments of new
/// labour, or individually, as the means of life of the labourers.
/// It is the physical side of reproduction and nothing more. What it holds is
/// counted in use-values, one heap per commodity, and never in value — the same
/// heap is worth whatever the labour to reproduce it costs today, and the
/// register is the one that knows that.
/// Immutable, like the stocks in it: a store is not edited, it is drawn on or
/// added to, and becomes another store. Nothing can be drawn that is not there,
/// and a draw that cannot be met in full takes nothing at all, since half the
/// yarn for a day's weaving weaves no cloth.
/// </summary>
public sealed record Store
{
  #region Fields

  public static readonly Store Empty = new([]);

  private readonly List<Stock> _stocks;

  #endregion

  #region Properties

  /// <summary>What lies here, one stock to a commodity. A heap drawn to nothing is no longer a heap.</summary>
  public IReadOnlyCollection<Stock> Stocks => _stocks;

  #endregion

  #region Ctors

  private Store(List<Stock> stocks)
  {
    _stocks = stocks;
  }

  #endregion

  #region Methods

  public static Store Of(params Stock[] stocks)
  {
    var duplicate = stocks.GroupBy(stock => stock.Commodity).FirstOrDefault(group => group.Count() > 1);

    if (duplicate is not null)
    {
      throw new DomainException("A store keeps one heap of each commodity, not two.");
    }

    return new Store([.. stocks.Where(stock => !stock.IsExhausted)]);
  }

  public decimal AmountOf(CommodityId commodity)
  {
    return StockOf(commodity)?.Quantity.Amount ?? 0m;
  }

  /// <summary>
  /// Whether every requirement could be met at once. Two demands on the same
  /// heap are added together before being weighed against it: a store holding
  /// ten pounds of yarn does not hold two requirements of eight.
  /// </summary>
  public bool Holds(IEnumerable<Stock> required)
  {
    return Totalled(required).All(
      requirement => requirement.IsExhausted ||
                     (StockOf(requirement.Commodity) is { } held && held.Quantity.CompareTo(requirement.Quantity) >= 0));
  }

  public Store LaidUp(Stock stock)
  {
    return LaidUp([stock]);
  }

  /// <summary>What is laid up here on top of what was already held.</summary>
  public Store LaidUp(IEnumerable<Stock> stocks)
  {
    List<Stock> laidUp = [.. _stocks];

    foreach (var stock in stocks.Where(stock => !stock.IsExhausted))
    {
      var index = laidUp.FindIndex(held => held.Commodity == stock.Commodity);

      if (index < 0)
      {
        laidUp.Add(stock);
      }
      else
      {
        laidUp[index] = laidUp[index].Accumulated(stock.Quantity);
      }
    }

    return new Store(laidUp);
  }

  /// <summary>Takes everything asked of the store, or, if any of it is lacking, nothing at all.</summary>
  public Store Drawn(IEnumerable<Stock> stocks)
  {
    var required = Totalled(stocks);

    if (!Holds(required))
    {
      throw new DomainException("Nothing can be drawn from a store that is not there.");
    }

    List<Stock> drawn = [.. _stocks];

    foreach (var requirement in required.Where(requirement => !requirement.IsExhausted))
    {
      var index = drawn.FindIndex(held => held.Commodity == requirement.Commodity);
      var remaining = drawn[index].Consumed(requirement.Quantity);

      if (remaining.IsExhausted)
      {
        drawn.RemoveAt(index);
      }
      else
      {
        drawn[index] = remaining;
      }
    }

    return new Store(drawn);
  }

  private Stock? StockOf(CommodityId commodity)
  {
    return _stocks.SingleOrDefault(stock => stock.Commodity == commodity);
  }

  /// <summary>Requirements on the same commodity summed into one, in the order they were first made.</summary>
  private static List<Stock> Totalled(IEnumerable<Stock> stocks)
  {
    return
    [
      .. stocks
        .GroupBy(stock => stock.Commodity)
        .Select(group => group.Skip(1).Aggregate(group.First(), (total, stock) => total.Accumulated(stock.Quantity)))
    ];
  }

  public bool Equals(Store? other)
  {
    return other is not null
           && _stocks.Count == other._stocks.Count
           && _stocks.All(other._stocks.Contains);
  }

  public override int GetHashCode()
  {
    return _stocks.Aggregate(0, (hash, stock) => hash ^ stock.GetHashCode());
  }

  public override string ToString()
  {
    return _stocks.Count == 0 ? "nothing laid up" : string.Join(", ", _stocks.Select(stock => stock.Quantity));
  }

  #endregion
}
