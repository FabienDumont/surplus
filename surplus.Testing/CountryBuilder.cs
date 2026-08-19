using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Politics;

namespace Surplus.Testing;

/// <summary>
/// Builds a <see cref="Country" /> directly in whatever state a test needs,
/// bypassing the checks <see cref="Country.Found" /> makes at founding.
/// Defaults to a one-province country left to the AI.
/// </summary>
public sealed class CountryBuilder
{
  #region Fields

  private CountryControl _control = CountryControl.Ai;
  private Government _government = new GovernmentBuilder().Build();
  private CountryId _id = CountryId.New();
  private string _name = "France";
  private List<Province> _provinces = [new ProvinceBuilder().Build()];

  #endregion

  #region Methods

  public CountryBuilder WithId(CountryId id)
  {
    _id = id;

    return this;
  }

  public CountryBuilder WithName(string name)
  {
    _name = name;

    return this;
  }

  public CountryBuilder WithGovernment(Government government)
  {
    _government = government;

    return this;
  }

  public CountryBuilder WithControl(CountryControl control)
  {
    _control = control;

    return this;
  }

  /// <summary>Shorthand for the country the human is sitting in.</summary>
  public CountryBuilder Played()
  {
    return WithControl(CountryControl.Player);
  }

  public CountryBuilder WithProvinces(params Province[] provinces)
  {
    _provinces = [.. provinces];

    return this;
  }

  public Country Build()
  {
    return Country.Load(_id, _name, _control, _government, _provinces);
  }

  #endregion
}
