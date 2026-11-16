using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Warfare;

namespace Surplus.Domain.Simulation;

/// <summary>
/// Aggregate root of the simulation side of the domain, as <see cref="Game" />
/// is of the engine side: every country, the commodities they produce, and the
/// wars between them, held together so that a day can pass for all of them at
/// once.
/// Countries are not left to pass their days separately because they are not
/// separate: the value of what each produces is settled across all of them, and
/// a war is a relation between two states, belonging to neither.
/// The world reads the calendar but does not keep it. The engine advances the
/// clock; the world is then told that a day has passed and answers with what
/// happened in it.
/// </summary>
public sealed class World
{
  #region Fields

  private readonly List<Country> _countries;
  private readonly List<Technique> _techniques;
  private readonly List<War> _wars;

  #endregion

  #region Properties

  public CommodityRegister Register { get; }

  /// <summary>
  /// Every way of working known in the world. A branch is equipped from this
  /// catalogue and nowhere else, so productivity cannot be raised by fiat, only
  /// by installing what a known technique calls for.
  /// </summary>
  public IReadOnlyList<Technique> Techniques => _techniques;

  public IReadOnlyList<Country> Countries => _countries;

  public IReadOnlyList<War> Wars => _wars;

  /// <summary>The country the human sits in, or null when the human's state has ceased to exist.</summary>
  public Country? Played => _countries.FirstOrDefault(country => country.IsPlayed);

  #endregion

  #region Ctors

  private World(CommodityRegister register, List<Technique> techniques, List<Country> countries, List<War> wars)
  {
    Register = register;
    _techniques = techniques;
    _countries = countries;
    _wars = wars;
  }

  #endregion

  #region Methods

  /// <summary>
  /// Brings countries together into one world, at peace. Territory is
  /// exclusive: the same ground cannot be governed by two states at once, and a
  /// state that claims it while another holds it is at war, not sharing it.
  /// Every technique must make a registered commodity, in its own unit, out of
  /// registered means of production.
  /// </summary>
  public static World Assemble(
    CommodityRegister register, IEnumerable<Technique> techniques, IEnumerable<Country> countries)
  {
    List<Technique> catalogue = [.. techniques];

    RejectUnworkableCatalogue(register, catalogue);

    List<Country> assembled = [.. countries];

    if (assembled.Count == 0)
    {
      throw new DomainException("A world with no country in it is no world.");
    }

    var twice = assembled.GroupBy(country => country.Id).FirstOrDefault(group => group.Count() > 1);

    if (twice is not null)
    {
      throw new DomainException($"{twice.First().Name} cannot be in the world twice.");
    }

    var contested = assembled
      .SelectMany(country => country.Provinces, (country, province) => (country, province))
      .GroupBy(pair => pair.province.Id)
      .FirstOrDefault(group => group.Count() > 1);

    if (contested is not null)
    {
      var claimants = contested.Select(pair => pair.country.Name).ToList();

      throw new DomainException(
        $"{contested.First().province.Name} cannot be governed by {claimants[0]} and {claimants[1]} at once.");
    }

    if (assembled.Count(country => country.IsPlayed) > 1)
    {
      throw new DomainException("The human can play only one country at a time.");
    }

    return new World(register, catalogue, assembled, []);
  }

  private static void RejectUnworkableCatalogue(CommodityRegister register, List<Technique> catalogue)
  {
    var twice = catalogue.GroupBy(technique => technique.Name).FirstOrDefault(group => group.Count() > 1);

    if (twice is not null)
    {
      throw new DomainException($"Two techniques cannot both be called {twice.Key}.");
    }

    foreach (var technique in catalogue)
    {
      var product = Registered(register, technique.Produces) ??
                    throw new DomainException($"{technique.Name} produces a commodity that is not in play.");

      if (product.UseValue.Unit != technique.OutputPerHour.Unit)
      {
        throw new DomainException(
          $"{product.Name} is measured in {product.UseValue.Unit}, and {technique.Name} turns out {technique.OutputPerHour.Unit}.");
      }

      foreach (var means in technique.InstrumentsPerHand.Concat(technique.MaterialPerHand))
      {
        var commodity = Registered(register, means.Commodity) ??
                        throw new DomainException($"{technique.Name} works with a commodity that is not in play.");

        if (commodity.Department is not Department.MeansOfProduction)
        {
          throw new DomainException(
            $"{technique.Name} cannot work with {commodity.Name}: it is consumed by people, not by production.");
        }
      }
    }
  }

  private static Commodity? Registered(CommodityRegister register, CommodityId id)
  {
    return register.Commodities.FirstOrDefault(commodity => commodity.Id == id);
  }

  /// <summary>
  /// Reconstitutes a world from a stored snapshot. Unlike <see cref="Assemble" />
  /// this asserts no invariant: the state it receives was already valid when it
  /// was saved.
  /// </summary>
  public static World Load(
    CommodityRegister register, IEnumerable<Technique> techniques, IEnumerable<Country> countries,
    IEnumerable<War> wars)
  {
    return new World(register, [.. techniques], [.. countries], [.. wars]);
  }

  public Technique Technique(string name)
  {
    return _techniques.SingleOrDefault(technique => technique.Name == name) ??
           throw new DomainException($"No technique called {name} is known.");
  }

  public Country Country(CountryId id)
  {
    return _countries.SingleOrDefault(country => country.Id == id) ??
           throw new DomainException("No country in the world goes by that identity.");
  }

  public Country CountryGoverning(ProvinceId province)
  {
    return _countries.FirstOrDefault(country => country.Governs(province)) ??
           throw new DomainException("No country in the world governs that province.");
  }

  /// <summary>Lets one day pass for every country in the world, and tells what came of it.</summary>
  public DayReport Elapse(GameDate today)
  {
    List<Occurrence> occurrences = [];
    List<Yield> yields = [];

    foreach (var country in _countries)
    {
      var day = country.WorkOneDay(Register);

      yields.AddRange(day.Yields);
      occurrences.AddRange(day.Idle.Select(idle => StoodIdle(country, idle)));
    }

    return new DayReport(today, occurrences);
  }

  private BranchStoodIdle StoodIdle(Country country, IdleBranch idle)
  {
    var province = country.Provinces.Single(province => province.Id == idle.Province);

    return new BranchStoodIdle(country.Name, province.Name, Register.Get(idle.Commodity).Name);
  }

  #endregion
}
