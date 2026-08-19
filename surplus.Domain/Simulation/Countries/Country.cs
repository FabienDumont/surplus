using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Countries;

/// <summary>
/// Aggregate root: a polity, played either by the human or by the machine, and
/// the territory it governs.
/// A country is its territory — it is founded on at least one province and
/// cannot be stripped of its last one, since a country without territory has
/// ceased to be a country rather than become an empty one.
/// Who controls it is a property of the country itself, not of the player: the
/// same world can be replayed from any seat, and every country the human is not
/// sitting in is run by the AI.
/// </summary>
public sealed class Country
{
  #region Fields

  private readonly List<Province> _provinces;

  #endregion

  #region Properties

  public CountryId Id { get; }
  public string Name { get; }
  public CountryControl Control { get; private set; }

  /// <summary>How the state is constituted, what it avows, and what it has put on the books.</summary>
  public Government Government { get; private set; }

  /// <summary>
  /// The mode most of this country's people produce under. A state can rest on
  /// two at once — in 1836 the United States is a capitalist North and a slave
  /// South — and a civil war is what settles which one the state will serve.
  /// </summary>
  public ModeOfProduction DominantMode =>
    _provinces
      .GroupBy(province => province.Mode)
      .OrderByDescending(group => group.Sum(province => province.Population))
      .Select(group => group.Key)
      .First();

  public int Population => _provinces.Sum(province => province.Population);

  /// <summary>The provinces this country governs, never empty.</summary>
  public IReadOnlyList<Province> Provinces => _provinces;

  public bool IsPlayed => Control is CountryControl.Player;

  #endregion

  #region Ctors

  private Country(
    CountryId id, string name, CountryControl control, Government government, List<Province> provinces)
  {
    Id = id;
    Name = name;
    Control = control;
    Government = government;
    _provinces = provinces;
  }

  #endregion

  #region Methods

  /// <summary>A country comes into being with the territory it holds from the start.</summary>
  public static Country Found(
    string name, CountryControl control, Government government, Province firstProvince)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new DomainException("A country must have a name.");
    }

    return new Country(CountryId.New(), name.Trim(), control, government, [firstProvince]);
  }

  /// <summary>
  /// Reconstitutes a country from a stored snapshot. Unlike <see cref="Found" />
  /// this asserts no invariant: the state it receives was already valid when it
  /// was saved.
  /// </summary>
  public static Country Load(
    CountryId id, string name, CountryControl control, Government government, IEnumerable<Province> provinces)
  {
    return new Country(id, name, control, government, [.. provinces]);
  }

  public bool Governs(ProvinceId provinceId)
  {
    return _provinces.Any(province => province.Id == provinceId);
  }

  /// <summary>Brings a province under this country's rule.</summary>
  public void Annex(Province province)
  {
    if (Governs(province.Id))
    {
      throw new DomainException($"{Name} already governs {province.Name}.");
    }

    _provinces.Add(province);
  }

  /// <summary>Gives a province up, so long as it is not the country's last.</summary>
  public Province Cede(ProvinceId provinceId)
  {
    var province = _provinces.SingleOrDefault(candidate => candidate.Id == provinceId) ??
                   throw new DomainException($"{Name} does not govern that province.");

    if (_provinces.Count == 1)
    {
      throw new DomainException($"A country without territory is no country: {Name} cannot cede {province.Name}.");
    }

    _provinces.Remove(province);

    return province;
  }

  /// <summary>
  /// Puts a statute on the books and brings it to bear on every province. The
  /// law is the same everywhere; its results are not, because the class
  /// structure it lands in differs from province to province.
  /// </summary>
  public void Enact(Law law)
  {
    Government = Government.Enacting(law);

    foreach (var province in _provinces)
    {
      province.Enforce(law);
    }
  }

  /// <summary>
  /// Strikes a statute off. Repeal does not restore the class it dissolved:
  /// the freed are not led back into the relation the law took them out of.
  /// </summary>
  public void Repeal(Law law)
  {
    Government = Government.Repealing(law);
  }

  /// <summary>Reconstitutes the state in another form, the base untouched.</summary>
  public void TakeForm(GovernmentForm form)
  {
    Government = Government.TakingForm(form);
  }

  /// <summary>
  /// Changes what the state avows. A turn to fascism is exactly this and no
  /// more: the mode of production is not disturbed, only the manner of ruling
  /// under it.
  /// </summary>
  public void Adopt(Ideology ideology)
  {
    Government = Government.Adopting(ideology);
  }

  public void GiveToPlayer()
  {
    if (IsPlayed)
    {
      throw new DomainException($"{Name} is already played by the player.");
    }

    Control = CountryControl.Player;
  }

  public void GiveToAi()
  {
    if (!IsPlayed)
    {
      throw new DomainException($"{Name} is already run by the AI.");
    }

    Control = CountryControl.Ai;
  }

  public override string ToString()
  {
    return $"{Name} — {Government.Ideology} {Government.Form}, {Control} " +
           $"({_provinces.Count} {(_provinces.Count == 1 ? "province" : "provinces")})";
  }

  #endregion
}
