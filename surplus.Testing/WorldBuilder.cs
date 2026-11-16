using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Circulation;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Domain.Simulation.Warfare;

namespace Surplus.Testing;

/// <summary>
/// Builds a <see cref="World" /> directly in whatever state a test needs,
/// bypassing the checks <see cref="World.Assemble" /> makes. Defaults to one
/// country, one commodity, no technique, no war, no border, no carriage, and a
/// basket with nothing in it, so that a test which is not about subsistence has
/// nobody starve in it.
/// </summary>
public sealed class WorldBuilder
{
  #region Fields

  private List<Border> _borders = [];
  private List<Country> _countries = [new CountryBuilder().Build()];
  private Haulage _haulage = Haulage.None;
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

  public WorldBuilder WithBorders(params Border[] borders)
  {
    _borders = [.. borders];

    return this;
  }

  public WorldBuilder WithHaulage(Haulage haulage)
  {
    _haulage = haulage;

    return this;
  }

  public WorldBuilder WithWars(params War[] wars)
  {
    _wars = [.. wars];

    return this;
  }

  public World Build()
  {
    return World.Load(_register, _techniques, _subsistence, _countries, _borders, _haulage, _wars, []);
  }

  #endregion
}
