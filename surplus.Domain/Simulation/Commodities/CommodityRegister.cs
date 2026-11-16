using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Domain.Simulation.Commodities;

/// <summary>
/// The commodities in play, held together so that a stock can be told what it is
/// worth.
/// A <see cref="Stock" /> knows what it is made of and how much of it lies
/// there, never its value — because value is social. It is one and the same for
/// every yard of linen in the market, whoever wove it and whenever, so it is
/// found here, once, rather than in any particular heap. Let the labour society
/// needs to reproduce linen fall, and it falls here: every stock of it in the
/// world is worth less the same morning, none of them having been touched.
/// </summary>
public sealed class CommodityRegister
{
  #region Fields

  private readonly Dictionary<CommodityId, Commodity> _commodities;

  #endregion

  #region Properties

  public IReadOnlyCollection<Commodity> Commodities => _commodities.Values;

  #endregion

  #region Ctors

  private CommodityRegister(Dictionary<CommodityId, Commodity> commodities)
  {
    _commodities = commodities;
  }

  #endregion

  #region Methods

  public static CommodityRegister Of(params Commodity[] commodities)
  {
    var duplicate = commodities.GroupBy(commodity => commodity.Id).FirstOrDefault(group => group.Count() > 1);

    if (duplicate is not null)
    {
      throw new DomainException($"{duplicate.First().Name} is registered twice.");
    }

    return new CommodityRegister(commodities.ToDictionary(commodity => commodity.Id));
  }

  public Commodity Get(CommodityId id)
  {
    return _commodities.TryGetValue(id, out var commodity)
      ? commodity
      : throw new DomainException("No commodity is registered under that identity.");
  }

  /// <summary>What a mass of one commodity is worth, at today's socially necessary labour time.</summary>
  public Value ValueOf(Stock stock)
  {
    return stock.ValuedAt(Get(stock.Commodity));
  }

  /// <summary>
  /// What a heterogeneous heap comes to. Yards of linen and tons of iron cannot
  /// be added as use-values, but their values share one substance and so do add
  /// up — which is the whole reason value must exist at all.
  /// </summary>
  public Value ValueOf(IEnumerable<Stock> stocks)
  {
    return stocks.Aggregate(Value.None, (total, stock) => total + ValueOf(stock));
  }

  /// <summary>
  /// Settles what each commodity is worth from what it cost to produce this
  /// period: the whole labour, dead and living, that went into the whole mass,
  /// spread over the whole mass. It is an average weighted by output, so a
  /// large producer weighs more than a small one, and the only way to pull it
  /// down is to produce below it.
  /// Every average is taken before any commodity is revalued. The dead labour
  /// in today's yields was counted at yesterday's values, and every commodity
  /// must be measured against the same yesterday. A commodity nobody produced
  /// keeps the value it had.
  /// </summary>
  public IReadOnlyList<Revaluation> Revalue(IEnumerable<Yield> yields)
  {
    var settled = yields
      .GroupBy(yield => yield.Product.Commodity)
      .Select(group => (
        Commodity: Get(group.Key),
        PerUnit: group.Aggregate(LaborTime.None, (total, yield) => total + yield.Composition.Product.Magnitude) /
                 group.Sum(yield => yield.Product.Quantity.Amount)))
      .ToList();

    return
    [
      .. settled.Select(settlement =>
      {
        var before = settlement.Commodity.Value;

        settlement.Commodity.Revalue(settlement.PerUnit);

        return new Revaluation(settlement.Commodity.Id, before, settlement.Commodity.Value);
      })
    ];
  }

  public override string ToString()
  {
    return $"{_commodities.Count} {(_commodities.Count == 1 ? "commodity" : "commodities")} in play";
  }

  #endregion
}
