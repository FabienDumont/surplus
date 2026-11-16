using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Countries;

/// <summary>
/// What one class in one province had a title to on one day, and what it got.
/// Claims and provisions are counted in baskets, so half a basket of relief is
/// half of one. <see cref="Workless" /> counts the heads with no title of their
/// own: the unemployed of a wage-earning class, and everyone who lives by
/// plunder.
/// </summary>
public sealed record Ration(
  ProvinceId Province, SocialClass Class, int Heads, int Workless, decimal Claimed, decimal Provided);
