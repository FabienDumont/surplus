using System.Globalization;
using Surplus.Application.Settings;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Application.Tests.Settings;

public class MeasuresTests
{
  #region Fields

  private static readonly UnitOfMeasure Metre = UnitOfMeasure.Of("m");

  #endregion

  #region Tests

  [Theory]
  [InlineData("m", "91.44", "100", "yd")]
  [InlineData("kg", "45.359237", "100", "lb")]
  [InlineData("t", "1.0160469088", "1", "long ton")]
  public void A_quantity_is_shown_in_its_imperial_counterpart(
    string unit, string amount, string imperial, string shownUnit)
  {
    var shown = Measures.Shown(Quantity.Of(Parsed(amount), UnitOfMeasure.Of(unit)), UnitSystem.Imperial);

    Assert.Equal(Quantity.Of(Parsed(imperial), UnitOfMeasure.Of(shownUnit)), shown);
    Assert.Equal(UnitOfMeasure.Of(shownUnit), Measures.Shown(UnitOfMeasure.Of(unit), UnitSystem.Imperial));
  }

  [Fact]
  public void The_metric_system_shows_quantities_as_the_simulation_counts_them()
  {
    var cloth = Quantity.Of(100m, Metre);

    Assert.Equal(cloth, Measures.Shown(cloth, UnitSystem.Metric));
    Assert.Equal(Metre, Measures.Shown(Metre, UnitSystem.Metric));
    Assert.Equal(Hours(2m), Measures.Shown(Hours(2m), Metre, UnitSystem.Metric));
  }

  [Fact]
  public void A_unit_with_no_imperial_counterpart_is_shown_as_it_is()
  {
    var machines = Quantity.Of(150m, UnitOfMeasure.Of("machine"));

    Assert.Equal(machines, Measures.Shown(machines, UnitSystem.Imperial));
    Assert.Equal(machines.Unit, Measures.Shown(machines.Unit, UnitSystem.Imperial));
    Assert.Equal(Hours(2_100m), Measures.Shown(Hours(2_100m), machines.Unit, UnitSystem.Imperial));
  }

  [Fact]
  public void A_heap_is_worth_the_same_hours_whichever_way_it_is_read()
  {
    var cloth = Quantity.Of(91.44m, Metre);
    var perMetre = Hours(2m);

    var perYard = Measures.Shown(perMetre, Metre, UnitSystem.Imperial);
    var yards = Measures.Shown(cloth, UnitSystem.Imperial);

    Assert.Equal(Quantity.Of(100m, UnitOfMeasure.Of("yd")), yards);
    Assert.Equal(Hours(1.8288m), perYard);
    Assert.Equal((perMetre * cloth.Amount).Magnitude.Hours, (perYard * yards.Amount).Magnitude.Hours);
  }

  [Fact]
  public void What_the_player_types_is_taken_back_into_the_simulations_unit()
  {
    Assert.Equal(Quantity.Of(91.44m, Metre), Measures.Entered(100m, Metre, UnitSystem.Imperial));
    Assert.Equal(Quantity.Of(100m, Metre), Measures.Entered(100m, Metre, UnitSystem.Metric));
    Assert.Equal(
      Quantity.Of(3m, UnitOfMeasure.Of("machine")), Measures.Entered(3m, UnitOfMeasure.Of("machine"), UnitSystem.Imperial));
  }

  #endregion

  #region Methods

  private static Value Hours(decimal hours)
  {
    return Value.CrystallisedFrom(LaborTime.FromHours(hours));
  }

  private static decimal Parsed(string amount)
  {
    return decimal.Parse(amount, CultureInfo.InvariantCulture);
  }

  #endregion
}
