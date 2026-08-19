using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Surplus.Testing;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Commodities;

public class CommodityTests
{
  #region Tests

  [Fact]
  public void A_commodity_belongs_to_the_department_whose_consumption_it_enters()
  {
    var iron = new UseValueBuilder().WithSatisfiedWant("working up into machines").WithUnit("ton").Build();

    var commodity = Commodity.Produce("Iron", iron, Department.MeansOfProduction, LaborTime.FromHours(6m));

    Assert.Equal(Department.MeansOfProduction, commodity.Department);
  }

  [Fact]
  public void A_commodity_must_have_a_name()
  {
    Assert.Throws<DomainException>(() => Commodity.Produce(
        "   ", new UseValueBuilder().Build(), Department.MeansOfConsumption, LaborTime.FromHours(1m)
      )
    );
  }

  [Fact]
  public void A_commodity_reads_as_its_name_and_its_two_factors()
  {
    var coat = new CommodityBuilder().Build();

    Assert.Equal("Coat (satisfies the want for clothing, value of 20h of labour)", coat.ToString());
  }

  [Fact]
  public void A_commodity_unites_a_use_value_and_a_value()
  {
    var clothing = new UseValueBuilder().Build();

    var coat = Commodity.Produce("  Coat  ", clothing, Department.MeansOfConsumption, LaborTime.FromHours(20m));

    Assert.Equal("Coat", coat.Name);
    Assert.Equal(clothing, coat.UseValue);
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(20m)), coat.Value);
  }

  [Fact]
  public void A_commodity_whose_reproduction_costs_no_labor_has_ceased_to_be_one()
  {
    var linen = new CommodityBuilder().WithName("Linen").Build();

    Assert.Throws<DomainException>(() => linen.Revalue(LaborTime.None));
  }

  [Fact]
  public void A_rise_in_productivity_depreciates_what_is_already_produced()
  {
    // The power-loom halves the labour of weaving, and the cloth on the shelf —
    // hand-woven, already finished, untouched — falls with it.
    var linen = new CommodityBuilder().WithName("Linen").WithSociallyNecessaryLaborTime(LaborTime.FromHours(2m))
      .Build();

    linen.Revalue(LaborTime.FromHours(1m));

    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(1m)), linen.Value);
  }

  [Fact]
  public void A_saved_commodity_is_reloaded_exactly_as_it_was_left()
  {
    var id = CommodityId.New();
    var linen = new UseValueBuilder().WithSatisfiedWant("clothing material").WithUnit("yard").Build();

    var commodity = new CommodityBuilder().WithId(id).WithName("Linen").WithUseValue(linen)
      .WithDepartment(Department.MeansOfProduction).WithSociallyNecessaryLaborTime(LaborTime.FromHours(10m)).Build();

    Assert.Equal(id, commodity.Id);
    Assert.Equal("Linen", commodity.Name);
    Assert.Equal(linen, commodity.UseValue);
    Assert.Equal(Department.MeansOfProduction, commodity.Department);
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(10m)), commodity.Value);
  }

  [Fact]
  public void A_useful_thing_owing_nothing_to_labor_is_no_commodity()
  {
    // Air and virgin soil are use-values, not commodities.
    var breathable = new UseValueBuilder().WithSatisfiedWant("breathing").WithUnit("litre").Build();

    Assert.Throws<DomainException>(
      () => Commodity.Produce("Air", breathable, Department.MeansOfConsumption, LaborTime.None)
    );
  }

  [Fact]
  public void Each_produced_commodity_has_its_own_identity()
  {
    var useValue = new UseValueBuilder().Build();

    var first = Commodity.Produce("Coat", useValue, Department.MeansOfConsumption, LaborTime.FromHours(20m));
    var second = Commodity.Produce("Coat", useValue, Department.MeansOfConsumption, LaborTime.FromHours(20m));

    Assert.NotEqual(first.Id, second.Id);
  }

  #endregion
}
