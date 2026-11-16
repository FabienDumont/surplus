using Surplus.Application.Reporting;
using Surplus.Application.Scenarios;
using Surplus.Application.Settings;
using Surplus.Domain.GameEngine;
using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Application.Tests.Scenarios;

public class EighteenThirtySixTests
{
  #region Tests

  [Fact]
  public void The_scenario_opens_on_the_first_of_january_1836()
  {
    var scenario = EighteenThirtySix.Build();

    Assert.Equal("1836", scenario.Name);
    Assert.Equal(GameDate.Of(1836, 1, 1), scenario.StartDate);
  }

  [Fact]
  public void The_human_plays_the_united_kingdom_and_the_machine_the_rest()
  {
    var world = EighteenThirtySix.Build().World;

    Assert.Equal(["United Kingdom", "United States", "Russia"], world.Countries.Select(country => country.Name));
    Assert.Equal("United Kingdom", world.Played?.Name);
    Assert.Equal(
      [CountryControl.Player, CountryControl.Ai, CountryControl.Ai],
      world.Countries.Select(country => country.Control));
  }

  [Theory]
  [InlineData("United Kingdom", 200_000, 200_000, ModeOfProduction.Capitalist)]
  [InlineData("United States", 150_000, 110_000, ModeOfProduction.Capitalist)]
  [InlineData("Russia", 180_000, 180_000, ModeOfProduction.Feudal)]
  public void Each_country_has_the_population_the_scenario_gives_it(
    string name, int population, int armable, ModeOfProduction dominantMode)
  {
    var country = Named(EighteenThirtySix.Build().World, name);

    Assert.Equal(population, country.Population);
    Assert.Equal(armable, country.Provinces.Sum(province => province.Composition.Armable));
    Assert.Equal(dominantMode, country.DominantMode);
  }

  [Fact]
  public void Each_province_stands_in_the_mode_its_classes_put_it_in()
  {
    var modes = EighteenThirtySix.Build().World.Countries
      .SelectMany(country => country.Provinces)
      .ToDictionary(province => province.Name, province => province.Mode);

    Assert.Equal(
      new Dictionary<string, ModeOfProduction>
      {
        ["Lancashire"] = ModeOfProduction.Capitalist,
        ["Kent"] = ModeOfProduction.Capitalist,
        ["New England"] = ModeOfProduction.Capitalist,
        ["Carolina"] = ModeOfProduction.Slave,
        ["Muscovy"] = ModeOfProduction.Feudal,
        ["Ukraine"] = ModeOfProduction.Feudal
      },
      modes);
  }

  /// <summary>
  /// Corn and Cloth rise by the day's product less what the people ate and
  /// wore: the surplus product. Fibre, Iron and Machinery rise by what was made
  /// less what production used up.
  /// </summary>
  [Theory]
  [InlineData("United Kingdom", 400, 20_000, 3_000, 6, 7)]
  [InlineData("United States", 90, 3_000, 21_000, 18, 6)]
  [InlineData("Russia", 150, 3_000, 1_500, 0, 0)]
  public void Each_country_produces_what_the_scenario_says_in_a_day(
    string name, int corn, int cloth, int fibre, int iron, int machinery)
  {
    var world = EighteenThirtySix.Build().World;
    var country = Named(world, name);
    var before = Holdings(world, country);

    world.Elapse(GameDate.Of(1836, 1, 2));

    var after = Holdings(world, country);
    Assert.Equal(corn, after["Corn"] - before["Corn"]);
    Assert.Equal(cloth, after["Cloth"] - before["Cloth"]);
    Assert.Equal(fibre, after["Fibre"] - before["Fibre"]);
    Assert.Equal(iron, after["Iron"] - before["Iron"]);
    Assert.Equal(machinery, after["Machinery"] - before["Machinery"]);
  }

  [Fact]
  public void No_branch_stands_idle_in_a_year_left_alone()
  {
    var session = Session.Begin(EighteenThirtySix.Build());

    var reports = session.Advance(365);

    Assert.Empty(reports.SelectMany(report => report.Of<BranchStoodIdle>()));
    // 1836 is a leap year.
    Assert.Equal(GameDate.Of(1836, 12, 31), session.Game.CurrentDate);
  }

  [Fact]
  public void After_one_day_each_commodity_is_worth_what_the_scenario_predicts()
  {
    var world = EighteenThirtySix.Build().World;

    world.Elapse(GameDate.Of(1836, 1, 2));

    Assert.Equal(
      new Dictionary<string, decimal>
      {
        ["Corn"] = 130.4536m,
        ["Cloth"] = 2.4175m,
        ["Fibre"] = 2m,
        ["Iron"] = 100m,
        ["Machinery"] = 2_100m
      },
      Values(world));
  }

  [Fact]
  public void Values_hold_steady_while_nothing_changes()
  {
    var session = Session.Begin(EighteenThirtySix.Build());

    session.Advance(1);
    var first = Values(session.World);
    session.Advance(1);
    var second = Values(session.World);
    session.Advance(28);

    Assert.Equal(first, second);
    Assert.Equal(first, Values(session.World));
  }

  [Fact]
  public void The_first_day_reports_the_values_production_moved()
  {
    var report = Assert.Single(Session.Begin(EighteenThirtySix.Build()).Advance(1));

    Assert.Equal(
      ["Cloth now holds 2.42h of labour per m, up from 2.00h.", "Corn now holds 130.45h of labour per t, up from 120.00h."],
      report.Of<CommodityWasRevalued>().Select(occurrence => occurrence.ToString()));
  }

  [Fact]
  public void After_one_day_necessary_labour_is_what_the_scenario_predicts()
  {
    var world = EighteenThirtySix.Build().World;

    world.Elapse(GameDate.Of(1836, 1, 2));

    Assert.All(
      world.Countries.SelectMany(country => country.Provinces).SelectMany(province => province.Branches),
      branch => Assert.Equal(4.4701m, decimal.Round(branch.NecessaryLabor.Hours, 4)));
  }

  [Fact]
  public void Nobody_goes_without_in_a_year_left_alone()
  {
    var reports = Session.Begin(EighteenThirtySix.Build()).Advance(365);

    Assert.Empty(reports.SelectMany(report => report.Of<PeopleWentWithout>()));
    Assert.Empty(reports.SelectMany(report => report.Of<BranchStoodIdle>()));
  }

  [Fact]
  public void Every_country_is_more_populous_after_a_year()
  {
    var session = Session.Begin(EighteenThirtySix.Build());
    var before = session.World.Countries.ToDictionary(country => country.Name, country => country.Population);

    session.Advance(365);

    Assert.All(session.World.Countries, country => Assert.True(country.Population > before[country.Name]));
  }

  [Fact]
  public void A_month_left_alone_runs_its_course_uninterrupted()
  {
    var session = Session.Begin(EighteenThirtySix.Build());

    var passage = session.AdvanceUntil(31, report => Significance.CallsForAttention(report, null, session.World));

    Assert.False(passage.Interrupted);
    Assert.Equal(GameDate.Of(1836, 2, 1), session.Game.CurrentDate);
  }

  [Fact]
  public void A_month_of_the_scenario_digests_into_a_few_lines()
  {
    var session = Session.Begin(EighteenThirtySix.Build());

    var passage = session.AdvanceUntil(31, report => Significance.CallsForAttention(report, null, session.World));
    var digest = Digest.Of(passage, session.World, UnitSystem.Metric);

    Assert.Equal(
      [
        "Cloth: 2.00 → 2.42 h per m",
        "Corn: 120.00 → 130.45 h per t",
        "Population grew: United Kingdom +200, United States +150, Russia +180.",
        "United Kingdom: 200,200 souls, 32 days of subsistence in store, 133,546 without work.",
        "Store: Corn +12,400 t, Cloth +620,000 m, Fibre +93,000 kg, Iron +186 t, Machinery +217 machine"
      ],
      digest.Lines);
  }

  [Fact]
  public void Every_build_is_a_fresh_world()
  {
    Assert.NotSame(EighteenThirtySix.Build().World, EighteenThirtySix.Build().World);
  }

  #endregion

  #region Methods

  private static Country Named(World world, string name)
  {
    return world.Countries.Single(country => country.Name == name);
  }

  /// <summary>Each commodity's value in hours per unit, to four decimals.</summary>
  private static Dictionary<string, decimal> Values(World world)
  {
    return world.Register.Commodities.ToDictionary(
      commodity => commodity.Name, commodity => decimal.Round(commodity.Value.Magnitude.Hours, 4));
  }

  private static Dictionary<string, decimal> Holdings(World world, Country country)
  {
    return world.Register.Commodities.ToDictionary(
      commodity => commodity.Name, commodity => country.Store.AmountOf(commodity.Id));
  }

  #endregion
}
