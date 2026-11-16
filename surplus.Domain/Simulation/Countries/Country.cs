using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Production;
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

  /// <summary>
  /// The share of those who went wholly without that die of it in one day. Want
  /// kills slowly and unevenly, by weakening before it starves; a percent a day
  /// is a tuned figure, not a demography.
  /// </summary>
  private static readonly decimal DeathsOfWant = 0.01m;

  /// <summary>
  /// How much the fed part of a class grows in a month. A tenth of a percent is
  /// a tuned figure; it is the same for every class because every class takes
  /// the same basket.
  /// </summary>
  private static readonly decimal MonthlyGrowth = 0.001m;

  /// <summary>
  /// The share of a basket the state gives those with no title, when it gives
  /// relief at all: less than the worst wage, by the principle of less
  /// eligibility.
  /// </summary>
  private static readonly decimal Relief = 0.5m;

  /// <summary>The share of the unemployed who sink into pauperism in a month. A tuned figure.</summary>
  private static readonly decimal Pauperism = 0.01m;

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

  /// <summary>
  /// Those this country could put under arms, read off its class structure
  /// province by province. A slave society counts more heads than it can arm.
  /// </summary>
  public int Armable => _provinces.Sum(province => province.Composition.Armable);

  /// <summary>The provinces this country governs, never empty.</summary>
  public IReadOnlyList<Province> Provinces => _provinces;

  public bool IsPlayed => Control is CountryControl.Player;

  /// <summary>
  /// Everything the country has laid up. It is national and not provincial:
  /// every province produces into it and draws on it, as though there were no
  /// distance inside a border, and nothing crosses a border either way.
  /// </summary>
  public Store Store { get; private set; }

  #endregion

  #region Ctors

  private Country(
    CountryId id, string name, CountryControl control, Government government, List<Province> provinces, Store store)
  {
    Id = id;
    Name = name;
    Control = control;
    Government = government;
    _provinces = provinces;
    Store = store;
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

    return new Country(CountryId.New(), name.Trim(), control, government, [firstProvince], Store.Empty);
  }

  /// <summary>
  /// Reconstitutes a country from a stored snapshot. Unlike <see cref="Found" />
  /// this asserts no invariant: the state it receives was already valid when it
  /// was saved.
  /// </summary>
  public static Country Load(
    CountryId id, string name, CountryControl control, Government government, IEnumerable<Province> provinces,
    Store store)
  {
    return new Country(id, name, control, government, [.. provinces], store);
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
  /// <summary>
  /// Gives up every province at once, the last included. Only an annexation
  /// does this, and the country does not survive it: it is taken out of the
  /// world in the same act.
  /// </summary>
  internal IReadOnlyList<Province> Surrender()
  {
    List<Province> surrendered = [.. _provinces];

    _provinces.Clear();

    return surrendered;
  }

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

  /// <summary>Starts keeping those with no wage on half a basket.</summary>
  public void RelieveThePoor()
  {
    Government = Government.Relieving();
  }

  /// <summary>Leaves those with no wage to themselves.</summary>
  public void WithdrawRelief()
  {
    Government = Government.WithdrawingRelief();
  }

  /// <summary>
  /// Puts something in the store that no day of the simulation produced: the
  /// granaries and warehouses a scenario opens on, the fruit of labour done
  /// before the game began.
  /// </summary>
  public void LayUp(Stock stock)
  {
    Store = Store.LaidUp(stock);
  }

  /// <summary>
  /// Works every branch of every province for one day, out of the store and
  /// into it. A branch works only if the store can replace in kind everything
  /// it uses up, its whole material and the worn share of its instruments.
  /// Otherwise it stands idle and takes nothing. What is made today is laid up
  /// only once every branch has had its turn: a product has to be finished
  /// before it can enter another process as material.
  /// </summary>
  public DayOfProduction WorkOneDay(CommodityRegister register)
  {
    List<Yield> yields = [];
    List<IdleBranch> idle = [];

    foreach (var province in _provinces)
    {
      foreach (var branch in province.Branches)
      {
        var consumed = branch.Means.ConsumedInOnePeriod();

        if (!Store.Holds(consumed))
        {
          idle.Add(new IdleBranch(province.Id, branch.Produces));

          continue;
        }

        var yield = branch.Work(register);

        Store = Store.Drawn(consumed);
        yields.Add(yield);
      }
    }

    Store = Store.LaidUp(yields.Select(yield => yield.Product));

    return new DayOfProduction(yields, idle);
  }

  /// <summary>
  /// Gives every class what its revenue gives it a title to, so far as the
  /// store can. The appropriators are served first; every other claim is then
  /// met in the same proportion. Each basket is taken whole or in proportion,
  /// never good by good: a people short of cloth does not make it up in corn.
  /// What is left in the store after this is the surplus product.
  /// </summary>
  public Rations Provision(Subsistence subsistence)
  {
    var relieved = Government.RelievesThePoor;
    var claims = _provinces
      .SelectMany(province => province.Composition.Presences, (province, presence) => Claim(province, presence, relieved))
      .ToList();

    var firstShare = Serve(subsistence, claims.Where(claim => claim.First).Sum(claim => claim.Claimed));
    var storeShare = Serve(subsistence, claims.Where(claim => !claim.First).Sum(claim => claim.Claimed));

    return new Rations(
      [
        .. claims.Select(claim => new Ration(
          claim.Province, claim.Class, claim.Heads, claim.Workless, claim.Claimed,
          claim.Claimed * (claim.First ? firstShare : storeShare)))
      ],
      storeShare);
  }

  /// <summary>What one class in one province has a title to, in baskets.</summary>
  private static (ProvinceId Province, SocialClass Class, int Heads, int Workless, decimal Claimed, bool First) Claim(
    Province province, ClassPresence presence, bool relieved)
  {
    var heads = presence.Heads;
    var access = presence.Profile.Income.Title();

    switch (access)
    {
      case Access.Employed:
        var employed = Math.Min(province.Employed(presence.Class), heads);
        var unemployed = heads - employed;

        return (province.Id, presence.Class, heads, unemployed, employed + (relieved ? unemployed * Relief : 0m), false);
      case Access.None:
        return (province.Id, presence.Class, heads, heads, relieved ? heads * Relief : 0m, false);
      default:
        return (province.Id, presence.Class, heads, 0, heads, access is Access.First);
    }
  }

  /// <summary>
  /// Draws so many baskets from the store, or the largest share of them it can
  /// give, and returns that share.
  /// </summary>
  private decimal Serve(Subsistence subsistence, decimal baskets)
  {
    var needed = subsistence.PerHead
      .Select(stock => Stock.Of(stock.Commodity, stock.Quantity * baskets))
      .Where(stock => !stock.IsExhausted)
      .ToList();
    var share = needed
      .Select(stock => Math.Min(1m, Store.AmountOf(stock.Commodity) / stock.Quantity.Amount))
      .DefaultIfEmpty(1m)
      .Min();

    // The share is a quotient, and the smallest good must never be asked for a
    // hair more than the store holds because of how the division rounded.
    List<Stock> taken =
    [
      .. needed.Select(stock => Stock.Of(
        stock.Commodity,
        Quantity.Of(Math.Min(stock.Quantity.Amount * share, Store.AmountOf(stock.Commodity)), stock.Quantity.Unit)))
    ];

    Store = Store.Drawn(taken);

    return share;
  }

  /// <summary>
  /// Those who went without die of it, a few each day: those whose title the
  /// store could not honour, and those who had none. The appropriators are left
  /// out; they do not starve first, and the model says so by not starving them
  /// at all. The dead fall on those without work before anyone is struck off a
  /// branch, because a branch loses hands only when its class has fewer heads
  /// than it employs.
  /// </summary>
  public Losses LoseTheUnprovided(Rations rations)
  {
    var ofShortage = 0;
    var ofWorklessness = 0;

    foreach (var ration in rations.Each.Where(ration => !ration.Class.Profile().AppropriatesSurplus))
    {
      var shortage = (int)Math.Floor((ration.Claimed - ration.Provided) * DeathsOfWant);
      var worklessness = (int)Math.Floor((ration.Heads - ration.Claimed) * DeathsOfWant);

      if (shortage + worklessness > 0)
      {
        Governed(ration.Province).Decline(ration.Class, shortage + worklessness);
        ofShortage += shortage;
        ofWorklessness += worklessness;
      }
    }

    return new Losses(ofShortage, ofWorklessness);
  }

  /// <summary>A people multiplies by those who were fed, class by class. Returns by how many.</summary>
  public int Multiply(Rations rations)
  {
    var born = 0;

    foreach (var ration in rations.Each)
    {
      var growth = (int)Math.Floor(ration.Provided * MonthlyGrowth);

      if (growth > 0)
      {
        Governed(ration.Province).Grow(ration.Class, growth);
        born += growth;
      }
    }

    return born;
  }

  /// <summary>
  /// Those out of work long enough fall out of the working class altogether.
  /// Marx calls pauperism "the hospital of the active labour-army and the dead
  /// weight of the industrial reserve army" (Capital I, ch. 25 §4): the same
  /// accumulation that throws workers out of work sinks a share of them below
  /// it. They become lumpenproletarians, whom no branch will take on again.
  /// Returns how many sank.
  /// </summary>
  public int Pauperise()
  {
    var sunk = 0;

    foreach (var province in _provinces)
    {
      foreach (var presence in province.Composition.Presences
                 .Where(presence => presence.Profile.Income.Title() is Access.Employed).ToList())
      {
        var sinking = (int)Math.Floor(province.Idle(presence.Class) * Pauperism);

        if (sinking > 0)
        {
          province.Transform(presence.Class, SocialClass.Lumpenproletariat, sinking);
          sunk += sinking;
        }
      }
    }

    return sunk;
  }

  /// <summary>
  /// Sets a branch going by a known technique. Its instruments have to exist
  /// before it can: they are taken from the store, where only past labour in
  /// Department I can have put them. That is accumulation in its plainest
  /// form, yesterday's surplus product become today's means of production.
  /// </summary>
  public void OpenBranch(
    ProvinceId province, Technique technique, SocialClass workforce, int hands, LaborTime workingDay,
    LaborTime necessaryLabor)
  {
    var where = Governed(province);
    var means = technique.EquipFor(hands);
    var branch = Branch.Open(
      technique.Produces, means, workforce, hands, workingDay, necessaryLabor, technique.OutputPerHour);

    RequireInStore(means.Instruments, hands);

    where.Open(branch);
    Store = Store.Drawn(means.Instruments);
  }

  /// <summary>Shuts a branch down. Its instruments are scrapped where they stand; nothing returns to the store.</summary>
  public void CloseBranch(ProvinceId province, CommodityId commodity)
  {
    Governed(province).Close(commodity);
  }

  /// <summary>Takes on hands, and takes out of the store the instruments to set them at.</summary>
  public void Employ(ProvinceId province, CommodityId commodity, int hands)
  {
    var where = Governed(province);
    var branch = BranchWorking(where, commodity);
    var instruments = branch.Means.Scaled((decimal)Math.Max(hands, 0) / branch.Hands).Instruments;

    RequireInStore(instruments, hands);

    where.Employ(commodity, hands);
    Store = Store.Drawn(instruments);
  }

  /// <summary>Turns hands out. The instruments they worked at are scrapped, not returned to the store.</summary>
  public void LayOff(ProvinceId province, CommodityId commodity, int hands)
  {
    Governed(province).LayOff(commodity, hands);
  }

  /// <summary>
  /// Puts a branch to work by another technique. The new instruments are
  /// installed whole, out of the store, and the old ones are scrapped: a
  /// hand-loom is not traded in against a power-loom, it is simply made
  /// worthless by it.
  /// </summary>
  public void Reequip(ProvinceId province, CommodityId commodity, Technique technique)
  {
    if (technique.Produces != commodity)
    {
      throw new DomainException($"{technique.Name} does not produce what that branch produces.");
    }

    var where = Governed(province);
    var branch = BranchWorking(where, commodity);
    var means = technique.EquipFor(branch.Hands);

    RequireInStore(means.Instruments, branch.Hands);

    where.Reequip(commodity, means, technique.OutputPerHour);
    Store = Store.Drawn(means.Instruments);
  }

  /// <summary>Fixes how long a day the hands in one branch work.</summary>
  public void FixTheWorkingDay(ProvinceId province, CommodityId commodity, LaborTime workingDay)
  {
    Governed(province).FixTheWorkingDay(commodity, workingDay);
  }

  private Province Governed(ProvinceId province)
  {
    return _provinces.SingleOrDefault(candidate => candidate.Id == province) ??
           throw new DomainException($"{Name} does not govern that province.");
  }

  private static Branch BranchWorking(Province province, CommodityId commodity)
  {
    return province.Branches.SingleOrDefault(branch => branch.Produces == commodity) ??
           throw new DomainException($"Nothing of that kind is produced in {province.Name}.");
  }

  /// <summary>
  /// Refuses a change the store cannot furnish the instruments for, naming what
  /// is lacking in its own unit. The country keeps no register of names, and a
  /// shortfall of so many machines is what the player needs to know anyway.
  /// </summary>
  private void RequireInStore(IReadOnlyList<Stock> instruments, int hands)
  {
    if (Store.Holds(instruments))
    {
      return;
    }

    var lacking = instruments
      .GroupBy(stock => stock.Commodity)
      .Select(group => (Unit: group.First().Quantity.Unit, Needed: group.Sum(stock => stock.Quantity.Amount),
                        Held: Store.AmountOf(group.Key)))
      .Where(need => need.Needed > need.Held)
      .Select(need => Quantity.Of(need.Needed - need.Held, need.Unit).ToString());

    throw new DomainException(
      $"There is not enough in store to equip {hands} {(hands == 1 ? "hand" : "hands")}: " +
      $"{string.Join(", ", lacking)} lacking.");
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
