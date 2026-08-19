using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Production;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Production;

public class ValueCompositionTests
{
  #region Tests

  [Fact]
  public void A_composition_reads_in_the_form_it_is_always_written_in()
  {
    Assert.Equal("80c + 20v + 20s", Composition(80m, 20m, 20m).ToString());
  }

  [Fact]
  public void A_rising_organic_composition_drags_the_rate_of_profit_down()
  {
    // The same rate of exploitation throughout: the labourer is squeezed exactly
    // as hard in both. All that changes is how much dead labour each pair of
    // hands sets in motion — and the profit rate falls by more than half.
    var handicraft = Composition(50m, 50m, 50m);
    var factory = Composition(400m, 100m, 100m);

    Assert.Equal(1m, handicraft.RateOfSurplusValue());
    Assert.Equal(1m, factory.RateOfSurplusValue());

    Assert.Equal(1m, handicraft.OrganicComposition());
    Assert.Equal(4m, factory.OrganicComposition());

    Assert.Equal(0.5m, handicraft.RateOfProfit());
    Assert.Equal(0.2m, factory.RateOfProfit());
  }

  [Fact]
  public void A_working_day_splits_into_necessary_and_surplus_labor()
  {
    // Twelve hours worked, six of them enough to reproduce the labourer. The
    // other six are worked for nothing, and no hour of the day says which.
    var composition = ValueComposition.FromWorkingDay(
      Value.CrystallisedFrom(LaborTime.FromHours(12m)), LaborTime.FromHours(12m), LaborTime.FromHours(6m)
    );

    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(6m)), composition.VariableCapital);
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(6m)), composition.SurplusValue);
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(12m)), composition.NewValue);
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(24m)), composition.Product);
    Assert.True(composition.ExtractsSurplus);
    Assert.Equal(1m, composition.RateOfSurplusValue());
  }

  [Fact]
  public void Dead_labor_alone_returns_no_profit()
  {
    var nothingAdvanced = ValueComposition.Of(Value.None, Value.None, Value.None);

    Assert.Throws<DomainException>(() => nothingAdvanced.RateOfProfit());
    Assert.Throws<DomainException>(() => nothingAdvanced.OrganicComposition());
  }

  [Fact]
  public void Necessary_labor_cannot_outrun_the_working_day()
  {
    Assert.Throws<DomainException>(() => ValueComposition.FromWorkingDay(
        Value.None, LaborTime.FromHours(6m), LaborTime.FromHours(7m)
      )
    );
  }

  [Fact]
  public void No_new_value_arises_where_no_labor_is_performed()
  {
    Assert.Throws<DomainException>(() => ValueComposition.FromWorkingDay(
        Value.CrystallisedFrom(LaborTime.FromHours(100m)), LaborTime.None, LaborTime.None
      )
    );
  }

  [Fact]
  public void The_whole_day_is_surplus_where_the_labourer_costs_nothing()
  {
    // The slave is bought outright, not their labour-power: there is no wage, so
    // no rate of surplus-value can be formed, and the whole day appears unpaid.
    var slaveDay = ValueComposition.FromWorkingDay(
      Value.CrystallisedFrom(LaborTime.FromHours(10m)), LaborTime.FromHours(12m), LaborTime.None
    );

    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(12m)), slaveDay.SurplusValue);
    Assert.True(slaveDay.ExtractsSurplus);
    Assert.Throws<DomainException>(() => slaveDay.RateOfSurplusValue());

    // A rate of profit can still be formed: something was advanced on the means.
    Assert.Equal(1.2m, slaveDay.RateOfProfit());
  }

  [Fact]
  public void Where_the_day_is_all_necessary_labor_nothing_is_left_over()
  {
    var composition = ValueComposition.FromWorkingDay(
      Value.None, LaborTime.FromHours(6m), LaborTime.FromHours(6m)
    );

    Assert.False(composition.ExtractsSurplus);
    Assert.Equal(0m, composition.RateOfSurplusValue());
    Assert.Equal(0m, composition.RateOfProfit());
  }

  #endregion

  #region Methods

  private static ValueComposition Composition(decimal constant, decimal variable, decimal surplus)
  {
    return ValueComposition.Of(
      Value.CrystallisedFrom(LaborTime.FromHours(constant)), Value.CrystallisedFrom(LaborTime.FromHours(variable)),
      Value.CrystallisedFrom(LaborTime.FromHours(surplus))
    );
  }

  #endregion
}
