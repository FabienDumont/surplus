using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Tests.GameEngine;

public class GameTests
{
    private static Game NewGame() => Game.Start("Workers of the world", GameDate.Of(1836, 1, 1));

    [Fact]
    public void A_new_game_starts_paused_on_the_scenario_start_date()
    {
        var game = Game.Start("  Workers of the world  ", GameDate.Of(1836, 1, 1));

        Assert.Equal("Workers of the world", game.Name);
        Assert.Equal(GameDate.Of(1836, 1, 1), game.CurrentDate);
        Assert.Equal(GameStatus.Paused, game.Status);
    }

    [Fact]
    public void A_game_must_have_a_name()
    {
        Assert.Throws<DomainException>(() => Game.Start("   ", GameDate.Of(1836, 1, 1)));
    }

    [Fact]
    public void Each_game_has_its_own_identity()
    {
        Assert.NotEqual(NewGame().Id, NewGame().Id);
    }

    [Fact]
    public void Resuming_sets_the_clock_in_motion()
    {
        var game = NewGame();

        game.Resume();
        game.AdvanceOneDay();

        Assert.Equal(GameStatus.Running, game.Status);
        Assert.Equal(GameDate.Of(1836, 1, 2), game.CurrentDate);
    }

    [Fact]
    public void A_paused_games_clock_does_not_move()
    {
        var game = NewGame();

        Assert.Throws<DomainException>(game.AdvanceOneDay);
    }

    [Fact]
    public void Pausing_stops_a_running_game()
    {
        var game = NewGame();
        game.Resume();

        game.Pause();

        Assert.Equal(GameStatus.Paused, game.Status);
    }

    [Fact]
    public void A_paused_game_cannot_be_paused_again()
    {
        var game = NewGame();

        Assert.Throws<DomainException>(game.Pause);
    }

    [Fact]
    public void A_running_game_cannot_be_resumed_again()
    {
        var game = NewGame();
        game.Resume();

        Assert.Throws<DomainException>(game.Resume);
    }
}
