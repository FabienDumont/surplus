using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Testing;

namespace Surplus.Domain.Tests.GameEngine;

public class GameTests
{
  #region Tests

  [Fact]
  public void A_game_must_have_a_name()
  {
    Assert.Throws<DomainException>(() => Game.Start("   ", GameDate.Of(1836, 1, 1)));
  }

  [Fact]
  public void A_game_reads_as_its_name_date_and_state()
  {
    var game = new GameBuilder().Build();

    Assert.Equal("Workers of the world — 1836-01-01 (Paused)", game.ToString());
  }

  [Fact]
  public void A_new_game_starts_paused_on_the_scenario_start_date()
  {
    var game = Game.Start("  Workers of the world  ", GameDate.Of(1836, 1, 1));

    Assert.Equal("Workers of the world", game.Name);
    Assert.Equal(GameDate.Of(1836, 1, 1), game.CurrentDate);
    Assert.Equal(GameStatus.Paused, game.Status);
  }

  [Fact]
  public void A_paused_game_cannot_be_paused_again()
  {
    var game = new GameBuilder().Build();

    Assert.Throws<DomainException>(game.Pause);
  }

  [Fact]
  public void A_paused_games_clock_does_not_move()
  {
    var game = new GameBuilder().Build();

    Assert.Throws<DomainException>(game.AdvanceOneDay);
  }

  [Fact]
  public void A_running_game_cannot_be_resumed_again()
  {
    var game = new GameBuilder().Running().Build();

    Assert.Throws<DomainException>(game.Resume);
  }

  [Fact]
  public void A_saved_game_is_reloaded_exactly_as_it_was_left()
  {
    var id = GameId.New();

    var game = new GameBuilder().WithId(id).WithName("Workers of the world").WithCurrentDate(GameDate.Of(1848, 2, 21))
      .Running().Build();

    Assert.Equal(id, game.Id);
    Assert.Equal("Workers of the world", game.Name);
    Assert.Equal(GameDate.Of(1848, 2, 21), game.CurrentDate);
    Assert.Equal(GameStatus.Running, game.Status);
  }

  [Fact]
  public void Each_game_has_its_own_identity()
  {
    var first = Game.Start("Workers of the world", GameDate.Of(1836, 1, 1));
    var second = Game.Start("Workers of the world", GameDate.Of(1836, 1, 1));

    Assert.NotEqual(first.Id, second.Id);
  }

  [Fact]
  public void Pausing_stops_a_running_game()
  {
    var game = new GameBuilder().Running().Build();

    game.Pause();

    Assert.Equal(GameStatus.Paused, game.Status);
  }

  [Fact]
  public void Resuming_sets_the_clock_in_motion()
  {
    var game = new GameBuilder().WithCurrentDate(GameDate.Of(1836, 1, 1)).Build();

    game.Resume();
    game.AdvanceOneDay();

    Assert.Equal(GameStatus.Running, game.Status);
    Assert.Equal(GameDate.Of(1836, 1, 2), game.CurrentDate);
  }

  #endregion
}
