using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Testing;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Commodities;

public class CommodityRegisterTests
{
  #region Fields

  private static readonly UnitOfMeasure Machine = UnitOfMeasure.Of("machine");
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

  [Fact]
  public void A_commodity_is_worth_what_its_production_cost_on_average()
  {
    var linen = Linen(LaborTime.FromHours(2m));
    var register = CommodityRegister.Of(linen);

    // A hundred yards at an hour each and fifty at three: 250 hours over 150 yards,
    // not the two hours a simple average of the two producers would give.
    register.Revalue([Woven(linen, 100m, 100m), Woven(linen, 50m, 150m)]);

    Assert.Equal(Hours(250m / 150m), linen.Value);
  }

  [Fact]
  public void The_larger_producer_weighs_more_in_the_average()
  {
    var linen = Linen(LaborTime.FromHours(2m));
    var register = CommodityRegister.Of(linen);

    register.Revalue([Woven(linen, 900m, 900m), Woven(linen, 100m, 300m)]);

    Assert.Equal(Hours(1.2m), linen.Value);
  }

  [Fact]
  public void A_commodity_nobody_produced_keeps_its_value()
  {
    var linen = Linen(LaborTime.FromHours(2m));
    var iron = Iron(LaborTime.FromHours(6m));
    var register = CommodityRegister.Of(linen, iron);

    var revaluations = register.Revalue([Woven(linen, 10m, 10m)]);

    Assert.Equal(Hours(6m), iron.Value);
    Assert.Equal(linen.Id, Assert.Single(revaluations).Commodity);
    Assert.Empty(register.Revalue([]));
  }

  [Fact]
  public void Dead_labour_counts_toward_what_a_thing_is_worth()
  {
    var linen = Linen(LaborTime.FromHours(2m));
    var register = CommodityRegister.Of(linen);
    var composition = ValueComposition.Of(Hours(40m), Hours(6m), Hours(6m));

    register.Revalue([Yield.Of(Stock.Of(linen.Id, Quantity.Of(10m, Yard)), composition)]);

    Assert.Equal(Hours(5.2m), linen.Value);
  }

  [Fact]
  public void Every_commodity_is_measured_against_the_same_yesterday()
  {
    var iron = Iron(LaborTime.FromHours(6m));
    var machines = new CommodityBuilder().WithName("Machine").WithDepartment(Department.MeansOfProduction)
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("motive power").WithUnit(Machine).Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(100m)).Build();
    var register = CommodityRegister.Of(iron, machines);
    var ironWorks = new BranchBuilder().Producing(iron.Id).WorkedBy(SocialClass.Proletariat, 10)
      .WithOutputPerHour(Quantity.Of(1m, Ton)).Build();
    var machineShop = new BranchBuilder().Producing(machines.Id).WorkedBy(SocialClass.Proletariat, 10)
      .WithMeans(MeansOfProduction.SetInMotion(
        ProductiveForm.Manufacture, 0m, [], [Stock.Of(iron.Id, Quantity.Of(10m, Ton))]))
      .WithOutputPerHour(Quantity.Of(0.1m, Machine)).Build();

    // The iron works makes iron at an hour a ton, cheaper than the six it was worth.
    var yields = new[] { ironWorks.Work(register), machineShop.Work(register) };
    register.Revalue(yields);

    // The machines worked up ten tons counted at yesterday's six hours, not today's one:
    // 60 hours of iron and 120 of living labour over twelve machines.
    Assert.Equal(Hours(1m), iron.Value);
    Assert.Equal(Hours(15m), machines.Value);
  }

  [Fact]
  public void A_revaluation_reports_what_the_value_was_and_what_it_became()
  {
    var linen = Linen(LaborTime.FromHours(2m));
    var register = CommodityRegister.Of(linen);

    var revaluation = Assert.Single(register.Revalue([Woven(linen, 10m, 10m)]));

    Assert.Equal(new Revaluation(linen.Id, Hours(2m), Hours(1m)), revaluation);
    Assert.Equal(-0.5m, revaluation.RelativeChange);
  }

  #endregion

  #region Methods

  private static Value Hours(decimal hours)
  {
    return Value.CrystallisedFrom(LaborTime.FromHours(hours));
  }

  /// <summary>So many yards of linen, holding so many hours of living labour and nothing else.</summary>
  private static Yield Woven(Commodity linen, decimal yards, decimal hours)
  {
    return Yield.Of(
      Stock.Of(linen.Id, Quantity.Of(yards, Yard)),
      ValueComposition.FromWorkingDay(Value.None, LaborTime.FromHours(hours), LaborTime.FromHours(hours / 2m)));
  }

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
