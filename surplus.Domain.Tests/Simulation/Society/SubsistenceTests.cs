using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Tests.Simulation.Society;

public class SubsistenceTests
{
  #region Fields

  private static readonly CommodityId Cloth = CommodityId.New();
  private static readonly CommodityId Corn = CommodityId.New();
  private static readonly UnitOfMeasure Metre = UnitOfMeasure.Of("m");
  private static readonly UnitOfMeasure Tonne = UnitOfMeasure.Of("t");

  #endregion

  #region Tests

  [Fact]
  public void A_basket_with_nothing_in_it_keeps_no_one_alive()
  {
    Assert.Throws<DomainException>(() => Subsistence.Of());
  }

  [Fact]
  public void The_same_thing_cannot_be_in_the_basket_twice()
  {
    Assert.Throws<DomainException>(() => Subsistence.Of(Tonnes(0.025m), Tonnes(0.01m)));
  }

  [Fact]
  public void So_many_heads_need_the_basket_so_many_times_over()
  {
    var basket = Subsistence.Of(Tonnes(0.025m), Metres(0.5m));

    Assert.Equal([Tonnes(5m), Metres(100m)], basket.For(200));
  }

  [Fact]
  public void No_one_needs_nothing()
  {
    Assert.Equal([Tonnes(0m), Metres(0m)], Subsistence.Of(Tonnes(0.025m), Metres(0.5m)).For(0));
  }

  [Fact]
  public void There_is_no_feeding_a_negative_number_of_mouths()
  {
    Assert.Throws<DomainException>(() => Subsistence.Of(Tonnes(0.025m)).For(-1));
  }

  [Fact]
  public void Two_baskets_of_the_same_goods_in_the_same_amounts_are_the_same_basket()
  {
    var basket = Subsistence.Of(Tonnes(0.025m), Metres(0.5m));
    var same = Subsistence.Of(Tonnes(0.025m), Metres(0.5m));

    Assert.Equal(basket, same);
    Assert.Equal(basket.GetHashCode(), same.GetHashCode());
    Assert.NotEqual(basket, Subsistence.Of(Tonnes(0.025m), Metres(1m)));
    Assert.False(basket.Equals(null));
  }

  [Fact]
  public void A_saved_basket_is_reloaded_as_it_was_left()
  {
    Assert.Equal(Subsistence.Of(Tonnes(0.025m)), Subsistence.Load([Tonnes(0.025m)]));
    Assert.Empty(Subsistence.Load([]).PerHead);
  }

  #endregion

  #region Methods

  private static Stock Tonnes(decimal amount)
  {
    return Stock.Of(Corn, Quantity.Of(amount, Tonne));
  }

  private static Stock Metres(decimal amount)
  {
    return Stock.Of(Cloth, Quantity.Of(amount, Metre));
  }

  #endregion
}
