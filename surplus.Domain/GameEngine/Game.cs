using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.GameEngine;

/// <summary>
/// Aggregate root of the game engine side of the domain: one running play
/// session, with its clock and its pause state.
/// It knows nothing of the simulation content (countries, production, classes):
/// advancing the clock is the engine's job, reacting to each elapsed day is the
/// simulation's, orchestrated by the application layer.
/// Its state is a pure snapshot, which is what makes a saved game possible —
/// persistence itself lives outside the domain.
/// </summary>
public sealed class Game
{
    public GameId Id { get; }
    public string Name { get; }
    public GameDate CurrentDate { get; private set; }
    public GameStatus Status { get; private set; }

    private Game(GameId id, string name, GameDate currentDate, GameStatus status)
    {
        Id = id;
        Name = name;
        CurrentDate = currentDate;
        Status = status;
    }

    /// <summary>A new game starts paused, on the scenario's start date.</summary>
    public static Game Start(string name, GameDate startDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("A game must have a name.");

        return new Game(GameId.New(), name.Trim(), startDate, GameStatus.Paused);
    }

    public void Resume()
    {
        if (Status is GameStatus.Running)
            throw new DomainException("The game is already running.");

        Status = GameStatus.Running;
    }

    public void Pause()
    {
        if (Status is GameStatus.Paused)
            throw new DomainException("The game is already paused.");

        Status = GameStatus.Paused;
    }

    /// <summary>The clock only moves while the game is running, one day per tick.</summary>
    public void AdvanceOneDay()
    {
        if (Status is GameStatus.Paused)
            throw new DomainException("A paused game's clock does not move.");

        CurrentDate = CurrentDate.NextDay();
    }

    public override string ToString() => $"{Name} — {CurrentDate} ({Status})";
}
