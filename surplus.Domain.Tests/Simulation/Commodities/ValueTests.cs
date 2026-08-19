using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Production;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Commodities;

public class ValueTests
{
  #region Tests

  [Fact]
  public void A_value_reads_as_the_labor_time_crystallised_in_it()
  {
    var value = Value.CrystallisedFrom(LaborTime.FromHours(20m));

    Assert.Equal("value of 20h of labour", value.ToString());
  }

  [Fact]
  public void Addition_congeals_the_labor_of_both_values()
  {
    var sum = Value.CrystallisedFrom(LaborTime.FromHours(3m)) + Value.CrystallisedFrom(LaborTime.FromHours(5m));

    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(8m)), sum);
  }

  [Fact]
  public void Equal_magnitudes_of_congealed_labor_are_the_same_value()
  {
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(2m)), Value.CrystallisedFrom(LaborTime.FromHours(2m)));
  }

  [Fact]
  public void No_proportion_can_be_formed_with_a_valueless_thing()
  {
    var value = Value.CrystallisedFrom(LaborTime.FromHours(1m));

    Assert.Throws<DomainException>(() => value.RatioTo(Value.None));
  }

  [Fact]
  public void Scaling_a_value_holds_the_labor_of_the_whole_mass()
  {
    // A hundred yards hold a hundred times the labour of one, because the
    // labour was performed a hundred times over.
    Assert.Equal(
      Value.CrystallisedFrom(LaborTime.FromHours(200m)), Value.CrystallisedFrom(LaborTime.FromHours(2m)) * 100m
    );
  }

  [Fact]
  public void Things_can_be_use_values_without_being_values()
  {
    // Air, virgin soil: useful, yet owing nothing to labour.
    Assert.True(Value.None.IsNone);
    Assert.True(Value.CrystallisedFrom(LaborTime.None).IsNone);
  }

  [Fact]
  public void Value_is_crystallised_labor_and_its_magnitude_is_labor_time()
  {
    var value = Value.CrystallisedFrom(LaborTime.FromHours(20m));

    Assert.Equal(LaborTime.FromHours(20m), value.Magnitude);
  }

  [Fact]
  public void Values_are_ordered_by_their_magnitude()
  {
    var smaller = Value.CrystallisedFrom(LaborTime.FromHours(1m));
    var greater = Value.CrystallisedFrom(LaborTime.FromHours(2m));

    Assert.True(greater.CompareTo(smaller) > 0);
    Assert.True(smaller.CompareTo(greater) < 0);
    Assert.Equal(0, smaller.CompareTo(Value.CrystallisedFrom(LaborTime.FromHours(1m))));
    Assert.True(greater.CompareTo(null) > 0);
  }

  [Fact]
  public void Values_share_one_substance_and_are_therefore_commensurable()
  {
    var linenValue = Value.CrystallisedFrom(LaborTime.FromHours(1m));
    var coatValue = Value.CrystallisedFrom(LaborTime.FromHours(20m));

    Assert.Equal(20m, coatValue.RatioTo(linenValue));
    Assert.Equal(0.05m, linenValue.RatioTo(coatValue));
  }

  #endregion
}
