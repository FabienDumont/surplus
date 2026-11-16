using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Domain.Tests.Simulation.Commodities;

public class StoreTests
{
  #region Fields

  private static readonly CommodityId Corn = CommodityId.New();
  private static readonly CommodityId Iron = CommodityId.New();
  private static readonly UnitOfMeasure Bushel = UnitOfMeasure.Of("bushel");
  private static readonly UnitOfMeasure Ton = UnitOfMeasure.Of("ton");

  #endregion

  #region Tests

  [Fact]
  public void An_empty_store_holds_nothing()
  {
    Assert.Empty(Store.Empty.Stocks);
    Assert.Equal(0m, Store.Empty.AmountOf(Corn));
    Assert.False(Store.Empty.Holds([Bushels(1m)]));
    Assert.True(Store.Empty.Holds([]));
  }

  [Fact]
  public void What_is_laid_up_is_added_to_what_was_already_held()
  {
    var store = Store.Of(Bushels(10m)).LaidUp(Bushels(5m)).LaidUp([Tons(2m)]);

    Assert.Equal(15m, store.AmountOf(Corn));
    Assert.Equal(2m, store.AmountOf(Iron));
    Assert.Equal(2, store.Stocks.Count);
  }

  [Fact]
  public void A_store_cannot_be_founded_on_the_same_commodity_twice()
  {
    Assert.Throws<DomainException>(() => Store.Of(Bushels(1m), Bushels(2m)));
  }

  [Fact]
  public void Nothing_can_be_drawn_that_is_not_there()
  {
    var store = Store.Of(Bushels(10m), Tons(1m));

    Assert.Throws<DomainException>(() => store.Drawn([Bushels(5m), Tons(2m)]));
    Assert.Equal(Store.Of(Bushels(10m), Tons(1m)), store);
  }

  [Fact]
  public void A_store_holds_a_requirement_only_if_it_holds_all_of_it()
  {
    var store = Store.Of(Bushels(10m), Tons(1m));

    Assert.True(store.Holds([Bushels(10m), Tons(1m)]));
    Assert.False(store.Holds([Bushels(10m), Tons(1.5m)]));
    Assert.False(store.Holds([Stock.Of(CommodityId.New(), Quantity.Of(1m, Ton))]));
  }

  [Fact]
  public void Two_requirements_for_one_commodity_are_counted_together()
  {
    var store = Store.Of(Bushels(10m));

    Assert.False(store.Holds([Bushels(8m), Bushels(8m)]));
    Assert.Throws<DomainException>(() => store.Drawn([Bushels(8m), Bushels(8m)]));
    Assert.Equal(4m, store.Drawn([Bushels(3m), Bushels(3m)]).AmountOf(Corn));
  }

  [Fact]
  public void A_stock_drawn_to_nothing_is_no_longer_listed()
  {
    var store = Store.Of(Bushels(10m), Tons(1m)).Drawn([Bushels(10m)]);

    Assert.Equal([Tons(1m)], store.Stocks);
    Assert.Equal(0m, store.AmountOf(Corn));
  }

  [Fact]
  public void Two_stores_holding_the_same_things_are_the_same_store()
  {
    var store = Store.Of(Bushels(10m), Tons(1m));

    Assert.Equal(store, Store.Of(Tons(1m), Bushels(10m)));
    Assert.Equal(store.GetHashCode(), Store.Of(Tons(1m), Bushels(10m)).GetHashCode());
    Assert.Equal(store, Store.Empty.LaidUp([Bushels(4m), Tons(1m), Bushels(6m)]));
    Assert.NotEqual(store, Store.Of(Bushels(10m)));
    Assert.NotEqual(store, Store.Of(Bushels(9m), Tons(1m)));
  }

  [Fact]
  public void A_store_reads_as_what_lies_in_it()
  {
    Assert.Equal("nothing laid up", Store.Empty.ToString());
    Assert.Equal("10 bushel, 1.5 ton", Store.Of(Bushels(10m), Tons(1.5m)).ToString());
  }

  [Fact]
  public void Use_values_of_different_kinds_cannot_be_drawn_for_one_another()
  {
    var store = Store.Of(Bushels(10m));

    Assert.Throws<DomainException>(() => store.Drawn([Stock.Of(Corn, Quantity.Of(1m, Ton))]));
    Assert.Throws<DomainException>(() => store.LaidUp(Stock.Of(Corn, Quantity.Of(1m, Ton))));
  }

  #endregion

  #region Methods

  private static Stock Bushels(decimal amount)
  {
    return Stock.Of(Corn, Quantity.Of(amount, Bushel));
  }

  private static Stock Tons(decimal amount)
  {
    return Stock.Of(Iron, Quantity.Of(amount, Ton));
  }

  #endregion
}
