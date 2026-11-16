namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>
/// Something that happened on a day and that a player would want to be told.
/// The world does not narrate itself: what it reports is a selection, and each
/// kind of occurrence reads as the sentence a player is given, not as the state
/// change behind it. It carries the names and figures that sentence needs
/// rather than identities, so that a report stays readable after the country or
/// province it speaks of has ceased to exist.
/// </summary>
public abstract record Occurrence;
