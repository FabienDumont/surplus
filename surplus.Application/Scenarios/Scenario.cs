using Surplus.Domain.GameEngine;
using Surplus.Domain.Simulation;

namespace Surplus.Application.Scenarios;

/// <summary>
/// A starting position: a world in a given state, and the day the clock starts
/// on. A scenario is not a game. It is what a game begins from, and the same
/// scenario can be begun any number of times.
/// </summary>
public sealed record Scenario(string Name, GameDate StartDate, World World);
