using Surplus.Domain.GameEngine;

namespace Surplus.Application.Saving.Snapshots;

/// <summary>The engine side of a game: which game, what day, whether the clock runs.</summary>
public sealed record GameSnapshot(
  Guid Id, string Name, int Year, int Month, int Day, GameStatus Status);
