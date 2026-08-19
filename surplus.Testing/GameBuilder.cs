using Surplus.Domain.GameEngine;

namespace Surplus.Testing;

/// <summary>
/// Builds a <see cref="Game" /> directly in whatever state a test needs, without
/// having to drive it there through the engine's behaviour.
/// </summary>
public sealed class GameBuilder
{
  #region Fields

  private GameDate _currentDate = GameDate.Of(1836, 1, 1);
  private GameId _id = GameId.New();
  private string _name = "Workers of the world";
  private GameStatus _status = GameStatus.Paused;

  #endregion

  #region Methods

  public GameBuilder WithId(GameId id)
  {
    _id = id;

    return this;
  }

  public GameBuilder WithName(string name)
  {
    _name = name;

    return this;
  }

  public GameBuilder WithCurrentDate(GameDate currentDate)
  {
    _currentDate = currentDate;

    return this;
  }

  public GameBuilder WithStatus(GameStatus status)
  {
    _status = status;

    return this;
  }

  /// <summary>Shorthand for a game whose clock is already in motion.</summary>
  public GameBuilder Running()
  {
    return WithStatus(GameStatus.Running);
  }

  public Game Build()
  {
    return Game.Load(_id, _name, _currentDate, _status);
  }

  #endregion
}
