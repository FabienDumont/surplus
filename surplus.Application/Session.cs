using Surplus.Application.Scenarios;
using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Application;

/// <summary>
/// One game being played: the engine's clock and the simulation's world, held
/// together. Neither knows of the other. <see cref="Game" /> advances the
/// calendar without knowing what a day holds, and <see cref="World" /> lives
/// through a day without keeping the calendar, so joining them falls to the
/// application layer.
/// A session is always paused between calls. Time passes only inside
/// <see cref="Advance" />, so whatever the player does between two advances
/// happens on a stopped clock.
/// </summary>
public sealed class Session
{
  #region Properties

  public Game Game { get; }
  public World World { get; }

  #endregion

  #region Ctors

  private Session(Game game, World world)
  {
    Game = game;
    World = world;
  }

  #endregion

  #region Methods

  /// <summary>Starts a new game from a scenario, paused on its start date.</summary>
  public static Session Begin(Scenario scenario)
  {
    return new Session(Game.Start(scenario.Name, scenario.StartDate), scenario.World);
  }

  /// <summary>Takes up a game already under way, as it was saved.</summary>
  public static Session Resume(Game game, World world)
  {
    return new Session(game, world);
  }

  /// <summary>Lets so many days pass and reports each of them.</summary>
  public IReadOnlyList<DayReport> Advance(int days)
  {
    if (days < 1)
    {
      throw new DomainException("Time is advanced by at least one day.");
    }

    if (Game.Status is GameStatus.Paused)
    {
      Game.Resume();
    }

    List<DayReport> reports = [];

    try
    {
      for (var day = 0; day < days; day++)
      {
        Game.AdvanceOneDay();
        reports.Add(World.Elapse(Game.CurrentDate));
      }
    }
    finally
    {
      Game.Pause();
    }

    return reports;
  }

  #endregion
}
