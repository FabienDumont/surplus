using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Domain.Simulation.Warfare;

namespace Surplus.Testing;

/// <summary>
/// Builds a <see cref="World" /> directly in whatever state a test needs,
/// bypassing the checks <see cref="World.Assemble" /> makes. Defaults to one
/// country, one commodity, no technique, no war, and a basket with nothing in
/// it, so that a test which is not about subsistence has nobody starve in it.
/// </summary>
public sealed class WorldBuilder
{
  #region Fields

  private List<Country> _countries = [new CountryBuilder().Build()];
  private CommodityRegister _register = CommodityRegister.Of(new CommodityBuilder().Build());
  private Subsistence _subsistence = Subsistence.Load([]);
  private List<Technique> _techniques = [];
  private List<War> _wars = [];

  #endregion

  #region Methods

  public WorldBuilder WithRegister(CommodityRegister register)
  {
    _register = register;

    return this;
  }

  public WorldBuilder WithCountries(params Country[] countries)
  {
    _countries = [.. countries];

    return this;
  }

  public WorldBuilder WithTechniques(params Technique[] techniques)
  {
    _techniques = [.. techniques];

    return this;
  }

  public WorldBuilder WithSubsistence(Subsistence subsistence)
  {
    _subsistence = subsistence;

    return this;
  }

  public WorldBuilder WithWars(params War[] wars)
  {
    _wars = [.. wars];

    return this;
  }

  public World Build()
  {
    return World.Load(_register, _techniques, _subsistence, _countries, _wars);
  }

  #endregion
}
