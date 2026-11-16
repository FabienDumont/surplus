using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Testing;

namespace Surplus.Domain.Tests.Simulation;

public class WorldTests
{
  #region Fields

  private static readonly UnitOfMeasure Pound = UnitOfMeasure.Of("lb");
  private static readonly UnitOfMeasure Yard = UnitOfMeasure.Of("yard");

  #endregion

  #region Tests

  [Fact]
  public void A_world_holds_the_countries_and_commodities_it_was_assembled_from()
  {
    var register = CommodityRegister.Of(new CommodityBuilder().Build());
    var france = new CountryBuilder().WithName("France").Build();
    var prussia = new CountryBuilder().WithName("Prussia").Build();

    var world = World.Assemble(register, [], [france, prussia]);

    Assert.Same(register, world.Register);
    Assert.Equal([france, prussia], world.Countries);
    Assert.Empty(world.Wars);
  }

  [Fact]
  public void A_world_with_no_country_in_it_is_no_world()
  {
    Assert.Throws<DomainException>(() => World.Assemble(CommodityRegister.Of(), [], []));
  }

  [Fact]
  public void The_same_country_cannot_be_in_a_world_twice()
  {
    var id = CountryId.New();
    var france = new CountryBuilder().WithId(id).Build();
    var again = new CountryBuilder().WithId(id).Build();

    Assert.Throws<DomainException>(() => World.Assemble(CommodityRegister.Of(), [], [france, again]));
  }

  [Fact]
  public void A_province_cannot_be_governed_by_two_countries_at_once()
  {
    var alsace = new ProvinceBuilder().WithName("Alsace").Build();
    var france = new CountryBuilder().WithName("France").WithProvinces(alsace).Build();
    var prussia = new CountryBuilder().WithName("Prussia").WithProvinces(alsace).Build();

    Assert.Throws<DomainException>(() => World.Assemble(CommodityRegister.Of(), [], [france, prussia]));
  }

  [Fact]
  public void Only_one_country_can_be_played_by_the_human()
  {
    var france = new CountryBuilder().Played().Build();
    var prussia = new CountryBuilder().Played().Build();

    Assert.Throws<DomainException>(() => World.Assemble(CommodityRegister.Of(), [], [france, prussia]));
  }

  [Fact]
  public void Two_techniques_cannot_go_by_the_same_name()
  {
    var (register, cloth, yarn) = Weaving();

    Assert.Throws<DomainException>(
      () => World.Assemble(register, [HandLoom(cloth, yarn), HandLoom(cloth, yarn)], [new CountryBuilder().Build()]));
  }

  [Fact]
  public void A_technique_cannot_produce_a_commodity_that_is_not_in_play()
  {
    var (register, _, yarn) = Weaving();
    var unregistered = new CommodityBuilder().WithName("Silk").Build();

    Assert.Throws<DomainException>(
      () => World.Assemble(register, [HandLoom(unregistered, yarn)], [new CountryBuilder().Build()]));
  }

  [Fact]
  public void A_technique_turns_out_its_commodity_in_the_unit_it_is_measured_in()
  {
    var (register, cloth, _) = Weaving();
    var byThePound = Technique.Of(
      "Weighing loom", cloth.Id, ProductiveForm.Handicraft, Quantity.Of(1m, Pound), 0m, [], []);

    Assert.Throws<DomainException>(() => World.Assemble(register, [byThePound], [new CountryBuilder().Build()]));
  }

  [Fact]
  public void A_technique_cannot_work_with_a_commodity_that_is_not_in_play()
  {
    var (register, cloth, _) = Weaving();
    var unregistered = new CommodityBuilder().WithName("Silk").WithDepartment(Department.MeansOfProduction).Build();

    Assert.Throws<DomainException>(
      () => World.Assemble(register, [HandLoom(cloth, unregistered)], [new CountryBuilder().Build()]));
  }

  [Fact]
  public void A_technique_works_only_with_means_of_production()
  {
    var (register, cloth, _) = Weaving();
    var tailoring = Technique.Of(
      "Patchwork", cloth.Id, ProductiveForm.Handicraft, Quantity.Of(1m, Yard), 0m,
      [Stock.Of(cloth.Id, Quantity.Of(1m, Yard))], []);

    Assert.Throws<DomainException>(() => World.Assemble(register, [tailoring], [new CountryBuilder().Build()]));
  }

  [Fact]
  public void A_technique_is_found_by_its_name()
  {
    var (register, cloth, yarn) = Weaving();
    var handLoom = HandLoom(cloth, yarn);

    var world = World.Assemble(register, [handLoom], [new CountryBuilder().Build()]);

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

    var world = World.Load(register, [], [france, prussia], [war]);

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
    Assert.All(reports.Take(3), report => Assert.Empty(report.Occurrences));
    Assert.Single(reports[3].Of<BranchStoodIdle>());
  }

  #endregion

  #region Methods

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
