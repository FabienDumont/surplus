using Surplus.Application.Scenarios;
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
  /// Nobody eats or wears anything yet, so Corn and Cloth rise by the whole
  /// day's product. Fibre, Iron and Machinery rise by what was made less what
  /// production used up.
  /// </summary>
  [Theory]
  [InlineData("United Kingdom", 5_400, 120_000, 3_000, 6, 7)]
  [InlineData("United States", 3_840, 78_000, 21_000, 18, 6)]
  [InlineData("Russia", 4_650, 93_000, 1_500, 0, 0)]
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

  private static Dictionary<string, decimal> Holdings(World world, Country country)
  {
    return world.Register.Commodities.ToDictionary(
      commodity => commodity.Name, commodity => country.Store.AmountOf(commodity.Id));
  }

  #endregion
}
