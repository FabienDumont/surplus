using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Domain.Simulation.Warfare;
using Surplus.Testing;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation;

public class WorldTests
{
  #region Fields

  private static readonly UnitOfMeasure Machine = UnitOfMeasure.Of("machine");
  private static readonly LaborTime Day = LaborTime.FromHours(12m);
  private static readonly UnitOfMeasure Pound = UnitOfMeasure.Of("lb");
  private static readonly UnitOfMeasure Tonne = UnitOfMeasure.Of("t");
  private static readonly UnitOfMeasure Yard = UnitOfMeasure.Of("yard");

  /// <summary>A basket with nothing in it, for worlds whose tests are not about subsistence.</summary>
  private static readonly Subsistence Unfed = Subsistence.Load([]);

  #endregion

  #region Tests

  [Fact]
  public void A_world_holds_the_countries_and_commodities_it_was_assembled_from()
  {
    var register = CommodityRegister.Of(new CommodityBuilder().Build());
    var france = new CountryBuilder().WithName("France").Build();
    var prussia = new CountryBuilder().WithName("Prussia").Build();

    var world = World.Assemble(register, [], Unfed, [france, prussia]);

    Assert.Same(register, world.Register);
    Assert.Equal([france, prussia], world.Countries);
    Assert.Empty(world.Wars);
  }

  [Fact]
  public void A_world_with_no_country_in_it_is_no_world()
  {
    Assert.Throws<DomainException>(() => World.Assemble(CommodityRegister.Of(), [], Unfed, []));
  }

  [Fact]
  public void The_same_country_cannot_be_in_a_world_twice()
  {
    var id = CountryId.New();
    var france = new CountryBuilder().WithId(id).Build();
    var again = new CountryBuilder().WithId(id).Build();

    Assert.Throws<DomainException>(() => World.Assemble(CommodityRegister.Of(), [], Unfed, [france, again]));
  }

  [Fact]
  public void A_province_cannot_be_governed_by_two_countries_at_once()
  {
    var alsace = new ProvinceBuilder().WithName("Alsace").Build();
    var france = new CountryBuilder().WithName("France").WithProvinces(alsace).Build();
    var prussia = new CountryBuilder().WithName("Prussia").WithProvinces(alsace).Build();

    Assert.Throws<DomainException>(() => World.Assemble(CommodityRegister.Of(), [], Unfed, [france, prussia]));
  }

  [Fact]
  public void Only_one_country_can_be_played_by_the_human()
  {
    var france = new CountryBuilder().Played().Build();
    var prussia = new CountryBuilder().Played().Build();

    Assert.Throws<DomainException>(() => World.Assemble(CommodityRegister.Of(), [], Unfed, [france, prussia]));
  }

  [Fact]
  public void Two_techniques_cannot_go_by_the_same_name()
  {
    var (register, cloth, yarn) = Weaving();

    Assert.Throws<DomainException>(
      () => World.Assemble(register, [HandLoom(cloth, yarn), HandLoom(cloth, yarn)], Unfed, [new CountryBuilder().Build()]));
  }

  [Fact]
  public void A_technique_cannot_produce_a_commodity_that_is_not_in_play()
  {
    var (register, _, yarn) = Weaving();
    var unregistered = new CommodityBuilder().WithName("Silk").Build();

    Assert.Throws<DomainException>(
      () => World.Assemble(register, [HandLoom(unregistered, yarn)], Unfed, [new CountryBuilder().Build()]));
  }

  [Fact]
  public void A_technique_turns_out_its_commodity_in_the_unit_it_is_measured_in()
  {
    var (register, cloth, _) = Weaving();
    var byThePound = Technique.Of(
      "Weighing loom", cloth.Id, ProductiveForm.Handicraft, Quantity.Of(1m, Pound), 0m, [], []);

    Assert.Throws<DomainException>(() => World.Assemble(register, [byThePound], Unfed, [new CountryBuilder().Build()]));
  }

  [Fact]
  public void A_technique_cannot_work_with_a_commodity_that_is_not_in_play()
  {
    var (register, cloth, _) = Weaving();
    var unregistered = new CommodityBuilder().WithName("Silk").WithDepartment(Department.MeansOfProduction).Build();

    Assert.Throws<DomainException>(
      () => World.Assemble(register, [HandLoom(cloth, unregistered)], Unfed, [new CountryBuilder().Build()]));
  }

  [Fact]
  public void A_technique_works_only_with_means_of_production()
  {
    var (register, cloth, _) = Weaving();
    var tailoring = Technique.Of(
      "Patchwork", cloth.Id, ProductiveForm.Handicraft, Quantity.Of(1m, Yard), 0m,
      [Stock.Of(cloth.Id, Quantity.Of(1m, Yard))], []);

    Assert.Throws<DomainException>(() => World.Assemble(register, [tailoring], Unfed, [new CountryBuilder().Build()]));
  }

  [Fact]
  public void A_world_keeps_the_basket_it_was_assembled_with()
  {
    var (register, corn, cloth, _) = Fed();
    var basket = Basket(corn, cloth);

    var world = World.Assemble(register, [], basket, [new CountryBuilder().Build()]);

    Assert.Same(basket, world.Subsistence);
  }

  [Fact]
  public void The_basket_can_only_hold_articles_of_consumption()
  {
    var (register, _, _, iron) = Fed();

    Assert.Throws<DomainException>(() => World.Assemble(
      register, [], Subsistence.Of(Stock.Of(iron.Id, Quantity.Of(0.01m, Tonne))), [new CountryBuilder().Build()]));
  }

  [Fact]
  public void The_basket_can_only_hold_commodities_in_play()
  {
    var (register, _, _, _) = Fed();
    var bread = new CommodityBuilder().WithName("Bread").Build();

    Assert.Throws<DomainException>(() => World.Assemble(
      register, [], Subsistence.Of(Stock.Of(bread.Id, Quantity.Of(1m, Pound))), [new CountryBuilder().Build()]));
  }

  [Fact]
  public void The_basket_counts_each_good_in_its_own_unit()
  {
    var (register, corn, _, _) = Fed();

    Assert.Throws<DomainException>(() => World.Assemble(
      register, [], Subsistence.Of(Stock.Of(corn.Id, Quantity.Of(50m, Pound))), [new CountryBuilder().Build()]));
  }

  [Fact]
  public void A_technique_is_found_by_its_name()
  {
    var (register, cloth, yarn) = Weaving();
    var handLoom = HandLoom(cloth, yarn);

    var world = World.Assemble(register, [handLoom], Unfed, [new CountryBuilder().Build()]);

    Assert.Equal([handLoom], world.Techniques);
    Assert.Same(handLoom, world.Technique("Hand loom"));
    Assert.Throws<DomainException>(() => world.Technique("Spinning jenny"));
  }

  [Fact]
  public void A_world_knows_which_country_the_human_plays()
  {
    var france = new CountryBuilder().Played().Build();
    var prussia = new CountryBuilder().Build();

    Assert.Same(france, new WorldBuilder().WithCountries(prussia, france).Build().Played);
    Assert.Null(new WorldBuilder().WithCountries(prussia).Build().Played);
  }

  [Fact]
  public void A_country_is_found_by_its_identity()
  {
    var france = new CountryBuilder().Build();
    var prussia = new CountryBuilder().Build();
    var world = new WorldBuilder().WithCountries(france, prussia).Build();

    Assert.Same(prussia, world.Country(prussia.Id));
  }

  [Fact]
  public void An_unknown_country_is_not_found()
  {
    Assert.Throws<DomainException>(() => new WorldBuilder().Build().Country(CountryId.New()));
  }

  [Fact]
  public void A_world_knows_who_governs_a_province()
  {
    var silesia = new ProvinceBuilder().WithName("Silesia").Build();
    var france = new CountryBuilder().Build();
    var prussia = new CountryBuilder().WithProvinces(new ProvinceBuilder().Build(), silesia).Build();
    var world = new WorldBuilder().WithCountries(france, prussia).Build();

    Assert.Same(prussia, world.CountryGoverning(silesia.Id));
  }

  [Fact]
  public void A_province_no_country_governs_has_no_governor_to_be_found()
  {
    Assert.Throws<DomainException>(() => new WorldBuilder().Build().CountryGoverning(ProvinceId.New()));
  }

  [Fact]
  public void A_saved_world_is_reloaded_exactly_as_it_was_left()
  {
    var register = CommodityRegister.Of(new CommodityBuilder().Build());
    var france = new CountryBuilder().Build();
    var prussia = new CountryBuilder().Played().Build();
    var war = new WarBuilder().WithAggressor(prussia.Id).WithDefender(france.Id).Build();

    var world = World.Load(register, [], Unfed, [france, prussia], [war], []);

    Assert.Same(register, world.Register);
    Assert.Equal([france, prussia], world.Countries);
    Assert.Equal([war], world.Wars);
    Assert.Same(prussia, world.Played);
  }

  [Fact]
  public void A_day_on_which_nothing_happens_is_reported_as_such()
  {
    var today = GameDate.Of(1836, 1, 1);

    var report = new WorldBuilder().Build().Elapse(today);

    Assert.Equal(today, report.Date);
    Assert.Empty(report.Occurrences);
  }

  [Fact]
  public void A_day_sets_every_country_to_work()
  {
    var (register, cloth, _) = Weaving();
    var britain = new CountryBuilder().WithProvinces(new ProvinceBuilder().Working(Weaver(cloth)).Build()).Build();
    var france = new CountryBuilder().WithProvinces(new ProvinceBuilder().Working(Weaver(cloth)).Build()).Build();
    var world = new WorldBuilder().WithRegister(register).WithCountries(britain, france).Build();

    world.Elapse(GameDate.Of(1836, 1, 1));

    Assert.Equal(48m, britain.Store.AmountOf(cloth.Id));
    Assert.Equal(48m, france.Store.AmountOf(cloth.Id));
  }

  [Fact]
  public void A_branch_that_stood_idle_is_reported()
  {
    var (register, cloth, yarn) = Weaving();
    var weaving = Weaver(cloth, MeansOfProduction.SetInMotion(
      ProductiveForm.Handicraft, 0m, [], [Stock.Of(yarn.Id, Quantity.Of(20m, Pound))]));
    var britain = new CountryBuilder().WithName("United Kingdom")
      .WithProvinces(new ProvinceBuilder().WithName("Lancashire").Working(weaving).Build()).Build();
    var world = new WorldBuilder().WithRegister(register).WithCountries(britain).Build();

    var report = world.Elapse(GameDate.Of(1836, 1, 1));

    var idle = Assert.Single(report.Of<BranchStoodIdle>());
    Assert.Equal(new BranchStoodIdle("United Kingdom", "Lancashire", "Cloth"), idle);
    Assert.Equal("Lancashire's Cloth branch stood idle for want of material (United Kingdom).", idle.ToString());
  }

  [Fact]
  public void Day_after_day_a_world_lays_up_its_product_and_runs_down_its_material()
  {
    var (register, cloth, yarn) = Weaving();
    var weaving = Weaver(cloth, MeansOfProduction.SetInMotion(
      ProductiveForm.Handicraft, 0m, [], [Stock.Of(yarn.Id, Quantity.Of(20m, Pound))]));
    var britain = new CountryBuilder().WithName("United Kingdom")
      .WithProvinces(new ProvinceBuilder().WithName("Lancashire").Working(weaving).Build())
      .Holding(Stock.Of(yarn.Id, Quantity.Of(70m, Pound))).Build();
    var world = new WorldBuilder().WithRegister(register).WithCountries(britain).Build();

    var date = GameDate.Of(1836, 1, 1);
    List<DayReport> reports = [];

    for (var day = 0; day < 4; day++)
    {
      reports.Add(world.Elapse(date));
      date = date.NextDay();
    }

    // Three days' yarn in the store, and a fourth day with none.
    Assert.Equal(10m, britain.Store.AmountOf(yarn.Id));
    Assert.Equal(144m, britain.Store.AmountOf(cloth.Id));
    Assert.All(reports.Take(3), report => Assert.Empty(report.Of<BranchStoodIdle>()));
    Assert.Single(reports[3].Of<BranchStoodIdle>());
  }

  [Fact]
  public void A_day_of_production_settles_what_commodities_are_worth()
  {
    var (register, cloth, _) = Weaving();
    var britain = new CountryBuilder().WithProvinces(new ProvinceBuilder().Working(Weaver(cloth)).Build()).Build();
    var world = new WorldBuilder().WithRegister(register).WithCountries(britain).Build();
    var before = cloth.Value;

    var report = world.Elapse(GameDate.Of(1836, 1, 1));

    // Two hands, twelve hours, forty-eight yards: half an hour of labour a yard.
    Assert.Equal(Hours(0.5m), cloth.Value);
    Assert.Equal(new CommodityWasRevalued("Cloth", "yard", before, Hours(0.5m)), Assert.Single(report.Of<CommodityWasRevalued>()));
  }

  [Fact]
  public void A_value_that_barely_moved_is_not_reported()
  {
    var cloth = new CommodityBuilder().WithName("Cloth")
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("clothing").WithUnit(Yard).Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(0.504m)).Build();
    var britain = new CountryBuilder().WithProvinces(new ProvinceBuilder().Working(Weaver(cloth)).Build()).Build();
    var world = new WorldBuilder().WithRegister(CommodityRegister.Of(cloth)).WithCountries(britain).Build();

    var report = world.Elapse(GameDate.Of(1836, 1, 1));

    Assert.Equal(Hours(0.5m), cloth.Value);
    Assert.Empty(report.Occurrences);
  }

  [Fact]
  public void A_cheaper_way_of_producing_lowers_the_value_for_everyone()
  {
    var (register, cloth, machinery) = PowerWeaving();
    var handLoom = Technique.Of("Hand loom", cloth.Id, ProductiveForm.Handicraft, Quantity.Of(0.5m, Yard), 0m, [], []);
    var powerLoom = Technique.Of(
      "Power loom", cloth.Id, ProductiveForm.MachineIndustry, Quantity.Of(2m, Yard), 0m,
      [Stock.Of(machinery.Id, Quantity.Of(0.5m, Machine))], []);
    var handWeavers = HandWeavers(cloth, handLoom);
    var lancashire = new ProvinceBuilder().WithName("Lancashire").Working(HandWeavers(cloth, handLoom)).Build();
    var britain = new CountryBuilder().WithProvinces(lancashire)
      .Holding(Stock.Of(machinery.Id, Quantity.Of(50m, Machine))).Build();
    var france = new CountryBuilder().WithProvinces(new ProvinceBuilder().Working(handWeavers).Build()).Build();
    var world = new WorldBuilder().WithRegister(register).WithTechniques(handLoom, powerLoom)
      .WithCountries(britain, france).Build();

    world.Elapse(GameDate.Of(1836, 1, 1));
    var byHand = cloth.Value;

    britain.Reequip(lancashire.Id, cloth.Id, powerLoom);
    world.Elapse(GameDate.Of(1836, 1, 2));

    // 600 yards by hand and 2,400 by machine, each for 1,200 hours: 0.8 hours a yard.
    Assert.Equal(Hours(2m), byHand);
    Assert.Equal(Hours(0.8m), cloth.Value);
    Assert.True(handWeavers.Work(register).IndividualValue.CompareTo(cloth.Value) > 0);
  }

  [Fact]
  public void Subsistence_costs_the_labour_contained_in_the_basket()
  {
    var (register, corn, cloth, _) = Fed();
    var world = new WorldBuilder().WithRegister(register).WithSubsistence(Basket(corn, cloth)).Build();

    // A fortieth of a tonne of corn at 100h, and half a yard of cloth at 2h.
    Assert.Equal(LaborTime.FromHours(3.5m), world.CostOfSubsistence());
  }

  [Fact]
  public void Necessary_labour_is_the_labour_contained_in_the_basket()
  {
    var (register, corn, cloth, iron) = Fed();
    var ironworks = Ironworks(iron);
    var britain = new CountryBuilder().WithProvinces(new ProvinceBuilder().Working(ironworks).Build()).Build();
    var world = new WorldBuilder().WithRegister(register).WithSubsistence(Basket(corn, cloth)).WithCountries(britain)
      .Build();

    world.Elapse(GameDate.Of(1836, 1, 2));

    Assert.Equal(LaborTime.FromHours(3.5m), ironworks.NecessaryLabor);
  }

  [Fact]
  public void Cheaper_subsistence_shortens_necessary_labour_and_lengthens_surplus_labour()
  {
    var (register, corn, cloth, iron) = Fed();
    var tillage = Technique.Of("Tillage", corn.Id, ProductiveForm.Cooperation, Quantity.Of(0.01m, Tonne), 0m, [], []);
    var farming = Technique.Of(
      "Large farming", corn.Id, ProductiveForm.Cooperation, Quantity.Of(0.02m, Tonne), 0m, [], []);
    var ironworks = Ironworks(iron);
    var farms = new BranchBuilder().Producing(corn.Id).InTheFormOf(ProductiveForm.Cooperation)
      .WorkedBy(SocialClass.Serfs, 100).WithOutputPerHour(tillage.OutputPerHour).Build();
    var kent = new ProvinceBuilder().Working(farms, ironworks).Build();
    var britain = new CountryBuilder().WithProvinces(kent).Build();
    var world = new WorldBuilder().WithRegister(register).WithTechniques(tillage, farming)
      .WithSubsistence(Basket(corn, cloth)).WithCountries(britain).Build();

    world.Elapse(GameDate.Of(1836, 1, 2));
    var before = ironworks.Work(register).Composition.SurplusValue;
    britain.Reequip(kent.Id, corn.Id, farming);
    world.Elapse(GameDate.Of(1836, 1, 3));

    // Corn falls from 100h to 50h a tonne, and the basket from 3.5h to 2.25h, in a trade that makes no corn.
    Assert.Equal(LaborTime.FromHours(2.25m), ironworks.NecessaryLabor);
    Assert.Equal(Day, ironworks.WorkingDay);
    Assert.True(ironworks.Work(register).Composition.SurplusValue.CompareTo(before) > 0);
  }

  [Fact]
  public void Necessary_labour_never_exceeds_the_working_day()
  {
    var (register, corn, cloth, iron) = Fed();
    corn.Revalue(LaborTime.FromHours(1_000m));
    var ironworks = Ironworks(iron);
    var britain = new CountryBuilder().WithProvinces(new ProvinceBuilder().Working(ironworks).Build()).Build();
    var world = new WorldBuilder().WithRegister(register).WithSubsistence(Basket(corn, cloth)).WithCountries(britain)
      .Build();

    world.Elapse(GameDate.Of(1836, 1, 2));

    // The basket is worth 26h now, and nobody works 26 hours out of 12.
    Assert.Equal(Day, ironworks.NecessaryLabor);
  }

  [Fact]
  public void A_people_grows_only_on_the_first_of_the_month()
  {
    var russia = Serfs(10_000);
    var world = new WorldBuilder().WithCountries(russia).Build();

    world.Elapse(GameDate.Of(1836, 1, 31));
    var before = russia.Population;
    world.Elapse(GameDate.Of(1836, 2, 1));

    Assert.Equal(10_000, before);
    Assert.Equal(10_010, russia.Population);
  }

  [Fact]
  public void A_people_that_went_without_today_does_not_grow()
  {
    var (register, corn, cloth, _) = Fed();
    var russia = Serfs(10_000);
    var world = new WorldBuilder().WithRegister(register).WithSubsistence(Basket(corn, cloth)).WithCountries(russia)
      .Build();

    var report = world.Elapse(GameDate.Of(1836, 2, 1));

    Assert.Equal(9_900, russia.Population);
    Assert.Empty(report.Of<PopulationGrew>());
  }

  [Fact]
  public void A_famine_is_reported()
  {
    var (register, corn, cloth, _) = Fed();
    var world = new WorldBuilder().WithRegister(register).WithSubsistence(Basket(corn, cloth))
      .WithCountries(Serfs(10_000)).Build();

    var report = world.Elapse(GameDate.Of(1836, 1, 2));

    Assert.Equal(new PeopleWentWithout("Russia", 0m, 100), Assert.Single(report.Of<PeopleWentWithout>()));
  }

  [Fact]
  public void Growth_is_reported()
  {
    var world = new WorldBuilder().WithCountries(Serfs(10_000)).Build();

    var report = world.Elapse(GameDate.Of(1836, 2, 1));

    Assert.Equal(new PopulationGrew("Russia", 10), Assert.Single(report.Of<PopulationGrew>()));
  }

  [Fact]
  public void A_people_too_few_to_grow_is_not_reported_as_growing()
  {
    var world = new WorldBuilder().WithCountries(Serfs(999)).Build();

    Assert.Empty(world.Elapse(GameDate.Of(1836, 2, 1)).Occurrences);
  }

  [Fact]
  public void Worklessness_is_reported_apart_from_shortage()
  {
    var (register, corn, cloth, _) = Fed();
    var britain = Workers(1_000);
    var world = new WorldBuilder().WithRegister(register).WithSubsistence(Basket(corn, cloth)).WithCountries(britain)
      .Build();

    var report = world.Elapse(GameDate.Of(1836, 1, 2));

    Assert.Equal(new TheWorklessWentWithout("United Kingdom", 1_000, 10), Assert.Single(report.Of<TheWorklessWentWithout>()));
    Assert.Empty(report.Of<PeopleWentWithout>());
  }

  [Fact]
  public void A_full_store_reports_no_shortage_even_when_the_workless_go_hungry()
  {
    var (register, corn, cloth, _) = Fed();
    var britain = Workers(1_000, Stock.Of(corn.Id, Quantity.Of(1_000m, Tonne)), Stock.Of(cloth.Id, Quantity.Of(1_000m, Yard)));
    var world = new WorldBuilder().WithRegister(register).WithSubsistence(Basket(corn, cloth)).WithCountries(britain)
      .Build();

    var report = world.Elapse(GameDate.Of(1836, 1, 2));

    Assert.Empty(report.Of<PeopleWentWithout>());
    Assert.Single(report.Of<TheWorklessWentWithout>());
    Assert.Equal(1_000m, britain.Store.AmountOf(corn.Id));
  }

  [Fact]
  public void Pauperism_is_reported_on_the_first_of_the_month()
  {
    var britain = Workers(10_000);
    var world = new WorldBuilder().WithCountries(britain).Build();

    var ordinary = world.Elapse(GameDate.Of(1836, 1, 31));
    var first = world.Elapse(GameDate.Of(1836, 2, 1));

    Assert.Empty(ordinary.Of<SankIntoPauperism>());
    Assert.Single(first.Of<SankIntoPauperism>());
    Assert.True(britain.Provinces[0].HeadsOf(SocialClass.Lumpenproletariat) > 0);
  }

  [Fact]
  public void A_branch_opened_through_the_world_pays_what_subsistence_costs()
  {
    var (world, russia, smelting) = Smelting();

    world.OpenBranch(russia.Id, russia.Provinces[0].Id, smelting, SocialClass.Serfs, 10, Day);

    Assert.Equal(LaborTime.FromHours(3.5m), Assert.Single(russia.Provinces[0].Branches).NecessaryLabor);
  }

  [Fact]
  public void A_branch_opened_on_a_day_shorter_than_its_hands_cost_is_charged_the_whole_day()
  {
    var (world, russia, smelting) = Smelting();

    world.OpenBranch(russia.Id, russia.Provinces[0].Id, smelting, SocialClass.Serfs, 10, LaborTime.FromHours(3m));

    Assert.Equal(LaborTime.FromHours(3m), Assert.Single(russia.Provinces[0].Branches).NecessaryLabor);
  }

  [Fact]
  public void War_is_declared_between_two_countries_of_the_world()
  {
    var (world, prussia, france) = Rivals(10_000, 1_000);

    var war = world.DeclareWar(prussia.Id, france.Id);

    Assert.Equal([war], world.Wars);
    Assert.Equal([war], world.WarsOf(france.Id));
    Assert.Equal(prussia.Id, war.Aggressor);
    Assert.Equal(0, war.Momentum);
    Assert.Equal([new WarWasDeclared("Prussia", "France")], world.Pending);
  }

  [Fact]
  public void War_cannot_be_declared_on_a_country_that_is_not_there()
  {
    var (world, prussia, _) = Rivals(10_000, 1_000);

    Assert.Throws<DomainException>(() => world.DeclareWar(prussia.Id, CountryId.New()));
    Assert.Empty(world.Wars);
  }

  [Fact]
  public void Two_countries_already_at_war_cannot_go_to_war_again()
  {
    var (world, prussia, france) = Rivals(10_000, 1_000);

    world.DeclareWar(prussia.Id, france.Id);

    Assert.Throws<DomainException>(() => world.DeclareWar(prussia.Id, france.Id));
    Assert.Throws<DomainException>(() => world.DeclareWar(france.Id, prussia.Id));
    Assert.Single(world.Wars);
  }

  [Fact]
  public void Two_countries_at_peace_again_can_go_to_war_again()
  {
    var (world, prussia, france) = Rivals(10_000, 1_000);
    var first = world.DeclareWar(prussia.Id, france.Id);
    world.MakePeace(first.Id, france.Id, Peace.White());

    var second = world.DeclareWar(france.Id, prussia.Id);

    Assert.Equal([first, second], world.Wars);
    Assert.Equal([second], world.WarsOf(prussia.Id));
  }

  [Fact]
  public void A_war_is_fought_on_the_first_of_each_month_and_on_no_other_day()
  {
    var (world, prussia, france) = Rivals(10_000, 1_000);
    var war = world.DeclareWar(prussia.Id, france.Id);

    world.Elapse(GameDate.Of(1870, 7, 31));
    var before = war.Momentum;
    world.Elapse(GameDate.Of(1870, 8, 1));

    Assert.Equal(0, before);
    Assert.NotEqual(0, war.Momentum);
  }

  [Fact]
  public void A_war_is_fought_with_what_each_society_can_put_under_arms()
  {
    var (world, prussia, france) = Rivals(10_000, 1_000);
    var war = world.DeclareWar(prussia.Id, france.Id);

    world.Elapse(GameDate.Of(1870, 8, 1));

    // Ten thousand against a thousand: twenty points of ground times nine tenths.
    Assert.Equal(18, war.Momentum);
  }

  [Fact]
  public void A_slave_society_fights_with_fewer_than_it_counts()
  {
    var carolina = Nation("Carolina", ClassPresence.Of(SocialClass.Slaves, 10_000), ClassPresence.Of(SocialClass.SlaveOwners, 1_000));
    var france = Nation("France", ClassPresence.Of(SocialClass.Serfs, 2_000));
    var world = new WorldBuilder().WithCountries(carolina, france).Build();
    var war = world.DeclareWar(carolina.Id, france.Id);
    var armable = carolina.Armable;

    world.Elapse(GameDate.Of(1861, 5, 1));

    // Eleven thousand souls, of whom a thousand can be armed, against two thousand serfs.
    Assert.Equal(1_000, armable);
    Assert.Equal(-10, war.Momentum);
  }

  [Fact]
  public void The_front_is_reported_every_month_whether_it_moved_or_not()
  {
    var (world, prussia, france) = Rivals(1_000, 1_000);
    world.DeclareWar(prussia.Id, france.Id);

    var report = world.Elapse(GameDate.Of(1870, 8, 1));

    Assert.Equal(new TheFrontMoved("Prussia", "France", 0), Assert.Single(report.Of<TheFrontMoved>()));
  }

  [Fact]
  public void A_war_that_is_over_is_no_longer_fought()
  {
    var (world, prussia, france) = Rivals(10_000, 1_000);
    var war = world.DeclareWar(prussia.Id, france.Id);
    world.MakePeace(war.Id, prussia.Id, Peace.White());

    var report = world.Elapse(GameDate.Of(1870, 8, 1));

    Assert.Empty(report.Of<TheFrontMoved>());
    Assert.Equal(0, war.Momentum);
  }

  [Fact]
  public void What_was_declared_or_signed_is_told_first_in_the_next_day_and_only_then()
  {
    var (world, prussia, france) = Rivals(10_000, 1_000);
    world.DeclareWar(prussia.Id, france.Id);

    var first = world.Elapse(GameDate.Of(1870, 7, 19));
    var second = world.Elapse(GameDate.Of(1870, 7, 20));

    Assert.Equal(new WarWasDeclared("Prussia", "France"), first.Occurrences[0]);
    Assert.Empty(second.Of<WarWasDeclared>());
    Assert.Empty(world.Pending);
  }

  [Fact]
  public void Either_side_may_simply_stop()
  {
    var (world, prussia, france, war) = AtWar(30);

    world.MakePeace(war.Id, france.Id, Peace.White());

    Assert.True(war.IsOver);
    Assert.Equal(3, france.Provinces.Count);
    Assert.Single(prussia.Provinces);
    Assert.Equal(
      "France and Prussia have made peace: nothing changes hands.", Assert.Single(world.Pending).ToString());
  }

  [Fact]
  public void Only_the_side_that_is_prevailing_dictates_terms()
  {
    var (world, _, france, war) = AtWar(30);

    Assert.Throws<DomainException>(
      () => world.MakePeace(war.Id, france.Id, Peace.Ceding(france.Provinces[0].Id)));
    Assert.False(war.IsOver);
  }

  [Fact]
  public void A_cession_moves_the_ceded_provinces_to_the_victor()
  {
    var (world, prussia, france, war) = AtWar(50);
    var alsace = france.Provinces[0];
    var lorraine = france.Provinces[1];

    world.MakePeace(war.Id, prussia.Id, Peace.Ceding(alsace.Id, lorraine.Id));

    Assert.True(prussia.Governs(alsace.Id) && prussia.Governs(lorraine.Id));
    Assert.Equal(["Paris"], france.Provinces.Select(province => province.Name));
    Assert.Equal(
      "Prussia and France have made peace: Alsace and Lorraine are ceded to Prussia.",
      Assert.Single(world.Pending).ToString());
  }

  [Fact]
  public void A_single_province_ceded_is_told_as_one()
  {
    var (world, prussia, france, war) = AtWar(30);

    world.MakePeace(war.Id, prussia.Id, Peace.Ceding(france.Provinces[0].Id));

    Assert.Equal(
      "Prussia and France have made peace: Alsace is ceded to Prussia.", Assert.Single(world.Pending).ToString());
  }

  [Fact]
  public void A_province_the_loser_does_not_govern_cannot_be_ceded()
  {
    var (world, prussia, france, war) = AtWar(50);

    Assert.Throws<DomainException>(
      () => world.MakePeace(war.Id, prussia.Id, Peace.Ceding(prussia.Provinces[0].Id)));
    Assert.False(war.IsOver);
    Assert.Equal(3, france.Provinces.Count);
    Assert.Empty(world.Pending);
  }

  [Fact]
  public void A_cession_cannot_take_a_countrys_last_province()
  {
    var prussia = Nation("Prussia", ClassPresence.Of(SocialClass.Serfs, 10_000));
    var france = Nation("France", ClassPresence.Of(SocialClass.Serfs, 1_000));
    var war = new WarBuilder().WithAggressor(prussia.Id).WithDefender(france.Id).WithMomentum(50).Build();
    var world = new WorldBuilder().WithCountries(prussia, france).WithWars(war).Build();

    Assert.Throws<DomainException>(
      () => world.MakePeace(war.Id, prussia.Id, Peace.Ceding(france.Provinces[0].Id)));
    Assert.False(war.IsOver);
    Assert.Single(france.Provinces);
  }

  [Fact]
  public void A_peace_the_fighting_has_not_earned_is_refused()
  {
    var (world, prussia, france, war) = AtWar(30);

    Assert.Throws<DomainException>(
      () => world.MakePeace(war.Id, prussia.Id, Peace.Ceding(france.Provinces[0].Id, france.Provinces[1].Id)));
    Assert.False(war.IsOver);
    Assert.Equal(3, france.Provinces.Count);
  }

  [Fact]
  public void An_annexed_country_ceases_to_exist()
  {
    var (world, prussia, france, war) = AtWar(100);

    world.MakePeace(war.Id, prussia.Id, Peace.Annexation());

    Assert.Equal([prussia], world.Countries);
    Assert.Throws<DomainException>(() => world.Country(france.Id));
    Assert.Equal(
      "Prussia and France have made peace: France is annexed by Prussia.", Assert.Single(world.Pending).ToString());
  }

  [Fact]
  public void The_victor_takes_the_annexed_countrys_provinces_and_stores()
  {
    var (register, corn, _, _) = Fed();
    var prussia = Nation("Prussia", ClassPresence.Of(SocialClass.Serfs, 10_000));
    var france = new CountryBuilder().WithName("France")
      .WithProvinces(new ProvinceBuilder().WithName("Paris").Build(), new ProvinceBuilder().WithName("Alsace").Build())
      .Holding(Stock.Of(corn.Id, Quantity.Of(500m, Tonne))).Build();
    var war = new WarBuilder().WithAggressor(prussia.Id).WithDefender(france.Id).Overwhelming().Build();
    var world = new WorldBuilder().WithRegister(register).WithCountries(prussia, france).WithWars(war).Build();

    world.MakePeace(war.Id, prussia.Id, Peace.Annexation());

    Assert.Equal(["Prussia proper", "Paris", "Alsace"], prussia.Provinces.Select(province => province.Name));
    Assert.Equal(500m, prussia.Store.AmountOf(corn.Id));
  }

  [Fact]
  public void The_other_wars_of_an_annexed_country_end_with_it()
  {
    var prussia = Nation("Prussia", ClassPresence.Of(SocialClass.Serfs, 10_000));
    var france = Nation("France", ClassPresence.Of(SocialClass.Serfs, 1_000));
    var austria = Nation("Austria", ClassPresence.Of(SocialClass.Serfs, 5_000));
    var war = new WarBuilder().WithAggressor(prussia.Id).WithDefender(france.Id).Overwhelming().Build();
    var world = new WorldBuilder().WithCountries(prussia, france, austria).WithWars(war).Build();
    var other = world.DeclareWar(austria.Id, france.Id);

    world.MakePeace(war.Id, prussia.Id, Peace.Annexation());

    Assert.True(other.IsOver);
    Assert.Equal(Peace.White(), other.Peace);
    Assert.Contains(new PeaceWasMade("Austria", "France", "France is no more"), world.Pending);
    Assert.Empty(world.WarsOf(austria.Id));
  }

  [Fact]
  public void When_the_humans_country_is_annexed_no_one_is_played()
  {
    var prussia = Nation("Prussia", ClassPresence.Of(SocialClass.Serfs, 10_000));
    var france = new CountryBuilder().WithName("France").Played().Build();
    var war = new WarBuilder().WithAggressor(prussia.Id).WithDefender(france.Id).Overwhelming().Build();
    var world = new WorldBuilder().WithCountries(prussia, france).WithWars(war).Build();

    world.MakePeace(war.Id, prussia.Id, Peace.Annexation());

    Assert.Null(world.Played);
  }

  [Fact]
  public void A_stranger_to_a_war_cannot_end_it()
  {
    var (world, _, _, war) = AtWar(30);

    Assert.Throws<DomainException>(() => world.MakePeace(war.Id, CountryId.New(), Peace.White()));
    Assert.False(war.IsOver);
  }

  [Fact]
  public void A_far_stronger_country_that_waits_long_enough_swallows_its_enemy()
  {
    var (world, prussia, france) = Rivals(10_000, 1_000);
    var war = world.DeclareWar(prussia.Id, france.Id);
    var today = GameDate.Of(1870, 7, 20);

    while (!war.Permits(Peace.Annexation()))
    {
      today = today.NextDay();
      world.Elapse(today);
    }

    world.MakePeace(war.Id, prussia.Id, Peace.Annexation());

    // Eighteen points a month: the sixth month of fighting reaches a hundred.
    Assert.Equal(GameDate.Of(1871, 1, 1), today);
    Assert.Equal([prussia], world.Countries);
  }

  [Fact]
  public void A_war_never_declared_or_already_ended_cannot_be_ended()
  {
    var (world, prussia, _, war) = AtWar(30);

    Assert.Throws<DomainException>(() => world.MakePeace(WarId.New(), prussia.Id, Peace.White()));
    world.MakePeace(war.Id, prussia.Id, Peace.White());
    Assert.Throws<DomainException>(() => world.MakePeace(war.Id, prussia.Id, Peace.White()));
  }

  #endregion

  #region Methods

  /// <summary>A one-province country of these classes, holding nothing.</summary>
  private static Country Nation(string name, params ClassPresence[] presences)
  {
    return new CountryBuilder().WithName(name)
      .WithProvinces(new ProvinceBuilder().WithName($"{name} proper").WithClasses(presences).Build()).Build();
  }

  /// <summary>Prussia and France at peace, each a single province of so many serfs.</summary>
  private static (World World, Country Prussia, Country France) Rivals(int prussians, int french)
  {
    var prussia = Nation("Prussia", ClassPresence.Of(SocialClass.Serfs, prussians));
    var france = Nation("France", ClassPresence.Of(SocialClass.Serfs, french));

    return (new WorldBuilder().WithCountries(prussia, france).Build(), prussia, france);
  }

  /// <summary>Prussia already at war with a three-province France, at this momentum.</summary>
  private static (World World, Country Prussia, Country France, War War) AtWar(int momentum)
  {
    var prussia = Nation("Prussia", ClassPresence.Of(SocialClass.Serfs, 10_000));
    var france = new CountryBuilder().WithName("France")
      .WithProvinces(
        new ProvinceBuilder().WithName("Alsace").Build(), new ProvinceBuilder().WithName("Lorraine").Build(),
        new ProvinceBuilder().WithName("Paris").Build())
      .Build();
    var war = new WarBuilder().WithAggressor(prussia.Id).WithDefender(france.Id).WithMomentum(momentum).Build();

    return (new WorldBuilder().WithCountries(prussia, france).WithWars(war).Build(), prussia, france, war);
  }

  /// <summary>A fortieth of a tonne of corn and half a yard of cloth a head.</summary>
  private static Subsistence Basket(Commodity corn, Commodity cloth)
  {
    return Subsistence.Of(Stock.Of(corn.Id, Quantity.Of(0.025m, Tonne)), Stock.Of(cloth.Id, Quantity.Of(0.5m, Yard)));
  }

  /// <summary>Corn at 100h a tonne and cloth at 2h a yard to live on, and iron at 100h a tonne to work.</summary>
  private static (CommodityRegister Register, Commodity Corn, Commodity Cloth, Commodity Iron) Fed()
  {
    var corn = new CommodityBuilder().WithName("Corn")
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("nourishment").WithUnit(Tonne).Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(100m)).Build();
    var cloth = new CommodityBuilder().WithName("Cloth")
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("clothing").WithUnit(Yard).Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(2m)).Build();
    var iron = new CommodityBuilder().WithName("Iron").WithDepartment(Department.MeansOfProduction)
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("metal to be worked").WithUnit(Tonne).Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(100m)).Build();

    return (CommodityRegister.Of(corn, cloth, iron), corn, cloth, iron);
  }

  /// <summary>A hundred serfs at the forge, a hundredth of a tonne an hour each.</summary>
  private static Branch Ironworks(Commodity iron)
  {
    return new BranchBuilder().Producing(iron.Id).InTheFormOf(ProductiveForm.Cooperation)
      .WorkedBy(SocialClass.Serfs, 100).WithOutputPerHour(Quantity.Of(0.01m, Tonne)).Build();
  }

  /// <summary>A United Kingdom of so many proletarians, none of them employed, holding these heaps.</summary>
  private static Country Workers(int heads, params Stock[] stocks)
  {
    return new CountryBuilder().WithName("United Kingdom")
      .WithProvinces(new ProvinceBuilder().WithClasses(ClassPresence.Of(SocialClass.Proletariat, heads)).Build())
      .Holding(stocks).Build();
  }

  /// <summary>A one-province Russia of so many serfs and nothing else, holding nothing.</summary>
  private static Country Serfs(int heads)
  {
    return new CountryBuilder().WithName("Russia")
      .WithProvinces(new ProvinceBuilder().WithClasses(ClassPresence.Of(SocialClass.Serfs, heads)).Build()).Build();
  }

  /// <summary>A hundred serfs in a world that knows how to smelt iron and eats the usual basket.</summary>
  private static (World World, Country Russia, Technique Smelting) Smelting()
  {
    var (register, corn, cloth, iron) = Fed();
    var smelting = Technique.Of("Smelting", iron.Id, ProductiveForm.Cooperation, Quantity.Of(0.01m, Tonne), 0m, [], []);
    var russia = Serfs(100);
    var world = new WorldBuilder().WithRegister(register).WithTechniques(smelting)
      .WithSubsistence(Basket(corn, cloth)).WithCountries(russia).Build();

    return (world, russia, smelting);
  }

  private static Value Hours(decimal hours)
  {
    return Value.CrystallisedFrom(LaborTime.FromHours(hours));
  }

  /// <summary>A hundred journeymen at a known technique.</summary>
  private static Branch HandWeavers(Commodity cloth, Technique technique)
  {
    return new BranchBuilder().Producing(cloth.Id).WithMeans(technique.EquipFor(100))
      .WorkedBy(SocialClass.Journeymen, 100).WithOutputPerHour(technique.OutputPerHour).Build();
  }

  /// <summary>Cloth worth two hours a yard, and the machinery a power loom stands on.</summary>
  private static (CommodityRegister Register, Commodity Cloth, Commodity Machinery) PowerWeaving()
  {
    var cloth = new CommodityBuilder().WithName("Cloth")
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("clothing").WithUnit(Yard).Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(2m)).Build();

    var machinery = new CommodityBuilder().WithName("Machinery").WithDepartment(Department.MeansOfProduction)
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("motive power").WithUnit(Machine).Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(2_100m)).Build();

    return (CommodityRegister.Of(cloth, machinery), cloth, machinery);
  }

  private static Technique HandLoom(Commodity cloth, Commodity yarn)
  {
    return Technique.Of(
      "Hand loom", cloth.Id, ProductiveForm.Handicraft, Quantity.Of(0.5m, Yard), 0m, [],
      [Stock.Of(yarn.Id, Quantity.Of(6m, Pound))]);
  }

  private static Branch Weaver(Commodity cloth, MeansOfProduction? means = null)
  {
    return new BranchBuilder().Producing(cloth.Id).WithMeans(means ?? MeansOfProduction.None(ProductiveForm.Handicraft))
      .WorkedBy(SocialClass.Journeymen, 2).WithOutputPerHour(Quantity.Of(2m, Yard)).Build();
  }

  private static (CommodityRegister Register, Commodity Cloth, Commodity Yarn) Weaving()
  {
    var cloth = new CommodityBuilder().WithName("Cloth")
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("clothing").WithUnit(Yard).Build()).Build();

    var yarn = new CommodityBuilder().WithName("Yarn").WithDepartment(Department.MeansOfProduction)
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("being woven").WithUnit(Pound).Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(1m)).Build();

    return (CommodityRegister.Of(cloth, yarn), cloth, yarn);
  }

  #endregion
}
