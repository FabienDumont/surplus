using Surplus.Application.Scenarios;
using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Testing;

namespace Surplus.Application.Tests;

public class SessionTests
{
  #region Fields

  private static readonly GameDate NewYear = GameDate.Of(1836, 1, 1);

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

  #endregion

  #region Methods

  private static Session Begun()
  {
    return Session.Begin(new Scenario("1836", NewYear, new WorldBuilder().Build()));
  }

  #endregion
}
