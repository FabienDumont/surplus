using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Surplus.Testing;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Commodities;

public class StockTests
{
  #region Fields

  private static readonly UnitOfMeasure Yard = UnitOfMeasure.Of("yard");

  #endregion

  #region Tests

  [Fact]
  public void A_stock_can_only_be_valued_at_the_commodity_it_is_made_of()
  {
    var linen = Linen(LaborTime.FromHours(2m));
    var coats = new CommodityBuilder().Build();

    var stock = Stock.Of(linen.Id, Quantity.Of(20m, Yard));

    Assert.Throws<DomainException>(() => stock.ValuedAt(coats));
  }

  [Fact]
  public void A_stock_can_only_be_valued_in_the_unit_its_commodity_is_measured_by()
  {
    var linen = Linen(LaborTime.FromHours(2m));

    var stock = Stock.Of(linen.Id, Quantity.Of(20m, UnitOfMeasure.Of("ell")));

    Assert.Throws<DomainException>(() => stock.ValuedAt(linen));
  }

  [Fact]
  public void A_stock_holds_a_quantity_of_one_commodity()
  {
    var linen = Linen(LaborTime.FromHours(2m));

    var stock = Stock.Of(linen.Id, Quantity.Of(20m, Yard));

    Assert.Equal(linen.Id, stock.Commodity);
    Assert.Equal(Quantity.Of(20m, Yard), stock.Quantity);
    Assert.Equal("20 yard in stock", stock.ToString());
  }

  [Fact]
  public void A_stock_is_not_edited_in_place_but_accumulated_and_consumed_into_another()
  {
    var id = CommodityId.New();
    var stock = Stock.Of(id, Quantity.Of(20m, Yard));

    var laidUp = stock.Accumulated(Quantity.Of(10m, Yard));
    var drawnOn = laidUp.Consumed(Quantity.Of(5m, Yard));

    Assert.Equal(Quantity.Of(20m, Yard), stock.Quantity);
    Assert.Equal(Stock.Of(id, Quantity.Of(30m, Yard)), laidUp);
    Assert.Equal(Stock.Of(id, Quantity.Of(25m, Yard)), drawnOn);
  }

  [Fact]
  public void A_stock_is_valued_at_what_reproducing_it_costs_now()
  {
    var linen = Linen(LaborTime.FromHours(2m));
    var stock = Stock.Of(linen.Id, Quantity.Of(100m, Yard));

    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(200m)), stock.ValuedAt(linen));

    // The power-loom arrives: not a yard has moved, and the heap is worth half.
    linen.Revalue(LaborTime.FromHours(1m));

    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(100m)), stock.ValuedAt(linen));
  }

  [Fact]
  public void An_empty_store_is_still_counted_in_its_commodity_unit()
  {
    var linen = Linen(LaborTime.FromHours(2m));

    var stock = Stock.EmptyOf(linen.Id, Yard);

    Assert.True(stock.IsExhausted);
    Assert.Equal(Yard, stock.Quantity.Unit);
    Assert.Equal(Value.None, stock.ValuedAt(linen));
  }

  [Fact]
  public void Nothing_can_be_consumed_that_is_not_in_store()
  {
    var stock = Stock.Of(CommodityId.New(), Quantity.Of(5m, Yard));

    Assert.Throws<DomainException>(() => stock.Consumed(Quantity.Of(6m, Yard)));
    Assert.True(stock.Consumed(Quantity.Of(5m, Yard)).IsExhausted);
  }

  #endregion

  #region Methods

  private static Commodity Linen(LaborTime perYard)
  {
    return new CommodityBuilder().WithName("Linen")
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("clothing material").WithUnit(Yard).Build())
      .WithSociallyNecessaryLaborTime(perYard).Build();
  }

  #endregion
}
