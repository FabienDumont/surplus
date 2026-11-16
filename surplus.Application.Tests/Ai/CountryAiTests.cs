using Surplus.Application.Ai;
using Surplus.Application.Scenarios;
using Surplus.Domain.GameEngine;
using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Circulation;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Domain.Simulation.Warfare;
using Surplus.Testing;

namespace Surplus.Application.Tests.Ai;

public class CountryAiTests
{
  #region Fields

  private static readonly UnitOfMeasure Load = UnitOfMeasure.Of("load");
  private static readonly UnitOfMeasure Loom = UnitOfMeasure.Of("loom");
  private static readonly UnitOfMeasure Pound = UnitOfMeasure.Of("lb");
  private static readonly UnitOfMeasure Tonne = UnitOfMeasure.Of("t");
  private static readonly UnitOfMeasure Yard = UnitOfMeasure.Of("yd");

  private readonly Commodity _carriage = Made("Carriage", Load, Department.MeansOfProduction);
  private readonly Commodity _cloth = Made("Cloth", Yard, Department.MeansOfConsumption);
  private readonly Commodity _corn = Made("Corn", Tonne, Department.MeansOfConsumption);
  private readonly Commodity _fibre = Made("Fibre", Pound, Department.MeansOfProduction);
  private readonly Commodity _loom = Made("Loom", Loom, Department.MeansOfProduction);

  #endregion

  #region Tests

  [Fact]
  public void An_ai_whose_flows_balance_does_nothing()
  {
    var (world, russia, farms) = Tillers(SerfsOf(1_000), 100);

    CountryAi.Act(world, russia);

    // A hundred hands grow twelve tonnes a day; a thousand serfs eat ten.
    Assert.Equal(100, farms.Hands);
  }

  [Fact]
  public void An_ai_short_of_corn_sets_idle_hands_to_growing_it()
  {
    var (world, russia, farms) = Tillers(SerfsOf(1_000), 50);

    CountryAi.Act(world, russia);

    Assert.True(farms.Hands > 50);
  }

  [Fact]
  public void An_ai_takes_on_no_more_hands_than_the_shortfall_calls_for()
  {
    var (world, russia, farms) = Tillers(SerfsOf(1_000), 50);

    CountryAi.Act(world, russia);

    // Four tonnes short at 0.12 a hand: thirty-four more hands, of 950 standing idle.
    Assert.Equal(84, farms.Hands);
  }

  [Fact]
  public void An_ai_takes_on_no_more_hands_than_stand_idle()
  {
    var (world, russia, farms) = Tillers(
      ClassComposition.Of(ClassPresence.Of(SocialClass.Serfs, 60), ClassPresence.Of(SocialClass.FeudalLords, 1_000)), 50);

    CountryAi.Act(world, russia);

    Assert.Equal(60, farms.Hands);
  }

  [Fact]
  public void An_ai_takes_on_no_more_hands_than_it_has_instruments_for()
  {
    var ploughs = new BranchBuilder().Producing(_corn.Id).WithOutputPerHour(Quantity.Of(0.01m, Tonne))
      .WithMeans(MeansOfProduction.SetInMotion(ProductiveForm.Cooperation, 0m, [Stock.Of(_loom.Id, Quantity.Of(50m, Loom))], []))
      .WorkedBy(SocialClass.Serfs, 50).Build();
    var kent = new ProvinceBuilder().WithClasses(ClassPresence.Of(SocialClass.Serfs, 1_000)).Working(ploughs)
      .Holding(Tonnes(1_000m), Stock.Of(_loom.Id, Quantity.Of(20m, Loom))).Build();
    var (world, russia) = Alone(kent);

    CountryAi.Act(world, russia);

    // One instrument a hand, and twenty in store.
    Assert.Equal(70, ploughs.Hands);
  }

  [Fact]
  public void An_ai_counts_the_material_its_own_branches_use()
  {
    var weaving = new BranchBuilder().Producing(_cloth.Id).WithOutputPerHour(Quantity.Of(0.5m, Yard))
      .WithMeans(MeansOfProduction.SetInMotion(ProductiveForm.Handicraft, 0m, [], [Stock.Of(_fibre.Id, Quantity.Of(300m, Pound))]))
      .WorkedBy(SocialClass.Serfs, 100).Build();
    var growing = new BranchBuilder().Producing(_fibre.Id).WithOutputPerHour(Quantity.Of(0.5m, Pound))
      .InTheFormOf(ProductiveForm.Cooperation).WorkedBy(SocialClass.Serfs, 10).Build();
    var farms = Farms(200);
    var kent = new ProvinceBuilder().WithClasses(ClassPresence.Of(SocialClass.Serfs, 1_000)).Working(farms, weaving, growing)
      .Holding(Tonnes(1_000m)).Build();
    var (world, russia) = Alone(kent);

    CountryAi.Act(world, russia);

    // The looms work up 300 lb a day; ten growers make 60. Forty more make up the rest.
    Assert.Equal(100, weaving.Hands);
    Assert.Equal(50, growing.Hands);
  }

  [Fact]
  public void An_ai_with_no_branch_for_what_it_lacks_does_nothing_and_does_not_fail()
  {
    var (world, russia, farms) = Tillers(SerfsOf(1_000), 100, Subsistence.Of(Tonnes(0.01m), Stock.Of(_cloth.Id, Quantity.Of(1m, Yard))));

    CountryAi.Act(world, russia);

    Assert.Equal(100, farms.Hands);
  }

  [Fact]
  public void An_ai_spreads_what_it_needs_over_its_provinces_in_order()
  {
    var first = Farms(50);
    var second = Farms(50);
    var muscovy = new ProvinceBuilder().WithName("Muscovy").WithClasses(ClassPresence.Of(SocialClass.Serfs, 60)).Working(first)
      .Holding(Tonnes(1_000m)).Build();
    var ukraine = new ProvinceBuilder().WithName("Ukraine").WithClasses(ClassPresence.Of(SocialClass.Serfs, 2_000)).Working(second)
      .Holding(Tonnes(1_000m)).Build();
    var (world, russia) = Alone(muscovy, ukraine);

    CountryAi.Act(world, russia);

    // Twenty and a half tonnes eaten, twelve grown: seventy-two more hands, ten of them from Muscovy.
    Assert.Equal(60, first.Hands);
    Assert.Equal(112, second.Hands);
  }

  [Fact]
  public void An_ai_keeps_carts_enough_for_what_its_neighbours_lack_and_a_fifth_more()
  {
    var carting = new BranchBuilder().Producing(_carriage.Id).WithOutputPerHour(Quantity.Of(0.1m, Load))
      .InTheFormOf(ProductiveForm.Cooperation).WorkedBy(SocialClass.Serfs, 2).Build();
    var ukraine = new ProvinceBuilder().WithName("Ukraine").WithClasses(ClassPresence.Of(SocialClass.Serfs, 1_000))
      .Working(Farms(200), carting).Holding(Tonnes(1_000m)).Build();
    var muscovy = new ProvinceBuilder().WithName("Muscovy").WithClasses(ClassPresence.Of(SocialClass.FeudalLords, 1_000))
      .Holding(Tonnes(1_000m)).Build();
    var (world, russia) = Alone(Haulage.Of(_carriage.Id, [new Bulk(_corn.Id, 1m)]), [new Border(ukraine.Id, muscovy.Id)], ukraine, muscovy);

    CountryAi.Act(world, russia);

    // Muscovy lacks ten tonnes a day, a load a tonne: twelve loads at 1.2 a carter.
    Assert.Equal(10, carting.Hands);
  }

  [Fact]
  public void A_province_whose_neighbours_lack_nothing_takes_on_no_carters()
  {
    var carting = new BranchBuilder().Producing(_carriage.Id).WithOutputPerHour(Quantity.Of(0.1m, Load))
      .InTheFormOf(ProductiveForm.Cooperation).WorkedBy(SocialClass.Serfs, 2).Build();
    var ukraine = new ProvinceBuilder().WithName("Ukraine").WithClasses(ClassPresence.Of(SocialClass.Serfs, 1_000))
      .Working(Farms(200), carting).Holding(Tonnes(1_000m)).Build();
    var muscovy = new ProvinceBuilder().WithName("Muscovy").WithClasses(ClassPresence.Of(SocialClass.Serfs, 1_000))
      .Working(Farms(100)).Holding(Tonnes(1_000m)).Build();
    var (world, russia) = Alone(Haulage.Of(_carriage.Id, [new Bulk(_corn.Id, 1m)]), [new Border(ukraine.Id, muscovy.Id)], ukraine, muscovy);

    CountryAi.Act(world, russia);

    Assert.Equal(2, carting.Hands);
  }

  [Fact]
  public void An_ai_that_has_broken_its_enemy_swallows_it()
  {
    var (world, prussia, france, war) = AtWar(100, 3);

    CountryAi.Act(world, prussia);

    Assert.Equal(Peace.Annexation(), war.Peace);
    Assert.DoesNotContain(france, world.Countries);
  }

  [Fact]
  public void An_ai_well_ahead_takes_the_provinces_within_its_reach()
  {
    var (world, prussia, france, war) = AtWar(50, 3);
    var reached = france.Provinces.Take(2).Select(province => province.Id).ToList();

    CountryAi.Act(world, prussia);

    Assert.Equal(Peace.Ceding([.. reached]), war.Peace);
    Assert.All(reached, province => Assert.True(prussia.Governs(province)));
  }

  [Fact]
  public void An_ai_never_takes_its_enemys_last_province_short_of_annexation()
  {
    var (world, prussia, france, war) = AtWar(75, 3);

    CountryAi.Act(world, prussia);

    Assert.Equal(2, war.Peace!.Ceded.Count);
    Assert.Single(france.Provinces);
  }

  [Fact]
  public void An_ai_facing_a_single_province_waits_for_annexation()
  {
    var (world, prussia, _, war) = AtWar(75, 1);

    CountryAi.Act(world, prussia);

    Assert.False(war.IsOver);
  }

  [Fact]
  public void An_ai_barely_ahead_fights_on()
  {
    var (world, prussia, _, war) = AtWar(30, 3);

    CountryAi.Act(world, prussia);

    Assert.False(war.IsOver);
  }

  [Fact]
  public void An_ai_that_is_losing_does_not_sue_for_peace()
  {
    var (world, prussia, _, war) = AtWar(-75, 3);

    CountryAi.Act(world, prussia);

    Assert.False(war.IsOver);
  }

  [Fact]
  public void The_ai_acts_on_the_first_of_the_month_and_on_no_other_day()
  {
    var (world, _, farms) = Tillers(SerfsOf(1_000), 50);
    var session = Session.Begin(new Scenario("1836", GameDate.Of(1836, 1, 1), world));

    session.Advance(30);
    var before = farms.Hands;
    session.Advance(1);

    Assert.Equal(50, before);
    Assert.True(farms.Hands > 50);
  }

  [Fact]
  public void The_ai_never_acts_for_the_country_the_human_plays()
  {
    var farms = Farms(50);
    var kent = new ProvinceBuilder().WithComposition(SerfsOf(1_000)).Working(farms).Holding(Tonnes(1_000m)).Build();
    var britain = new CountryBuilder().Played().WithProvinces(kent).Build();
    var world = new WorldBuilder().WithRegister(Register()).WithSubsistence(Subsistence.Of(Tonnes(0.01m)))
      .WithCountries(britain).Build();
    var session = Session.Begin(new Scenario("1836", GameDate.Of(1836, 1, 1), world));

    session.Advance(31);

    Assert.Equal(50, farms.Hands);
  }

  #endregion

  #region Methods

  private static Commodity Made(string name, UnitOfMeasure unit, Department department)
  {
    return new CommodityBuilder().WithName(name).WithDepartment(department)
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant(name.ToLowerInvariant()).WithUnit(unit).Build()).Build();
  }

  private CommodityRegister Register()
  {
    return CommodityRegister.Of(_corn, _cloth, _fibre, _loom, _carriage);
  }

  private static ClassComposition SerfsOf(int heads)
  {
    return ClassComposition.Of(ClassPresence.Of(SocialClass.Serfs, heads));
  }

  /// <summary>So many serfs at the plough, each growing 0.12 t a day.</summary>
  private Branch Farms(int hands)
  {
    return new BranchBuilder().Producing(_corn.Id).WithOutputPerHour(Quantity.Of(0.01m, Tonne))
      .InTheFormOf(ProductiveForm.Cooperation).WorkedBy(SocialClass.Serfs, hands).Build();
  }

  /// <summary>
  /// A one-province Russia of these classes, a corn branch of so many hands,
  /// plenty of corn in store, and a basket of a hundredth of a tonne a head.
  /// </summary>
  private (World World, Country Russia, Branch Farms) Tillers(
    ClassComposition composition, int hands, Subsistence? basket = null)
  {
    var farms = Farms(hands);
    var kent = new ProvinceBuilder().WithComposition(composition).Working(farms).Holding(Tonnes(1_000m)).Build();
    var russia = new CountryBuilder().WithName("Russia").WithProvinces(kent).Build();
    var world = new WorldBuilder().WithRegister(Register()).WithSubsistence(basket ?? Subsistence.Of(Tonnes(0.01m)))
      .WithCountries(russia).Build();

    return (world, russia, farms);
  }

  private (World World, Country Russia) Alone(params Province[] provinces)
  {
    return Alone(Haulage.None, [], provinces);
  }

  private (World World, Country Russia) Alone(Haulage haulage, Border[] borders, params Province[] provinces)
  {
    var russia = new CountryBuilder().WithName("Russia").WithProvinces(provinces).Build();
    var world = new WorldBuilder().WithRegister(Register()).WithSubsistence(Subsistence.Of(Tonnes(0.01m)))
      .WithHaulage(haulage).WithBorders(borders).WithCountries(russia).Build();

    return (world, russia);
  }

  /// <summary>Prussia at war on a France of so many provinces, at this momentum.</summary>
  private static (World World, Country Prussia, Country France, War War) AtWar(int momentum, int provinces)
  {
    var prussia = new CountryBuilder().WithName("Prussia").Build();
    var france = new CountryBuilder().WithName("France")
      .WithProvinces([.. Enumerable.Range(1, provinces).Select(index => new ProvinceBuilder().WithName($"Province {index}").Build())])
      .Build();
    var war = new WarBuilder().WithAggressor(prussia.Id).WithDefender(france.Id).WithMomentum(momentum).Build();

    return (new WorldBuilder().WithCountries(prussia, france).WithWars(war).Build(), prussia, france, war);
  }

  private Stock Tonnes(decimal amount)
  {
    return Stock.Of(_corn.Id, Quantity.Of(amount, Tonne));
  }

  #endregion
}
