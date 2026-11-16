using Surplus.Application.Reporting;
using Surplus.Domain.GameEngine;
using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Surplus.Testing;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Application.Tests.Reporting;

public class SignificanceTests
{
  #region Tests

  [Fact]
  public void A_famine_at_home_is_urgent_and_abroad_is_notable()
  {
    var world = Kingdom();

    Assert.Equal(Importance.Urgent, Significance.Of(new PeopleWentWithout("United Kingdom", 0.5m, 3), world));
    Assert.Equal(Importance.Notable, Significance.Of(new PeopleWentWithout("Russia", 0.5m, 3), world));
  }

  [Fact]
  public void An_idle_branch_at_home_is_urgent_and_abroad_is_notable()
  {
    var world = Kingdom();

    Assert.Equal(
      Importance.Urgent, Significance.Of(new BranchStoodIdle("United Kingdom", "Lancashire", "Cloth"), world));
    Assert.Equal(Importance.Notable, Significance.Of(new BranchStoodIdle("Russia", "Muscovy", "Cloth"), world));
  }

  [Fact]
  public void A_change_in_value_is_worth_reading_wherever_it_came_from()
  {
    var revalued = new CommodityWasRevalued("Cloth", "m", Hours(2m), Hours(2.4m));

    Assert.Equal(Importance.Notable, Significance.Of(revalued, Kingdom()));
  }

  [Fact]
  public void Growth_is_routine()
  {
    Assert.Equal(Importance.Routine, Significance.Of(new PopulationGrew("United Kingdom", 200), Kingdom()));
  }

  [Fact]
  public void A_war_declared_on_the_player_is_urgent_and_one_they_declared_is_not()
  {
    var world = Kingdom();

    Assert.Equal(Importance.Urgent, Significance.Of(new WarWasDeclared("Russia", "United Kingdom"), world));
    Assert.Equal(Importance.Notable, Significance.Of(new WarWasDeclared("United Kingdom", "Russia"), world));
    Assert.Equal(Importance.Notable, Significance.Of(new WarWasDeclared("Russia", "United States"), world));
  }

  [Fact]
  public void A_peace_forced_on_the_player_is_urgent_and_one_they_made_is_not()
  {
    var world = Kingdom();

    Assert.Equal(
      Importance.Urgent, Significance.Of(new PeaceWasMade("Russia", "United Kingdom", "Kent is ceded to Russia"), world));
    Assert.Equal(
      Importance.Notable, Significance.Of(new PeaceWasMade("United Kingdom", "Russia", "nothing changes hands"), world));
  }

  [Fact]
  public void Carts_falling_short_at_home_are_urgent_and_abroad_are_notable()
  {
    var world = Kingdom();

    Assert.Equal(Importance.Urgent, Significance.Of(new CarriageFellShort("United Kingdom", "Kent", "Corn"), world));
    Assert.Equal(Importance.Notable, Significance.Of(new CarriageFellShort("Russia", "Muscovy", "Cloth"), world));
  }

  [Fact]
  public void A_technique_found_at_home_is_urgent()
  {
    var world = Kingdom();

    Assert.Equal(Importance.Urgent, Significance.Of(new TechniqueWasDeveloped("United Kingdom", "Power loom", "Cloth"), world));
    Assert.Equal(Importance.Notable, Significance.Of(new TechniqueWasDeveloped("Russia", "Large farming", "Corn"), world));
  }

  [Fact]
  public void The_front_is_routine()
  {
    Assert.Equal(Importance.Routine, Significance.Of(new TheFrontMoved("Russia", "United Kingdom", -4), Kingdom()));
  }

  [Fact]
  public void Worklessness_is_notable_and_does_not_stop_the_clock()
  {
    var world = Kingdom();
    var today = new DayReport(GameDate.Of(1836, 1, 2), [new TheWorklessWentWithout("United Kingdom", 3_350, 16)]);

    Assert.Equal(Importance.Notable, Significance.Of(new TheWorklessWentWithout("United Kingdom", 3_350, 16), world));
    Assert.Equal(Importance.Notable, Significance.Of(new SankIntoPauperism("United Kingdom", 28), world));
    Assert.False(Significance.CallsForAttention(today, null, world));
  }

  [Fact]
  public void Worklessness_is_the_same_concern_whatever_its_numbers()
  {
    Assert.Equal(
      Significance.Concern(new TheWorklessWentWithout("United Kingdom", 3_350, 16)),
      Significance.Concern(new TheWorklessWentWithout("United Kingdom", 3_000, 15)));
  }

  [Fact]
  public void An_occurrence_nobody_has_classified_is_shown()
  {
    Assert.Equal(Importance.Notable, Significance.Of(new CometSighted(), Kingdom()));
  }

  [Fact]
  public void With_no_one_played_nothing_happens_at_home()
  {
    var world = new WorldBuilder().WithCountries(new CountryBuilder().WithName("United Kingdom").Build()).Build();

    Assert.Equal(Importance.Notable, Significance.Of(new PeopleWentWithout("United Kingdom", 0.5m, 3), world));
  }

  [Fact]
  public void A_day_calls_for_attention_when_anything_in_it_is_urgent()
  {
    var world = Kingdom();
    var today = GameDate.Of(1836, 1, 2);

    Assert.True(Significance.CallsForAttention(
      new DayReport(today, [new PopulationGrew("United Kingdom", 200), new PeopleWentWithout("United Kingdom", 0.5m, 3)]),
      null, world));
    Assert.False(Significance.CallsForAttention(
      new DayReport(today, [new PopulationGrew("United Kingdom", 200), new PeopleWentWithout("Russia", 0.5m, 3)]),
      null, world));
  }

  [Fact]
  public void A_trouble_already_under_way_the_day_before_does_not_stop_the_clock_again()
  {
    var yesterday = new DayReport(GameDate.Of(1836, 1, 2), [new PeopleWentWithout("United Kingdom", 0.5m, 3)]);
    var today = new DayReport(GameDate.Of(1836, 1, 3), [new PeopleWentWithout("United Kingdom", 0.6m, 2)]);

    Assert.False(Significance.CallsForAttention(today, yesterday, Kingdom()));
  }

  [Fact]
  public void A_new_trouble_stops_the_clock_while_an_old_one_lasts()
  {
    var looms = new BranchStoodIdle("United Kingdom", "Lancashire", "Cloth");
    var yesterday = new DayReport(GameDate.Of(1836, 1, 2), [looms]);
    var today = new DayReport(
      GameDate.Of(1836, 1, 3), [looms, new BranchStoodIdle("United Kingdom", "Lancashire", "Machinery")]);

    Assert.True(Significance.CallsForAttention(today, yesterday, Kingdom()));
  }

  [Fact]
  public void The_same_trouble_is_recognised_on_two_days_and_anything_else_is_only_itself()
  {
    Assert.Equal(
      Significance.Concern(new PeopleWentWithout("Russia", 0.5m, 3)),
      Significance.Concern(new PeopleWentWithout("Russia", 0.9m, 1)));
    Assert.NotEqual(
      Significance.Concern(new BranchStoodIdle("Russia", "Muscovy", "Cloth")),
      Significance.Concern(new BranchStoodIdle("Russia", "Ukraine", "Cloth")));
    Assert.Equal(new PopulationGrew("Russia", 1), Significance.Concern(new PopulationGrew("Russia", 1)));
  }

  #endregion

  #region Methods

  private static World Kingdom()
  {
    return new WorldBuilder().WithCountries(new CountryBuilder().WithName("United Kingdom").Played().Build()).Build();
  }

  private static Value Hours(decimal hours)
  {
    return Value.CrystallisedFrom(LaborTime.FromHours(hours));
  }

  #endregion
}
