using Surplus.Application.Scenarios;
using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Domain.Simulation.Warfare;
using Surplus.Testing;

namespace Surplus.Application.Tests;

public class SessionTests
{
  #region Fields

  private static readonly GameDate NewYear = GameDate.Of(1836, 1, 1);
  private static readonly LaborTime TwelveHours = LaborTime.FromHours(12m);

  #endregion

  #region Tests

  [Fact]
  public void A_session_begins_paused_on_the_scenarios_start_date()
  {
    var world = new WorldBuilder().Build();

    var session = Session.Begin(new Scenario("1836", NewYear, world));

    Assert.Equal("1836", session.Game.Name);
    Assert.Equal(NewYear, session.Game.CurrentDate);
    Assert.Equal(GameStatus.Paused, session.Game.Status);
    Assert.Same(world, session.World);
  }

  [Fact]
  public void Advancing_moves_the_calendar_by_as_many_days_as_asked()
  {
    var session = Begun();

    session.Advance(30);

    Assert.Equal(GameDate.Of(1836, 1, 31), session.Game.CurrentDate);
  }

  [Fact]
  public void Every_day_advanced_is_reported_in_order()
  {
    var reports = Begun().Advance(3);

    Assert.Equal(
      [GameDate.Of(1836, 1, 2), GameDate.Of(1836, 1, 3), GameDate.Of(1836, 1, 4)],
      reports.Select(report => report.Date));
  }

  [Fact]
  public void A_session_is_left_paused_after_advancing()
  {
    var session = Begun();

    session.Advance(1);

    Assert.Equal(GameStatus.Paused, session.Game.Status);
  }

  [Fact]
  public void Fewer_than_one_day_cannot_be_advanced()
  {
    var session = Begun();

    Assert.Throws<DomainException>(() => session.Advance(0));
    Assert.Equal(NewYear, session.Game.CurrentDate);
    Assert.Equal(GameStatus.Paused, session.Game.Status);
  }

  [Fact]
  public void A_saved_game_is_taken_up_where_it_was_left()
  {
    var game = new GameBuilder().WithCurrentDate(GameDate.Of(1840, 6, 1)).Build();
    var world = new WorldBuilder().Build();

    var session = Session.Resume(game, world);

    Assert.Same(game, session.Game);
    Assert.Same(world, session.World);
  }

  [Fact]
  public void A_game_somehow_left_running_is_advanced_without_being_resumed_twice()
  {
    var session = Session.Resume(new GameBuilder().WithCurrentDate(NewYear).Running().Build(), new WorldBuilder().Build());

    session.Advance(2);

    Assert.Equal(GameDate.Of(1836, 1, 3), session.Game.CurrentDate);
    Assert.Equal(GameStatus.Paused, session.Game.Status);
  }

  [Fact]
  public void The_player_enacts_a_law_in_their_own_country()
  {
    var session = InEighteenThirtySix();

    session.Enact(Law.EnclosureActs);
    var enacted = Kingdom(session).Government.HasEnacted(Law.EnclosureActs);
    session.Repeal(Law.EnclosureActs);

    Assert.True(enacted);
    Assert.False(Kingdom(session).Government.HasEnacted(Law.EnclosureActs));
    Assert.All(
      session.World.Countries.Where(country => !country.IsPlayed),
      country => Assert.False(country.Government.HasEnacted(Law.EnclosureActs)));
  }

  [Fact]
  public void The_player_changes_what_the_state_is_and_what_it_avows()
  {
    var session = InEighteenThirtySix();

    session.TakeForm(GovernmentForm.Republic);
    session.Adopt(Ideology.SocialDemocracy);

    Assert.Equal(GovernmentForm.Republic, Kingdom(session).Government.Form);
    Assert.Equal(Ideology.SocialDemocracy, Kingdom(session).Government.Ideology);
  }

  [Fact]
  public void The_player_withdraws_and_restores_poor_relief()
  {
    var session = InEighteenThirtySix();

    session.WithdrawRelief();
    var withdrawn = !Kingdom(session).Government.RelievesThePoor;
    session.RelieveThePoor();

    Assert.True(withdrawn);
    Assert.True(Kingdom(session).Government.RelievesThePoor);
  }

  [Fact]
  public void The_player_opens_a_branch_out_of_their_own_stores()
  {
    var session = InEighteenThirtySix();
    var lancashire = Province(session, "Lancashire");

    session.OpenBranch(lancashire.Id, "Large farming", SocialClass.Proletariat, 300, TwelveHours);

    var farms = lancashire.Branches.Single(branch => branch.Produces == Commodity(session, "Corn").Id);
    Assert.Equal(300, farms.Hands);
    Assert.Equal(session.World.CostOfSubsistence(), farms.NecessaryLabor);
  }

  [Fact]
  public void The_player_closes_a_branch()
  {
    var session = InEighteenThirtySix();
    var kent = Province(session, "Kent");

    session.CloseBranch(kent.Id, Commodity(session, "Fibre").Id);

    Assert.DoesNotContain(kent.Branches, branch => branch.Produces == Commodity(session, "Fibre").Id);
  }

  [Fact]
  public void The_player_takes_on_and_turns_off_hands()
  {
    var session = InEighteenThirtySix();
    var kent = Province(session, "Kent");
    var corn = Commodity(session, "Corn").Id;

    session.Employ(kent.Id, corn, 1_000);
    var taken = Branch(kent, corn).Hands;
    session.LayOff(kent.Id, corn, 500);

    Assert.Equal(46_000, taken);
    Assert.Equal(45_500, Branch(kent, corn).Hands);
  }

  [Fact]
  public void The_player_reequips_a_branch_from_the_catalogue()
  {
    var session = InEighteenThirtySix();
    var kent = Province(session, "Kent");
    var corn = Commodity(session, "Corn").Id;

    session.Reequip(kent.Id, corn, "Peasant tillage");

    Assert.Equal(session.World.Technique("Peasant tillage").OutputPerHour, Branch(kent, corn).OutputPerHour);
  }

  [Fact]
  public void An_unknown_technique_cannot_be_used()
  {
    var session = InEighteenThirtySix();
    var kent = Province(session, "Kent");

    Assert.Throws<DomainException>(() => session.Reequip(kent.Id, Commodity(session, "Corn").Id, "Spinning jenny"));
  }

  [Fact]
  public void A_technique_the_country_has_not_found_cannot_be_used()
  {
    var loom = Technique.Of("Hand loom", CommodityId.New(), ProductiveForm.Handicraft, Quantity.Of(0.5m, UnitOfMeasure.Of("m")), 0m, [], []);
    var province = new ProvinceBuilder().WithClasses(ClassPresence.Of(SocialClass.Proletariat, 100)).Build();
    var country = new CountryBuilder().WithName("United Kingdom").Played().WithProvinces(province).Build();
    var session = Session.Begin(new Scenario("1836", NewYear, new WorldBuilder().WithTechniques(loom).WithCountries(country).Build()));

    var refusal = Assert.Throws<DomainException>(
      () => session.OpenBranch(province.Id, "Hand loom", SocialClass.Proletariat, 10, TwelveHours));

    Assert.Equal("United Kingdom does not know how to work by Hand loom.", refusal.Message);
  }

  [Fact]
  public void The_player_fixes_the_working_day()
  {
    var session = InEighteenThirtySix();
    var lancashire = Province(session, "Lancashire");
    var cloth = Commodity(session, "Cloth").Id;

    session.FixTheWorkingDay(lancashire.Id, cloth, LaborTime.FromHours(10m));

    Assert.Equal(LaborTime.FromHours(10m), Branch(lancashire, cloth).WorkingDay);
  }

  [Fact]
  public void The_player_declares_war_and_makes_peace()
  {
    var session = InEighteenThirtySix();
    var russia = session.World.Countries.Single(country => country.Name == "Russia");

    var war = session.DeclareWarOn(russia.Id);
    session.MakePeace(war.Id, Peace.White());

    Assert.Equal(Kingdom(session).Id, war.Aggressor);
    Assert.Equal(russia.Id, war.Defender);
    Assert.True(war.IsOver);
  }

  [Fact]
  public void A_player_whose_state_is_no_more_can_order_nothing()
  {
    var session = Begun();
    var province = ProvinceId.New();
    var commodity = CommodityId.New();

    List<Action> orders =
    [
      () => session.Enact(Law.EnclosureActs),
      () => session.Repeal(Law.EnclosureActs),
      () => session.TakeForm(GovernmentForm.Republic),
      () => session.Adopt(Ideology.Liberalism),
      () => session.RelieveThePoor(),
      () => session.WithdrawRelief(),
      () => session.OpenBranch(province, "Hand loom", SocialClass.Proletariat, 10, TwelveHours),
      () => session.CloseBranch(province, commodity),
      () => session.Employ(province, commodity, 10),
      () => session.LayOff(province, commodity, 10),
      () => session.Reequip(province, commodity, "Hand loom"),
      () => session.FixTheWorkingDay(province, commodity, TwelveHours),
      () => session.DeclareWarOn(CountryId.New()),
      () => session.MakePeace(WarId.New(), Peace.White())
    ];

    Assert.All(orders, order => Assert.Equal(
      "No one is played: the state you sat in is no more.", Assert.Throws<DomainException>(order).Message));
  }

  [Fact]
  public void Time_stops_on_the_day_something_interrupts_it()
  {
    var session = Begun();

    var passage = session.AdvanceUntil(30, report => report.Date == GameDate.Of(1836, 1, 5));

    Assert.True(passage.Interrupted);
    Assert.Equal(4, passage.Reports.Count);
    Assert.Equal(GameDate.Of(1836, 1, 5), session.Game.CurrentDate);
    Assert.Equal(GameStatus.Paused, session.Game.Status);
  }

  [Fact]
  public void Time_runs_its_full_course_when_nothing_interrupts_it()
  {
    var session = Begun();

    var passage = session.AdvanceUntil(10, _ => false);

    Assert.False(passage.Interrupted);
    Assert.Equal(10, passage.Reports.Count);
    Assert.Equal(GameDate.Of(1836, 1, 11), session.Game.CurrentDate);
  }

  [Fact]
  public void Time_cannot_be_let_pass_for_fewer_than_one_day_whatever_would_interrupt_it()
  {
    Assert.Throws<DomainException>(() => Begun().AdvanceUntil(0, _ => false));
  }

  [Fact]
  public void A_passage_remembers_the_players_store_as_it_was()
  {
    var session = InEighteenThirtySix();
    var before = Kingdom(session).Store;

    var passage = session.AdvanceUntil(3, _ => false);

    Assert.Equal(before, passage.PlayedStoreBefore);
    Assert.NotEqual(before, Kingdom(session).Store);
  }

  [Fact]
  public void A_passage_with_no_one_played_remembers_no_store()
  {
    Assert.Null(Begun().AdvanceUntil(1, _ => false).PlayedStoreBefore);
  }

  [Fact]
  public void Passages_of_the_same_days_are_the_same_passage()
  {
    var passage = Begun().AdvanceUntil(2, _ => false);
    var same = passage with { Reports = [.. passage.Reports] };

    Assert.Equal(passage, same);
    Assert.Equal(passage.GetHashCode(), same.GetHashCode());
    Assert.NotEqual(passage, passage with { Interrupted = true });
    Assert.False(passage.Equals(null));
  }

  #endregion

  #region Methods

  private static Session InEighteenThirtySix()
  {
    return Session.Begin(EighteenThirtySix.Build());
  }

  private static Country Kingdom(Session session)
  {
    return session.World.Played!;
  }

  private static Province Province(Session session, string name)
  {
    return Kingdom(session).Provinces.Single(province => province.Name == name);
  }

  private static Commodity Commodity(Session session, string name)
  {
    return session.World.Register.Commodities.Single(commodity => commodity.Name == name);
  }

  private static Branch Branch(Province province, CommodityId commodity)
  {
    return province.Branches.Single(branch => branch.Produces == commodity);
  }

  private static Session Begun()
  {
    return Session.Begin(new Scenario("1836", NewYear, new WorldBuilder().Build()));
  }

  #endregion
}
