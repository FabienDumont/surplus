using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Production;

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
  private Knowledge _knowledge = Knowledge.None;
  private List<Province> _provinces = [new ProvinceBuilder().Build()];
  private List<Stock> _stocks = [];

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

  /// <summary>Knows exactly these techniques, by name, and has nothing under way.</summary>
  public CountryBuilder Knowing(params string[] techniques)
  {
    _knowledge = Knowledge.Of(techniques, []);

    return this;
  }

  public CountryBuilder WithKnowledge(Knowledge knowledge)
  {
    _knowledge = knowledge;

    return this;
  }

  /// <summary>
  /// Lays these heaps up in the country's first province, on top of whatever
  /// that province already holds. Stores are kept by province; a test that
  /// needs heaps elsewhere lays them up in the province itself.
  /// </summary>
  public CountryBuilder Holding(params Stock[] stocks)
  {
    _stocks = [.. stocks];

    return this;
  }

  public Country Build()
  {
    foreach (var stock in _stocks)
    {
      _provinces[0].LayUp(stock);
    }

    return Country.Load(_id, _name, _control, _government, _provinces, _knowledge);
  }

  #endregion
}
