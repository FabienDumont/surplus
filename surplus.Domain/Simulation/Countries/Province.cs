using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Countries;

/// <summary>
/// A subdivision of a country, and the unit the simulation works on: a mode of
/// production, the class structure standing on it, and the branches of
/// production those classes are set to work in.
/// The resolution stops here deliberately. Value is determined by socially
/// necessary labour time — a social average, not the time any one workshop
/// happens to spend — so modelling individual mills and mines and summing them
/// would imply value is made locally, which is precisely what Marx denies. What
/// a province holds is therefore a <see cref="Branch" /> per commodity: all the
/// mills of one kind at once, which have a productivity between them without any
/// of them having a value of its own.
/// A province is an entity, not an aggregate root: it has an identity of its
/// own but no life of its own, existing only as part of the
/// <see cref="Country" /> that governs it.
/// The mode lives here rather than on the country because one state can rest on
/// two of them at once — in 1836 the United States is a capitalist North and a
/// slave South, and a civil war is what settles which mode the state serves.
/// </summary>
public sealed class Province
{
  #region Fields

  private readonly List<Branch> _branches;

  #endregion

  #region Properties

  public ProvinceId Id { get; }
  public string Name { get; }

  /// <summary>
  /// How surplus-labour is pumped out of the direct producers here — read off
  /// the class structure, never declared over it. A mode of production is not
  /// proclaimed into being: it changes when enough people have changed the
  /// relation they stand in, which is why there is no method here to set it.
  /// </summary>
  public ModeOfProduction Mode => Composition.PrevailingMode;

  public ClassComposition Composition { get; private set; }

  public int Population => Composition.Population;

  public bool IsClassless => Composition.IsClassless;

  /// <summary>Classes here that the prevailing mode does not bear of its own accord.</summary>
  public IReadOnlyList<SocialClass> Survivals => Composition.SurvivalsUnder(Mode);

  /// <summary>What is produced here, one branch to a commodity.</summary>
  public IReadOnlyList<Branch> Branches => _branches;

  #endregion

  #region Ctors

  private Province(ProvinceId id, string name, ClassComposition composition, List<Branch> branches)
  {
    Id = id;
    Name = name;
    Composition = composition;
    _branches = branches;
  }

  #endregion

  #region Methods

  public static Province Establish(string name, ClassComposition composition)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new DomainException("A province must have a name.");
    }

    return new Province(ProvinceId.New(), name.Trim(), composition, []);
  }

  /// <summary>
  /// Reconstitutes a province from a stored snapshot. Unlike
  /// <see cref="Establish" /> this asserts no invariant: the state it receives
  /// was already valid when it was saved.
  /// </summary>
  public static Province Load(
    ProvinceId id, string name, ClassComposition composition, IEnumerable<Branch> branches)
  {
    return new Province(id, name, composition, [.. branches]);
  }

  public int HeadsOf(SocialClass socialClass)
  {
    return Composition.HeadsOf(socialClass);
  }

  /// <summary>How many of a class are at work in the branches here.</summary>
  public int Employed(SocialClass socialClass)
  {
    return _branches.Where(branch => branch.Workforce == socialClass).Sum(branch => branch.Hands);
  }

  /// <summary>
  /// Those of a class the branches have no work for. Under capital this is no
  /// accident but a condition: an industrial reserve army, kept in being by the
  /// same accumulation that throws people out of work, and pressing on the wages
  /// of everyone still in it.
  /// </summary>
  public int Idle(SocialClass socialClass)
  {
    return HeadsOf(socialClass) - Employed(socialClass);
  }

  public bool Produces(CommodityId commodity)
  {
    return _branches.Any(branch => branch.Produces == commodity);
  }

  /// <summary>
  /// Sets a branch going here. Nobody can be put to work who is not here: the
  /// hands a branch employs must be standing in the class it employs them from.
  /// </summary>
  public void Open(Branch branch)
  {
    if (Produces(branch.Produces))
    {
      throw new DomainException($"{Name} already works that commodity, and works all of it in one branch.");
    }

    RejectHandsThatAreNotHere(branch.Workforce, branch.Hands);

    _branches.Add(branch);
  }

  /// <summary>Shuts a branch down. The hands are not destroyed with it, only turned out.</summary>
  public void Close(CommodityId commodity)
  {
    _branches.Remove(BranchWorking(commodity));
  }

  /// <summary>Takes on hands, so far as there are any here to take on.</summary>
  public void Employ(CommodityId commodity, int hands)
  {
    var branch = BranchWorking(commodity);

    RejectHandsThatAreNotHere(branch.Workforce, hands);

    branch.Employ(hands);
  }

  /// <summary>
  /// Turns hands out. A branch left without any is not a branch of production
  /// but a building, and is closed.
  /// </summary>
  public void LayOff(CommodityId commodity, int hands)
  {
    var branch = BranchWorking(commodity);

    branch.LayOff(hands);

    if (branch.Hands == 0)
    {
      _branches.Remove(branch);
    }
  }

  /// <summary>
  /// Works every branch for one period. The yields are individual: what each
  /// branch's own labour cost it. What any of it is worth is settled elsewhere,
  /// across all the provinces supplying the same market.
  /// </summary>
  public IReadOnlyList<Yield> Work(CommodityRegister register)
  {
    return [.. _branches.Select(branch => branch.Work(register))];
  }

  public void Grow(SocialClass socialClass, int heads)
  {
    Composition = Composition.Grown(socialClass, heads);
  }

  public void Decline(SocialClass socialClass, int heads)
  {
    Composition = Composition.Declined(socialClass, heads);

    DismissThoseWhoAreNoLongerHere();
  }

  /// <summary>
  /// Drives people out of one class and into another. Enclosure, expropriation,
  /// emancipation and proletarianisation are all this operation.
  /// </summary>
  public void Transform(SocialClass from, SocialClass into, int heads)
  {
    Composition = Composition.Transformed(from, into, heads);

    DismissThoseWhoAreNoLongerHere();
  }

  /// <summary>
  /// Brings a statute to bear on this province. Everyone standing in a relation
  /// the law forbids must become something else — and what they become is
  /// settled not by the statute but by whether those who lived on their labour
  /// still hold the conditions of production when it lands.
  /// </summary>
  public void Enforce(Law law)
  {
    foreach (var relation in law.Forbids())
    {
      Emancipate(relation);
    }
  }

  private void Emancipate(ProductionRelation relation)
  {
    var outlawed = Composition.Presences.Where(presence => presence.Profile.Relation == relation).ToList();

    if (outlawed.Count == 0)
    {
      return;
    }

    // Those who appropriate by some other title are untouched by this statute,
    // and it is their survival that decides what the freed become.
    var appropriatorsSurvive = Composition.Presences.Any(
      presence => presence.Profile.AppropriatesSurplus && presence.Profile.Relation != relation);

    foreach (var presence in outlawed)
    {
      var successor = Laws.Frees(relation, appropriatorsSurvive);

      Composition = presence.Class == successor
        ? Composition
        : Composition.Transformed(presence.Class, successor, presence.Heads);
    }

    DismissThoseWhoAreNoLongerHere();
  }

  /// <summary>
  /// Takes out of the branches the hands that are no longer standing in the
  /// class they were employed from. Death, emigration and expropriation do not
  /// ask a mill whether it can spare them. Which mill loses them first is not
  /// something this model claims to know, so they are taken in the order the
  /// branches were opened.
  /// </summary>
  private void DismissThoseWhoAreNoLongerHere()
  {
    foreach (var socialClass in _branches.Select(branch => branch.Workforce).Distinct().ToList())
    {
      var gone = Employed(socialClass) - HeadsOf(socialClass);

      foreach (var branch in _branches.Where(branch => branch.Workforce == socialClass).ToList())
      {
        if (gone <= 0)
        {
          break;
        }

        var turnedOut = Math.Min(gone, branch.Hands);

        branch.LayOff(turnedOut);
        gone -= turnedOut;

        if (branch.Hands == 0)
        {
          _branches.Remove(branch);
        }
      }
    }
  }

  private Branch BranchWorking(CommodityId commodity)
  {
    return _branches.SingleOrDefault(branch => branch.Produces == commodity) ??
           throw new DomainException($"Nothing of that kind is produced in {Name}.");
  }

  private void RejectHandsThatAreNotHere(SocialClass workforce, int hands)
  {
    if (hands > Idle(workforce))
    {
      throw new DomainException(
        $"There are not {hands} {workforce} idle in {Name} to set to work."
      );
    }
  }

  public override string ToString()
  {
    return $"{Name} — {Mode} ({Population} souls)";
  }

  #endregion
}
