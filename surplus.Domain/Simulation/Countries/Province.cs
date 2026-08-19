using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Countries;

/// <summary>
/// A subdivision of a country, and the unit the simulation works on: a mode of
/// production and the class structure standing on it.
/// The resolution stops here deliberately. Value is determined by socially
/// necessary labour time — a social average, not the time any one workshop
/// happens to spend — so modelling individual mills and mines and summing them
/// would imply value is made locally, which is precisely what Marx denies.
/// A province is an entity, not an aggregate root: it has an identity of its
/// own but no life of its own, existing only as part of the
/// <see cref="Country" /> that governs it.
/// The mode lives here rather than on the country because one state can rest on
/// two of them at once — in 1836 the United States is a capitalist North and a
/// slave South, and a civil war is what settles which mode the state serves.
/// </summary>
public sealed class Province
{
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

  #endregion

  #region Ctors

  private Province(ProvinceId id, string name, ClassComposition composition)
  {
    Id = id;
    Name = name;
    Composition = composition;
  }

  #endregion

  #region Methods

  public static Province Establish(string name, ClassComposition composition)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new DomainException("A province must have a name.");
    }

    return new Province(ProvinceId.New(), name.Trim(), composition);
  }

  /// <summary>
  /// Reconstitutes a province from a stored snapshot. Unlike
  /// <see cref="Establish" /> this asserts no invariant: the state it receives
  /// was already valid when it was saved.
  /// </summary>
  public static Province Load(ProvinceId id, string name, ClassComposition composition)
  {
    return new Province(id, name, composition);
  }

  public int HeadsOf(SocialClass socialClass)
  {
    return Composition.HeadsOf(socialClass);
  }

  public void Grow(SocialClass socialClass, int heads)
  {
    Composition = Composition.Grown(socialClass, heads);
  }

  public void Decline(SocialClass socialClass, int heads)
  {
    Composition = Composition.Declined(socialClass, heads);
  }

  /// <summary>
  /// Drives people out of one class and into another. Enclosure, expropriation,
  /// emancipation and proletarianisation are all this operation.
  /// </summary>
  public void Transform(SocialClass from, SocialClass into, int heads)
  {
    Composition = Composition.Transformed(from, into, heads);
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
  }

  public override string ToString()
  {
    return $"{Name} — {Mode} ({Population} souls)";
  }

  #endregion
}
