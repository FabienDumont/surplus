using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Society;

/// <summary>
/// How a population divides by its relation to the conditions of production.
/// Immutable: a society does not edit its class structure in place, it is
/// transformed into another one — which is why every operation here returns a
/// new composition rather than mutating this one.
/// </summary>
public sealed record ClassComposition
{
  #region Fields

  public static readonly ClassComposition Empty = new([]);

  private readonly List<ClassPresence> _presences;

  #endregion

  #region Properties

  public IReadOnlyList<ClassPresence> Presences => _presences;

  public int Population => _presences.Sum(presence => presence.Heads);

  /// <summary>Those whose labour the whole structure rests on.</summary>
  public int DirectProducers =>
    _presences.Where(presence => presence.Profile.IsDirectProducer).Sum(presence => presence.Heads);

  /// <summary>Those living on labour they did not perform.</summary>
  public int Appropriators =>
    _presences.Where(presence => presence.Profile.AppropriatesSurplus).Sum(presence => presence.Heads);

  /// <summary>
  /// The mode of production this class structure amounts to. It is read off the
  /// classes rather than declared over them: a society is not feudal because
  /// someone says so, but because most of its people stand in the relations
  /// feudalism puts them in.
  /// Weight is divided across the modes a class belongs to, so classes at home
  /// in several epochs — merchants, usurers, the peasantry — count as the weak
  /// evidence they are. Ties fall to the earlier mode: a structure that does
  /// not prove it has moved on has not moved on.
  /// </summary>
  public ModeOfProduction PrevailingMode =>
    _presences
      .SelectMany(
        presence => presence.Profile.NativeModes.Select(
          mode => (Mode: mode, Weight: (decimal)presence.Heads / presence.Profile.NativeModes.Count)))
      .GroupBy(weighted => weighted.Mode)
      .OrderByDescending(group => group.Sum(weighted => weighted.Weight))
      .ThenBy(group => group.Key)
      .Select(group => group.Key)
      .DefaultIfEmpty(ModeOfProduction.PrimitiveCommunal)
      .First();

  /// <summary>Whether any class here lives on the surplus-labour of another.</summary>
  public bool IsAntagonistic => _presences.Any(presence => presence.Profile.AppropriatesSurplus);

  /// <summary>
  /// The communist condition, and the only honest way to score it: not a
  /// threshold reached but an antagonism that no longer exists.
  /// </summary>
  public bool IsClassless => !IsAntagonistic;

  #endregion

  #region Ctors

  private ClassComposition(List<ClassPresence> presences)
  {
    _presences = presences;
  }

  #endregion

  #region Methods

  public static ClassComposition Of(params ClassPresence[] presences)
  {
    var duplicate = presences
      .GroupBy(presence => presence.Class)
      .FirstOrDefault(group => group.Count() > 1);

    if (duplicate is not null)
    {
      throw new DomainException($"{duplicate.Key} is counted twice in the same composition.");
    }

    return new ClassComposition([..presences]);
  }

  public int HeadsOf(SocialClass socialClass)
  {
    return _presences.SingleOrDefault(presence => presence.Class == socialClass)?.Heads ?? 0;
  }

  public bool Holds(SocialClass socialClass)
  {
    return HeadsOf(socialClass) > 0;
  }

  /// <summary>
  /// The classes present that the given mode does not bear of its own accord —
  /// the dead generations still weighing on the living. A Junker in a capitalist
  /// province, a serf where serfdom was abolished on paper.
  /// </summary>
  public IReadOnlyList<SocialClass> SurvivalsUnder(ModeOfProduction mode)
  {
    return
    [
      .._presences.Where(presence => !presence.Profile.IsNativeTo(mode)).Select(presence => presence.Class)
    ];
  }

  public ClassComposition Grown(SocialClass socialClass, int heads)
  {
    if (heads <= 0)
    {
      throw new DomainException("A class grows by someone, or not at all.");
    }

    return Rebuilt(socialClass, HeadsOf(socialClass) + heads);
  }

  public ClassComposition Declined(SocialClass socialClass, int heads)
  {
    if (heads <= 0)
    {
      throw new DomainException("A class declines by someone, or not at all.");
    }

    var standing = HeadsOf(socialClass);

    if (heads > standing)
    {
      throw new DomainException($"There are not {heads} {socialClass} here to lose.");
    }

    return Rebuilt(socialClass, standing - heads);
  }

  /// <summary>
  /// Moves people from one class into another — the only way a class structure
  /// ever really changes. Peasants expropriated into proletarians is the whole
  /// of primitive accumulation written as one call.
  /// </summary>
  public ClassComposition Transformed(SocialClass from, SocialClass into, int heads)
  {
    if (from == into)
    {
      throw new DomainException($"{from} cannot be transformed into itself.");
    }

    return Declined(from, heads).Grown(into, heads);
  }

  private ClassComposition Rebuilt(SocialClass socialClass, int heads)
  {
    var others = _presences.Where(presence => presence.Class != socialClass);

    return new ClassComposition(
      [..heads > 0 ? others.Append(ClassPresence.Of(socialClass, heads)) : others]
    );
  }

  public bool Equals(ClassComposition? other)
  {
    return other is not null
           && _presences.Count == other._presences.Count
           && _presences.All(presence => other.HeadsOf(presence.Class) == presence.Heads);
  }

  public override int GetHashCode()
  {
    return _presences.Aggregate(0, (hash, presence) => hash ^ HashCode.Combine(presence.Class, presence.Heads));
  }

  public override string ToString()
  {
    return _presences.Count == 0
      ? "no one"
      : string.Join(", ", _presences.OrderByDescending(presence => presence.Heads));
  }

  #endregion
}
