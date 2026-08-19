using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Surplus.Testing;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Commodities;

public class CommodityRegisterTests
{
  #region Fields

  private static readonly UnitOfMeasure Ton = UnitOfMeasure.Of("ton");
  private static readonly UnitOfMeasure Yard = UnitOfMeasure.Of("yard");

  #endregion

  #region Tests

  [Fact]
  public void A_falling_labor_time_reaches_every_stock_of_the_commodity_at_once()
  {
    var linen = Linen(LaborTime.FromHours(2m));
    var register = CommodityRegister.Of(linen);

    var inManchester = Stock.Of(linen.Id, Quantity.Of(100m, Yard));
    var inLeeds = Stock.Of(linen.Id, Quantity.Of(50m, Yard));

    linen.Revalue(LaborTime.FromHours(1m));

    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(100m)), register.ValueOf(inManchester));
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(50m)), register.ValueOf(inLeeds));
  }

  [Fact]
  public void A_register_reads_as_how_many_commodities_are_in_play()
  {
    Assert.Equal("1 commodity in play", CommodityRegister.Of(Linen(LaborTime.FromHours(2m))).ToString());
    Assert.Equal(
      "2 commodities in play",
      CommodityRegister.Of(Linen(LaborTime.FromHours(2m)), Iron(LaborTime.FromHours(6m))).ToString()
    );
  }

  [Fact]
  public void A_register_tells_a_stock_what_it_is_worth()
  {
    var linen = Linen(LaborTime.FromHours(2m));
    var register = CommodityRegister.Of(linen);

    var stock = Stock.Of(linen.Id, Quantity.Of(20m, Yard));

    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(40m)), register.ValueOf(stock));
    Assert.Equal(linen, register.Get(linen.Id));
  }

  [Fact]
  public void A_register_shows_what_is_in_play()
  {
    var linen = Linen(LaborTime.FromHours(2m));
    var iron = Iron(LaborTime.FromHours(6m));

    Assert.Equal([linen, iron], CommodityRegister.Of(linen, iron).Commodities);
    Assert.Empty(CommodityRegister.Of().Commodities);
  }

  [Fact]
  public void A_register_will_not_hold_the_same_commodity_twice()
  {
    var linen = Linen(LaborTime.FromHours(2m));

    Assert.Throws<DomainException>(() => CommodityRegister.Of(linen, linen));
  }

  [Fact]
  public void An_unregistered_commodity_cannot_be_valued()
  {
    var register = CommodityRegister.Of(Linen(LaborTime.FromHours(2m)));

    Assert.Throws<DomainException>(() => register.Get(CommodityId.New()));
    Assert.Throws<DomainException>(() => register.ValueOf(Stock.Of(CommodityId.New(), Quantity.Of(1m, Yard))));
  }

  [Fact]
  public void Values_add_up_across_use_values_that_could_never_be_added()
  {
    var linen = Linen(LaborTime.FromHours(2m));
    var iron = Iron(LaborTime.FromHours(6m));
    var register = CommodityRegister.Of(linen, iron);

    // Twenty yards and three tons cannot be summed as use-values. As values they
    // can, because they are the same thing in different bodies.
    var total = register.ValueOf(
      [Stock.Of(linen.Id, Quantity.Of(20m, Yard)), Stock.Of(iron.Id, Quantity.Of(3m, Ton))]
    );

    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(58m)), total);
    Assert.Equal(Value.None, register.ValueOf([]));
  }

  #endregion

  #region Methods

  private static Commodity Linen(LaborTime perYard)
  {
    return new CommodityBuilder().WithName("Linen")
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("clothing material").WithUnit(Yard).Build())
      .WithSociallyNecessaryLaborTime(perYard).Build();
  }

  private static Commodity Iron(LaborTime perTon)
  {
    return new CommodityBuilder().WithName("Iron").WithDepartment(Department.MeansOfProduction)
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("working up into machines").WithUnit(Ton).Build())
      .WithSociallyNecessaryLaborTime(perTon).Build();
  }

  #endregion
}
