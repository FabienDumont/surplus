using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Testing;

namespace Surplus.Domain.Tests.Simulation.Countries;

public class CountryTests
{
  #region Fields

  private static readonly LaborTime Day = LaborTime.FromHours(12m);
  private static readonly UnitOfMeasure Looms = UnitOfMeasure.Of("loom");
  private static readonly LaborTime Necessary = LaborTime.FromHours(4.5m);
  private static readonly UnitOfMeasure Pound = UnitOfMeasure.Of("lb");
  private static readonly UnitOfMeasure Tonne = UnitOfMeasure.Of("t");
  private static readonly UnitOfMeasure Yard = UnitOfMeasure.Of("yard");

  #endregion

  #region Tests

  [Fact]
  public void A_country_already_played_cannot_be_given_to_the_player_again()
  {
    Assert.Throws<DomainException>(new CountryBuilder().Played().Build().GiveToPlayer);
  }

  [Fact]
  public void A_country_already_run_by_the_ai_cannot_be_given_to_it_again()
  {
    Assert.Throws<DomainException>(new CountryBuilder().Build().GiveToAi);
  }

  [Fact]
  public void A_country_cannot_cede_a_province_it_does_not_govern()
  {
    var france = new CountryBuilder().Build();

    Assert.Throws<DomainException>(() => france.Cede(ProvinceId.New()));
  }

  [Fact]
  public void A_country_governs_the_provinces_it_holds_and_no_others()
  {
    var normandie = new ProvinceBuilder().WithName("Normandie").Build();
    var france = new CountryBuilder().WithProvinces(normandie).Build();

    Assert.True(france.Governs(normandie.Id));
    Assert.False(france.Governs(ProvinceId.New()));
  }

  [Fact]
  public void A_country_is_founded_on_the_territory_it_holds_from_the_start()
  {
    var capital = new ProvinceBuilder().WithName("Île-de-France").Build();

    var france = Country.Found("  France  ", CountryControl.Ai, new GovernmentBuilder().Build(), capital);

    Assert.Equal("France", france.Name);
    Assert.Equal(CountryControl.Ai, france.Control);
    Assert.Equal([capital], france.Provinces);
  }

  [Fact]
  public void A_country_is_played_either_by_the_player_or_by_the_ai()
  {
    Assert.True(new CountryBuilder().Played().Build().IsPlayed);
    Assert.False(new CountryBuilder().Build().IsPlayed);
  }

  [Fact]
  public void A_country_must_have_a_name()
  {
    Assert.Throws<DomainException>(() => Country.Found("   ", CountryControl.Ai, new GovernmentBuilder().Build(), new ProvinceBuilder().Build()));
  }

  [Fact]
  public void A_country_reads_as_its_name_its_control_and_the_size_of_its_territory()
  {
    var one = new CountryBuilder().WithProvinces(new ProvinceBuilder().Build()).Build();
    var two = new CountryBuilder().Played().WithProvinces(new ProvinceBuilder().Build(), new ProvinceBuilder().Build())
      .Build();

    Assert.Equal("France — Liberalism Republic, Ai (1 province)", one.ToString());
    Assert.Equal("France — Liberalism Republic, Player (2 provinces)", two.ToString());
  }

  [Fact]
  public void A_country_without_territory_is_no_country()
  {
    var capital = new ProvinceBuilder().Build();
    var france = new CountryBuilder().WithProvinces(capital).Build();

    Assert.Throws<DomainException>(() => france.Cede(capital.Id));
    Assert.Equal([capital], france.Provinces);
  }

  [Fact]
  public void A_province_cannot_be_annexed_twice_by_the_same_country()
  {
    var normandie = new ProvinceBuilder().WithName("Normandie").Build();
    var france = new CountryBuilder().WithProvinces(normandie).Build();

    Assert.Throws<DomainException>(() => france.Annex(normandie));
  }

  [Fact]
  public void A_saved_country_is_reloaded_exactly_as_it_was_left()
  {
    var id = CountryId.New();
    var normandie = new ProvinceBuilder().WithName("Normandie").Build();
    var bretagne = new ProvinceBuilder().WithName("Bretagne").Build();

    var france = new CountryBuilder().WithId(id).WithName("France").Played().WithProvinces(normandie, bretagne).Build();

    Assert.Equal(id, france.Id);
    Assert.Equal("France", france.Name);
    Assert.Equal(CountryControl.Player, france.Control);
    Assert.Equal([normandie, bretagne], france.Provinces);
  }

  [Fact]
  public void Annexing_brings_a_province_under_the_countrys_rule()
  {
    var normandie = new ProvinceBuilder().WithName("Normandie").Build();
    var bretagne = new ProvinceBuilder().WithName("Bretagne").Build();
    var france = new CountryBuilder().WithProvinces(normandie).Build();

    france.Annex(bretagne);

    Assert.Equal([normandie, bretagne], france.Provinces);
  }

  [Fact]
  public void Ceding_gives_a_province_up()
  {
    var normandie = new ProvinceBuilder().WithName("Normandie").Build();
    var bretagne = new ProvinceBuilder().WithName("Bretagne").Build();
    var france = new CountryBuilder().WithProvinces(normandie, bretagne).Build();

    var ceded = france.Cede(bretagne.Id);

    Assert.Equal(bretagne, ceded);
    Assert.Equal([normandie], france.Provinces);
  }

  [Fact]
  public void Each_country_has_its_own_identity()
  {
    var first = Country.Found("France", CountryControl.Ai, new GovernmentBuilder().Build(), new ProvinceBuilder().Build());
    var second = Country.Found("France", CountryControl.Ai, new GovernmentBuilder().Build(), new ProvinceBuilder().Build());

    Assert.NotEqual(first.Id, second.Id);
  }

  [Fact]
  public void A_country_is_founded_with_empty_stores()
  {
    var france = Country.Found("France", CountryControl.Ai, new GovernmentBuilder().Build(), new ProvinceBuilder().Build());

    Assert.Equal(Store.Empty, france.Store);
  }

  [Fact]
  public void A_days_product_is_laid_up_in_the_countrys_store()
  {
    var (register, cloth, _, _) = Weaving();
    var weaving = new BranchBuilder().Producing(cloth.Id).WorkedBy(SocialClass.Journeymen, 2)
      .WithOutputPerHour(Quantity.Of(2m, Yard)).Build();
    var france = new CountryBuilder().WithProvinces(new ProvinceBuilder().Working(weaving).Build()).Build();

    var day = france.WorkOneDay(register);

    // Two hands, twelve hours, two yards an hour.
    Assert.Equal(48m, france.Store.AmountOf(cloth.Id));
    Assert.Equal(Stock.Of(cloth.Id, Quantity.Of(48m, Yard)), Assert.Single(day.Yields).Product);
    Assert.Empty(day.Idle);
  }

  [Fact]
  public void A_branch_draws_its_material_from_the_store()
  {
    var (register, cloth, yarn, _) = Weaving();
    var weaving = WeavingBranch(cloth, MeansOfProduction.SetInMotion(
      ProductiveForm.Handicraft, 0m, [], [Stock.Of(yarn.Id, Quantity.Of(20m, Pound))]));
    var france = new CountryBuilder().WithProvinces(new ProvinceBuilder().Working(weaving).Build())
      .Holding(Stock.Of(yarn.Id, Quantity.Of(50m, Pound))).Build();

    france.WorkOneDay(register);

    Assert.Equal(30m, france.Store.AmountOf(yarn.Id));
  }

  [Fact]
  public void A_branch_draws_the_worn_share_of_its_instruments_from_the_store()
  {
    var (register, cloth, _, loom) = Weaving();
    var weaving = WeavingBranch(cloth, MeansOfProduction.SetInMotion(
      ProductiveForm.Manufacture, 0.1m, [Stock.Of(loom.Id, Quantity.Of(20m, Looms))], []));
    var france = new CountryBuilder().WithProvinces(new ProvinceBuilder().Working(weaving).Build())
      .Holding(Stock.Of(loom.Id, Quantity.Of(5m, Looms))).Build();

    france.WorkOneDay(register);

    // Twenty looms worn by a tenth: two are used up and replaced out of the store.
    Assert.Equal(3m, france.Store.AmountOf(loom.Id));
  }

  [Fact]
  public void A_branch_without_its_material_stands_idle_and_takes_nothing()
  {
    var (register, cloth, yarn, loom) = Weaving();
    var weaving = WeavingBranch(cloth, MeansOfProduction.SetInMotion(
      ProductiveForm.Manufacture, 0.1m, [Stock.Of(loom.Id, Quantity.Of(20m, Looms))],
      [Stock.Of(yarn.Id, Quantity.Of(20m, Pound))]));
    var lancashire = new ProvinceBuilder().Working(weaving).Build();
    var stocks = new[] { Stock.Of(yarn.Id, Quantity.Of(10m, Pound)), Stock.Of(loom.Id, Quantity.Of(5m, Looms)) };
    var britain = new CountryBuilder().WithProvinces(lancashire).Holding(stocks).Build();

    var day = britain.WorkOneDay(register);

    Assert.Empty(day.Yields);
    Assert.Equal(new IdleBranch(lancashire.Id, cloth.Id), Assert.Single(day.Idle));
    Assert.Equal(Store.Of(stocks), britain.Store);
  }

  [Fact]
  public void What_is_produced_today_cannot_be_consumed_today()
  {
    var (register, cloth, yarn, _) = Weaving();
    var spinning = new BranchBuilder().Producing(yarn.Id).WorkedBy(SocialClass.Journeymen, 2)
      .WithOutputPerHour(Quantity.Of(1m, Pound)).Build();
    var weaving = WeavingBranch(cloth, MeansOfProduction.SetInMotion(
      ProductiveForm.Handicraft, 0m, [], [Stock.Of(yarn.Id, Quantity.Of(20m, Pound))]));
    var lancashire = new ProvinceBuilder().Working(spinning, weaving).Build();
    var britain = new CountryBuilder().WithProvinces(lancashire).Build();

    var first = britain.WorkOneDay(register);

    Assert.Equal(new IdleBranch(lancashire.Id, cloth.Id), Assert.Single(first.Idle));
    Assert.Equal(24m, britain.Store.AmountOf(yarn.Id));
    Assert.Equal(0m, britain.Store.AmountOf(cloth.Id));

    var second = britain.WorkOneDay(register);

    Assert.Empty(second.Idle);
    Assert.Equal(2, second.Yields.Count);
    Assert.Equal(28m, britain.Store.AmountOf(yarn.Id));
    Assert.Equal(48m, britain.Store.AmountOf(cloth.Id));
  }

  [Fact]
  public void A_saved_country_is_reloaded_with_its_stores()
  {
    var corn = Stock.Of(CommodityId.New(), Quantity.Of(1000m, UnitOfMeasure.Of("bushel")));

    var france = new CountryBuilder().Holding(corn).Build();

    Assert.Equal(Store.Of(corn), france.Store);
  }

  [Fact]
  public void What_a_scenario_lays_up_is_added_to_the_store()
  {
    var corn = CommodityId.New();
    var bushel = UnitOfMeasure.Of("bushel");
    var france = new CountryBuilder().Holding(Stock.Of(corn, Quantity.Of(10m, bushel))).Build();

    france.LayUp(Stock.Of(corn, Quantity.Of(5m, bushel)));

    Assert.Equal(15m, france.Store.AmountOf(corn));
  }

  [Fact]
  public void Opening_a_branch_takes_its_instruments_out_of_the_store()
  {
    var (_, cloth, yarn, loom) = Weaving();
    var lancashire = new ProvinceBuilder().Build();
    var britain = Britain(lancashire, LoomsOf(loom, 100m));

    britain.OpenBranch(lancashire.Id, PowerLoom(cloth, yarn, loom), SocialClass.Serfs, 40, Day, Necessary);

    var branch = Assert.Single(lancashire.Branches);
    Assert.Equal(cloth.Id, branch.Produces);
    Assert.Equal([LoomsOf(loom, 20m)], branch.Means.Instruments);
    Assert.Equal(80m, britain.Store.AmountOf(loom.Id));
  }

  [Fact]
  public void A_branch_cannot_be_opened_on_instruments_the_store_does_not_hold()
  {
    var (_, cloth, yarn, loom) = Weaving();
    var lancashire = new ProvinceBuilder().Build();
    var britain = Britain(lancashire, LoomsOf(loom, 10m));

    var refusal = Assert.Throws<DomainException>(
      () => britain.OpenBranch(lancashire.Id, PowerLoom(cloth, yarn, loom), SocialClass.Serfs, 40, Day, Necessary));

    Assert.Equal("There is not enough in store to equip 40 hands: 10 loom lacking.", refusal.Message);
    Assert.Empty(lancashire.Branches);
    Assert.Equal(Store.Of(LoomsOf(loom, 10m)), britain.Store);
  }

  [Fact]
  public void A_branch_cannot_be_opened_in_a_province_the_country_does_not_govern()
  {
    var (_, cloth, yarn, loom) = Weaving();
    var britain = Britain(new ProvinceBuilder().Build(), LoomsOf(loom, 100m));
    var normandie = new ProvinceBuilder().WithName("Normandie").Build();

    Assert.Throws<DomainException>(
      () => britain.OpenBranch(normandie.Id, PowerLoom(cloth, yarn, loom), SocialClass.Serfs, 40, Day, Necessary));
    Assert.Empty(normandie.Branches);
    Assert.Equal(100m, britain.Store.AmountOf(loom.Id));
  }

  [Fact]
  public void Taking_on_hands_takes_their_instruments_out_of_the_store()
  {
    var (_, cloth, yarn, loom) = Weaving();
    var lancashire = new ProvinceBuilder().Build();
    var britain = Britain(lancashire, LoomsOf(loom, 100m));
    britain.OpenBranch(lancashire.Id, PowerLoom(cloth, yarn, loom), SocialClass.Serfs, 40, Day, Necessary);

    britain.Employ(lancashire.Id, cloth.Id, 20);

    var branch = Assert.Single(lancashire.Branches);
    Assert.Equal(60, branch.Hands);
    Assert.Equal([LoomsOf(loom, 30m)], branch.Means.Instruments);
    Assert.Equal(70m, britain.Store.AmountOf(loom.Id));
  }

  [Fact]
  public void Hands_cannot_be_taken_on_without_instruments_to_give_them()
  {
    var (_, cloth, yarn, loom) = Weaving();
    var lancashire = new ProvinceBuilder().Build();
    var britain = Britain(lancashire, LoomsOf(loom, 25m));
    britain.OpenBranch(lancashire.Id, PowerLoom(cloth, yarn, loom), SocialClass.Serfs, 40, Day, Necessary);

    Assert.Throws<DomainException>(() => britain.Employ(lancashire.Id, cloth.Id, 20));

    var branch = Assert.Single(lancashire.Branches);
    Assert.Equal(40, branch.Hands);
    Assert.Equal([LoomsOf(loom, 20m)], branch.Means.Instruments);
    Assert.Equal(5m, britain.Store.AmountOf(loom.Id));
  }

  [Fact]
  public void Hands_with_bare_hands_need_nothing_from_the_store()
  {
    var (_, cloth, yarn, _) = Weaving();
    var lancashire = new ProvinceBuilder().Build();
    var britain = Britain(lancashire);

    britain.OpenBranch(lancashire.Id, HandLoom(cloth, yarn), SocialClass.Serfs, 40, Day, Necessary);
    britain.Employ(lancashire.Id, cloth.Id, 20);

    Assert.Equal(60, Assert.Single(lancashire.Branches).Hands);
    Assert.Equal(Store.Empty, britain.Store);
  }

  [Fact]
  public void Laying_off_returns_nothing_to_the_store()
  {
    var (_, cloth, yarn, loom) = Weaving();
    var lancashire = new ProvinceBuilder().Build();
    var britain = Britain(lancashire, LoomsOf(loom, 20m));
    britain.OpenBranch(lancashire.Id, PowerLoom(cloth, yarn, loom), SocialClass.Serfs, 40, Day, Necessary);

    britain.LayOff(lancashire.Id, cloth.Id, 20);

    var branch = Assert.Single(lancashire.Branches);
    Assert.Equal(20, branch.Hands);
    Assert.Equal([LoomsOf(loom, 10m)], branch.Means.Instruments);
    Assert.Equal(Store.Empty, britain.Store);
  }

  [Fact]
  public void Reequipping_takes_the_new_instruments_and_scraps_the_old()
  {
    var (_, cloth, yarn, loom) = Weaving();
    var lancashire = new ProvinceBuilder().Build();
    var britain = Britain(lancashire, LoomsOf(loom, 100m));
    britain.OpenBranch(lancashire.Id, PowerLoom(cloth, yarn, loom), SocialClass.Serfs, 40, Day, Necessary);

    britain.Reequip(lancashire.Id, cloth.Id, PowerLoom(cloth, yarn, loom, "Self-acting loom", perHand: 1m));

    // The forty looms come out of the store; the twenty they replace do not go back into it.
    Assert.Equal([LoomsOf(loom, 40m)], Assert.Single(lancashire.Branches).Means.Instruments);
    Assert.Equal(40m, britain.Store.AmountOf(loom.Id));
  }

  [Fact]
  public void A_branch_cannot_be_reequipped_to_make_something_else()
  {
    var (_, cloth, yarn, _) = Weaving();
    var lancashire = new ProvinceBuilder().Build();
    var britain = Britain(lancashire);
    britain.OpenBranch(lancashire.Id, HandLoom(cloth, yarn), SocialClass.Serfs, 40, Day, Necessary);
    var spinning = Technique.Of(
      "Spinning wheel", yarn.Id, ProductiveForm.Handicraft, Quantity.Of(1m, Pound), 0m, [], []);

    Assert.Throws<DomainException>(() => britain.Reequip(lancashire.Id, cloth.Id, spinning));
    Assert.Equal(ProductiveForm.Handicraft, Assert.Single(lancashire.Branches).Form);
  }

  [Fact]
  public void Reequipping_raises_what_an_hour_of_labour_yields()
  {
    var (register, cloth, yarn, loom) = Weaving();
    var lancashire = new ProvinceBuilder().Build();
    var britain = Britain(lancashire, LoomsOf(loom, 100m), Stock.Of(yarn.Id, Quantity.Of(2_000m, Pound)));
    britain.OpenBranch(lancashire.Id, HandLoom(cloth, yarn), SocialClass.Serfs, 40, Day, Necessary);

    var byHand = Assert.Single(britain.WorkOneDay(register).Yields).Product.Quantity;

    britain.Reequip(lancashire.Id, cloth.Id, PowerLoom(cloth, yarn, loom));

    var byMachine = Assert.Single(britain.WorkOneDay(register).Yields).Product.Quantity;

    Assert.Equal(Quantity.Of(240m, Yard), byHand);
    Assert.Equal(Quantity.Of(960m, Yard), byMachine);
    Assert.Equal(4m, byMachine.RatioTo(byHand));
  }

  [Fact]
  public void Closing_a_branch_scraps_its_instruments()
  {
    var (_, cloth, yarn, loom) = Weaving();
    var lancashire = new ProvinceBuilder().Build();
    var britain = Britain(lancashire, LoomsOf(loom, 100m));
    britain.OpenBranch(lancashire.Id, PowerLoom(cloth, yarn, loom), SocialClass.Serfs, 40, Day, Necessary);

    britain.CloseBranch(lancashire.Id, cloth.Id);

    Assert.Empty(lancashire.Branches);
    Assert.Equal(80m, britain.Store.AmountOf(loom.Id));
  }

  [Fact]
  public void A_country_fixes_the_working_day_in_its_own_provinces_only()
  {
    var (_, cloth, _, _) = Weaving();
    var weaving = WeavingBranch(cloth, MeansOfProduction.None(ProductiveForm.Handicraft));
    var lancashire = new ProvinceBuilder().Working(weaving).Build();
    var britain = Britain(lancashire);

    britain.FixTheWorkingDay(lancashire.Id, cloth.Id, LaborTime.FromHours(10m));

    Assert.Equal(LaborTime.FromHours(10m), weaving.WorkingDay);
    Assert.Throws<DomainException>(() => britain.FixTheWorkingDay(ProvinceId.New(), cloth.Id, Day));
  }

  [Fact]
  public void The_player_can_take_a_country_over_and_hand_it_back()
  {
    var france = new CountryBuilder().Build();

    france.GiveToPlayer();
    Assert.Equal(CountryControl.Player, france.Control);

    france.GiveToAi();
    Assert.Equal(CountryControl.Ai, france.Control);
  }

  [Fact]
  public void A_provided_people_takes_its_subsistence_out_of_the_store()
  {
    var (corn, cloth, basket) = Basket();
    var russia = Peopled(ClassPresence.Of(SocialClass.Serfs, 100), Corn(corn, 150m), Cloth(cloth, 300m));

    var provided = russia.Provision(basket);

    Assert.Equal(1m, provided);
    Assert.Equal(50m, russia.Store.AmountOf(corn.Id));
    Assert.Equal(100m, russia.Store.AmountOf(cloth.Id));
  }

  [Fact]
  public void A_people_short_of_one_thing_goes_short_of_everything_in_proportion()
  {
    var (corn, cloth, basket) = Basket();
    var russia = Peopled(ClassPresence.Of(SocialClass.Serfs, 100), Corn(corn, 150m), Cloth(cloth, 100m));

    var provided = russia.Provision(basket);

    // Half the cloth they need, so half the corn too, though there was corn enough.
    Assert.Equal(0.5m, provided);
    Assert.Equal(100m, russia.Store.AmountOf(corn.Id));
    Assert.Equal(0m, russia.Store.AmountOf(cloth.Id));
  }

  [Fact]
  public void The_scarcest_good_is_taken_to_the_last_grain_and_no_further()
  {
    var (corn, cloth, basket) = Basket();
    var russia = Peopled(ClassPresence.Of(SocialClass.Serfs, 3), Corn(corn, 2m), Cloth(cloth, 6m));

    var provided = russia.Provision(basket);

    // Two thirds do not divide evenly; the store must still give up exactly what it holds.
    Assert.Equal(0.6667m, decimal.Round(provided, 4));
    Assert.Equal(0m, russia.Store.AmountOf(corn.Id));
  }

  [Fact]
  public void A_country_of_no_one_wants_for_nothing()
  {
    var (_, _, basket) = Basket();
    var nobody = new CountryBuilder().WithProvinces(new ProvinceBuilder().Unpeopled().Build()).Build();

    Assert.Equal(1m, nobody.Provision(basket));
    Assert.Equal(Store.Empty, nobody.Store);
  }

  [Fact]
  public void Those_who_went_without_die_a_few_each_day()
  {
    var russia = Peopled(ClassPresence.Of(SocialClass.Serfs, 2_000));

    // Half their basket: a percent of the half who went without.
    var dead = russia.LoseTheUnprovided(0.5m);

    Assert.Equal(10, dead);
    Assert.Equal(1_990, russia.Population);
  }

  [Fact]
  public void Those_who_live_on_the_labour_of_others_do_not_starve_first()
  {
    var russia = Peopled(ClassPresence.Of(SocialClass.Serfs, 1_000), ClassPresence.Of(SocialClass.FeudalLords, 1_000));

    var dead = russia.LoseTheUnprovided(0m);

    Assert.Equal(10, dead);
    Assert.Equal(990, russia.Provinces[0].HeadsOf(SocialClass.Serfs));
    Assert.Equal(1_000, russia.Provinces[0].HeadsOf(SocialClass.FeudalLords));
  }

  [Fact]
  public void The_dead_are_struck_off_the_branches_they_worked_in()
  {
    var (_, cloth, _) = Basket();
    var tillage = new BranchBuilder().Producing(cloth.Id).WorkedBy(SocialClass.Serfs, 1_000).Build();
    var muscovy = new ProvinceBuilder().WithClasses(ClassPresence.Of(SocialClass.Serfs, 1_000)).Working(tillage).Build();
    var russia = new CountryBuilder().WithProvinces(muscovy).Build();

    russia.LoseTheUnprovided(0m);

    // No serf stood idle, so the dead are missed at the loom.
    Assert.Equal(990, tillage.Hands);
  }

  [Fact]
  public void A_fully_provided_people_loses_no_one()
  {
    var russia = Peopled(ClassPresence.Of(SocialClass.Serfs, 2_000));

    Assert.Equal(0, russia.LoseTheUnprovided(1m));
    Assert.Equal(2_000, russia.Population);
  }

  [Fact]
  public void A_people_multiplies_class_by_class()
  {
    var russia = Peopled(ClassPresence.Of(SocialClass.Serfs, 10_000), ClassPresence.Of(SocialClass.FeudalLords, 2_000));

    var born = russia.Multiply();

    Assert.Equal(12, born);
    Assert.Equal(10_010, russia.Provinces[0].HeadsOf(SocialClass.Serfs));
    Assert.Equal(2_002, russia.Provinces[0].HeadsOf(SocialClass.FeudalLords));
  }

  [Fact]
  public void A_class_too_small_to_multiply_stays_as_it_is()
  {
    var russia = Peopled(ClassPresence.Of(SocialClass.Serfs, 10_000), ClassPresence.Of(SocialClass.FeudalLords, 999));

    var born = russia.Multiply();

    Assert.Equal(10, born);
    Assert.Equal(999, russia.Provinces[0].HeadsOf(SocialClass.FeudalLords));
  }

  #endregion

  #region Methods

  /// <summary>Corn by the tonne and cloth by the yard, and a basket of one tonne and two yards a head.</summary>
  private static (Commodity Corn, Commodity Cloth, Subsistence Basket) Basket()
  {
    var corn = new CommodityBuilder().WithName("Corn")
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("nourishment").WithUnit(Tonne).Build()).Build();
    var cloth = new CommodityBuilder().WithName("Cloth")
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("clothing").WithUnit(Yard).Build()).Build();

    return (corn, cloth, Subsistence.Of(Corn(corn, 1m), Cloth(cloth, 2m)));
  }

  private static Stock Corn(Commodity corn, decimal tonnes)
  {
    return Stock.Of(corn.Id, Quantity.Of(tonnes, Tonne));
  }

  private static Stock Cloth(Commodity cloth, decimal yards)
  {
    return Stock.Of(cloth.Id, Quantity.Of(yards, Yard));
  }

  /// <summary>A one-province Russia of exactly these classes, holding exactly these heaps.</summary>
  private static Country Peopled(ClassPresence presence, params Stock[] stocks)
  {
    return Peopled([presence], stocks);
  }

  private static Country Peopled(ClassPresence first, ClassPresence second)
  {
    return Peopled([first, second], []);
  }

  private static Country Peopled(ClassPresence[] presences, Stock[] stocks)
  {
    return new CountryBuilder().WithName("Russia")
      .WithProvinces(new ProvinceBuilder().WithName("Muscovy").WithClasses(presences).Build())
      .Holding(stocks).Build();
  }

  private static Country Britain(Province province, params Stock[] stocks)
  {
    return new CountryBuilder().WithName("United Kingdom").WithProvinces(province).Holding(stocks).Build();
  }

  private static Stock LoomsOf(Commodity loom, decimal amount)
  {
    return Stock.Of(loom.Id, Quantity.Of(amount, Looms));
  }

  /// <summary>Half a yard an hour, from six pounds of yarn a day and no instrument worth counting.</summary>
  private static Technique HandLoom(Commodity cloth, Commodity yarn)
  {
    return Technique.Of(
      "Hand loom", cloth.Id, ProductiveForm.Handicraft, Quantity.Of(0.5m, Yard), 0m, [],
      [Stock.Of(yarn.Id, Quantity.Of(6m, Pound))]);
  }

  /// <summary>Two yards an hour, from twenty-four pounds of yarn a day, at a share of a loom each.</summary>
  private static Technique PowerLoom(
    Commodity cloth, Commodity yarn, Commodity loom, string name = "Power loom", decimal perHand = 0.5m)
  {
    return Technique.Of(
      name, cloth.Id, ProductiveForm.MachineIndustry, Quantity.Of(2m, Yard), 0.002m,
      [Stock.Of(loom.Id, Quantity.Of(perHand, Looms))], [Stock.Of(yarn.Id, Quantity.Of(24m, Pound))]);
  }

  private static Branch WeavingBranch(Commodity cloth, MeansOfProduction means)
  {
    return new BranchBuilder().Producing(cloth.Id).WithMeans(means).WorkedBy(SocialClass.Journeymen, 2)
      .WithOutputPerHour(Quantity.Of(2m, Yard)).Build();
  }

  /// <summary>Cloth woven from yarn on looms, each counted in its own unit.</summary>
  private static (CommodityRegister Register, Commodity Cloth, Commodity Yarn, Commodity Loom) Weaving()
  {
    var cloth = new CommodityBuilder().WithName("Cloth")
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("clothing").WithUnit(Yard).Build()).Build();

    var yarn = new CommodityBuilder().WithName("Yarn").WithDepartment(Department.MeansOfProduction)
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("being woven").WithUnit(Pound).Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(1m)).Build();

    var loom = new CommodityBuilder().WithName("Loom").WithDepartment(Department.MeansOfProduction)
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("weaving").WithUnit(Looms).Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(100m)).Build();

    return (CommodityRegister.Of(cloth, yarn, loom), cloth, yarn, loom);
  }

  #endregion
}
