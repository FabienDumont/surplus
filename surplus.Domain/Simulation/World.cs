using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
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

  /// <summary>
  /// The smallest movement of value worth telling a player about. Smaller ones
  /// are applied all the same: value is not rounded to spare the reader.
  /// </summary>
  private static readonly decimal ReportedMovement = 0.01m;

  private readonly List<Country> _countries;

  /// <summary>
  /// What was done between two days, by declaration or treaty, and is told in
  /// the report of the next one. A war is declared on a stopped clock, but it
  /// belongs to the chronicle of the day it begins.
  /// </summary>
  private readonly List<Occurrence> _pending;

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

  /// <summary>Every war ever declared, concluded ones included as a record.</summary>
  public IReadOnlyList<War> Wars => _wars;

  /// <summary>Declarations and treaties made since the last day, to be told in the next.</summary>
  public IReadOnlyList<Occurrence> Pending => _pending;

  /// <summary>
  /// What every head takes out of its country's store each day, the same for
  /// every class. Its value is the necessary labour of every branch in the world.
  /// </summary>
  public Subsistence Subsistence { get; }

  /// <summary>The country the human sits in, or null when the human's state has ceased to exist.</summary>
  public Country? Played => _countries.FirstOrDefault(country => country.IsPlayed);

  #endregion

  #region Ctors

  private World(
    CommodityRegister register, List<Technique> techniques, Subsistence subsistence, List<Country> countries,
    List<War> wars, List<Occurrence> pending)
  {
    Register = register;
    _techniques = techniques;
    Subsistence = subsistence;
    _countries = countries;
    _wars = wars;
    _pending = pending;
  }

  #endregion

  #region Methods

  /// <summary>
  /// Brings countries together into one world, at peace. Territory is
  /// exclusive: the same ground cannot be governed by two states at once, and a
  /// state that claims it while another holds it is at war, not sharing it.
  /// Every technique must make a registered commodity, in its own unit, out of
  /// registered means of production. The basket can hold only articles of
  /// consumption, each in the unit it is measured in.
  /// </summary>
  public static World Assemble(
    CommodityRegister register, IEnumerable<Technique> techniques, Subsistence subsistence,
    IEnumerable<Country> countries)
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

    RejectUnfitBasket(register, subsistence);

    return new World(register, catalogue, subsistence, assembled, [], []);
  }

  private static void RejectUnfitBasket(CommodityRegister register, Subsistence subsistence)
  {
    foreach (var stock in subsistence.PerHead)
    {
      var commodity = Registered(register, stock.Commodity) ??
                      throw new DomainException("The basket holds a commodity that is not in play.");

      if (commodity.Department is not Department.MeansOfConsumption)
      {
        throw new DomainException(
          $"{commodity.Name} cannot be in the basket: it is consumed by production, not by people.");
      }

      if (commodity.UseValue.Unit != stock.Quantity.Unit)
      {
        throw new DomainException(
          $"{commodity.Name} is measured in {commodity.UseValue.Unit}, and the basket counts it in {stock.Quantity.Unit}.");
      }
    }
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
    CommodityRegister register, IEnumerable<Technique> techniques, Subsistence subsistence,
    IEnumerable<Country> countries, IEnumerable<War> wars, IEnumerable<Occurrence> pending)
  {
    return new World(register, [.. techniques], subsistence, [.. countries], [.. wars], [.. pending]);
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

  /// <summary>
  /// The value of one basket at today's values: the labour a day of one
  /// person's life costs society to reproduce, and so the necessary labour in
  /// every working day.
  /// </summary>
  public LaborTime CostOfSubsistence()
  {
    return Register.ValueOf(Subsistence.PerHead).Magnitude;
  }

  /// <summary>
  /// Sets a branch going without the caller having to say what a hand costs to
  /// keep: necessary labour is not anyone's to choose, it is the value of the
  /// basket, and never more than the day it is worked out of.
  /// </summary>
  public void OpenBranch(
    CountryId country, ProvinceId province, Technique technique, SocialClass workforce, int hands,
    LaborTime workingDay)
  {
    Country(country).OpenBranch(province, technique, workforce, hands, workingDay, Lesser(CostOfSubsistence(), workingDay));
  }

  /// <summary>The wars a country is fighting now; concluded ones are left out.</summary>
  public IReadOnlyList<War> WarsOf(CountryId country)
  {
    return [.. _wars.Where(war => !war.IsOver && (war.Aggressor == country || war.Defender == country))];
  }

  /// <summary>
  /// Opens a war between two states of the world. A state already at war with
  /// another is not at war with it twice: the war that exists is the one to
  /// win or end.
  /// </summary>
  public War DeclareWar(CountryId aggressor, CountryId defender)
  {
    var attacking = Country(aggressor);
    var attacked = Country(defender);

    if (WarsOf(aggressor).Any(war => war.Aggressor == defender || war.Defender == defender))
    {
      throw new DomainException($"{attacking.Name} and {attacked.Name} are already at war.");
    }

    var war = War.Declare(aggressor, defender);

    _wars.Add(war);
    _pending.Add(new WarWasDeclared(attacking.Name, attacked.Name));

    return war;
  }

  /// <summary>
  /// Ends a war on the terms one of its belligerents proposes, and applies
  /// them. Everything is checked before anything is changed, so a peace refused
  /// leaves the world exactly as it was. Either side may simply stop, but only
  /// the side that is prevailing can take anything, and it takes no more than
  /// the fighting earned.
  /// </summary>
  public void MakePeace(WarId warId, CountryId proposer, Peace peace)
  {
    var war = _wars.SingleOrDefault(candidate => candidate.Id == warId) ??
              throw new DomainException("No such war has been declared.");

    if (war.IsOver)
    {
      throw new DomainException("This war has already been brought to an end.");
    }

    if (proposer != war.Aggressor && proposer != war.Defender)
    {
      throw new DomainException("Only those who fight a war can end it.");
    }

    var proposing = Country(proposer);
    var other = Country(proposer == war.Aggressor ? war.Defender : war.Aggressor);

    if (peace.Terms is not PeaceTerms.White && war.Prevailing != proposer)
    {
      throw new DomainException($"{proposing.Name} is not prevailing, and only the side that is can dictate terms.");
    }

    if (peace.Terms is PeaceTerms.Cession)
    {
      RejectUnfitCession(other, peace);
    }

    war.ConcludeWith(peace);

    // From here on nothing may refuse: the treaty is signed.
    switch (peace.Terms)
    {
      case PeaceTerms.Cession:
        var ceded = peace.Ceded.Select(other.Cede).ToList();

        foreach (var province in ceded)
        {
          proposing.Annex(province);
        }

        _pending.Add(new PeaceWasMade(proposing.Name, other.Name, Ceding(ceded, proposing)));
        break;
      case PeaceTerms.Annexation:
        Swallow(proposing, other);
        break;
      default:
        _pending.Add(new PeaceWasMade(proposing.Name, other.Name, "nothing changes hands"));
        break;
    }
  }

  private static void RejectUnfitCession(Country loser, Peace peace)
  {
    if (peace.Ceded.Any(province => !loser.Governs(province)))
    {
      throw new DomainException($"{loser.Name} does not govern every province that peace would cede.");
    }

    if (peace.Ceded.Count >= loser.Provinces.Count)
    {
      throw new DomainException(
        $"A cession cannot take the last of {loser.Name}'s provinces; only an annexation swallows a state whole.");
    }
  }

  private static string Ceding(List<Province> ceded, Country winner)
  {
    var names = ceded.Select(province => province.Name).ToList();
    var listed = names.Count == 1 ? names[0] : $"{string.Join(", ", names[..^1])} and {names[^1]}";

    return $"{listed} {(names.Count == 1 ? "is" : "are")} ceded to {winner.Name}";
  }

  /// <summary>
  /// The loser ceases to exist. Its territory and its stores pass to the
  /// victor, and every other war it was fighting ends with it, since there is
  /// no one left to fight it.
  /// </summary>
  private void Swallow(Country winner, Country loser)
  {
    foreach (var province in loser.Surrender())
    {
      winner.Annex(province);
    }

    foreach (var stock in loser.Store.Stocks)
    {
      winner.LayUp(stock);
    }

    _countries.Remove(loser);
    _pending.Add(new PeaceWasMade(winner.Name, loser.Name, $"{loser.Name} is annexed by {winner.Name}"));

    foreach (var war in WarsOf(loser.Id))
    {
      var survivor = Country(war.Aggressor == loser.Id ? war.Defender : war.Aggressor);

      war.ConcludeWith(Peace.White());
      _pending.Add(new PeaceWasMade(survivor.Name, loser.Name, $"{loser.Name} is no more"));
    }
  }

  /// <summary>
  /// Lets one day pass for every country in the world, and tells what came of
  /// it. Every country works first; only then is what the day's production
  /// cost settled into the value of each commodity, across all of them at once.
  /// The new values set what keeping a hand costs. Then everyone takes their
  /// basket out of the store, and those who could not die of it. On the first
  /// of the month a people that went short of nothing that day multiplies, and
  /// every open war is fought. What was declared or signed since the last day
  /// is told first.
  /// </summary>
  public DayReport Elapse(GameDate today)
  {
    List<Occurrence> occurrences = [.. _pending];

    _pending.Clear();
    List<Yield> yields = [];

    foreach (var country in _countries)
    {
      var day = country.WorkOneDay(Register);

      yields.AddRange(day.Yields);
      occurrences.AddRange(day.Idle.Select(idle => StoodIdle(country, idle)));
    }

    occurrences.AddRange(
      Register.Revalue(yields)
        .Where(revaluation => Math.Abs(revaluation.RelativeChange) >= ReportedMovement)
        .Select(Revalued));

    SettleNecessaryLabor();

    List<Country> provided = [];

    foreach (var country in _countries)
    {
      var share = country.Provision(Subsistence);

      if (share < 1m)
      {
        occurrences.Add(new PeopleWentWithout(country.Name, share, country.LoseTheUnprovided(share)));
      }
      else
      {
        provided.Add(country);
      }
    }

    if (today.Date.Day == 1)
    {
      foreach (var country in provided)
      {
        var born = country.Multiply();

        if (born > 0)
        {
          occurrences.Add(new PopulationGrew(country.Name, born));
        }
      }

      occurrences.AddRange(Fight());
    }

    return new DayReport(today, occurrences);
  }

  /// <summary>
  /// A month of fighting in every open war, weighed by what each society can
  /// put under arms.
  /// </summary>
  private List<TheFrontMoved> Fight()
  {
    List<TheFrontMoved> fronts = [];

    foreach (var war in _wars.Where(war => !war.IsOver))
    {
      var aggressor = Country(war.Aggressor);
      var defender = Country(war.Defender);

      war.Fight(aggressor.Armable, defender.Armable);
      fronts.Add(new TheFrontMoved(aggressor.Name, defender.Name, war.Momentum));
    }

    return fronts;
  }

  /// <summary>
  /// Resets what keeping a hand costs on every branch in the world. The cap is
  /// the working day: a branch rightly refuses a day too short to reproduce the
  /// labourer, and a famine in value terms must not bring the day to a halt.
  /// </summary>
  private void SettleNecessaryLabor()
  {
    var cost = CostOfSubsistence();

    foreach (var branch in _countries.SelectMany(country => country.Provinces).SelectMany(province => province.Branches))
    {
      branch.CostsToKeepAHand(Lesser(cost, branch.WorkingDay));
    }
  }

  private static LaborTime Lesser(LaborTime one, LaborTime other)
  {
    return one.CompareTo(other) <= 0 ? one : other;
  }

  private CommodityWasRevalued Revalued(Revaluation revaluation)
  {
    var commodity = Register.Get(revaluation.Commodity);

    return new CommodityWasRevalued(
      commodity.Name, commodity.UseValue.Unit.Name, revaluation.Before, revaluation.After);
  }

  private BranchStoodIdle StoodIdle(Country country, IdleBranch idle)
  {
    var province = country.Provinces.Single(province => province.Id == idle.Province);

    return new BranchStoodIdle(country.Name, province.Name, Register.Get(idle.Commodity).Name);
  }

  #endregion
}
