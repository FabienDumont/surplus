namespace Surplus.Domain.Simulation.Politics;

/// <summary>
/// How state power is held — how many hands, and by what title. This says
/// nothing about whose interest is served: a republic can rest on slavery, and
/// a dictatorship and an oligarchy can fly the same banner. That is why form
/// and <see cref="Ideology" /> are separate axes.
/// </summary>
public enum GovernmentForm
{
  TribalCouncil,
  AbsoluteMonarchy,
  ConstitutionalMonarchy,
  Theocracy,
  Republic,
  Oligarchy,
  Dictatorship,
  Technocracy,
  PartyState,
  CouncilRepublic,
  Commune
}
