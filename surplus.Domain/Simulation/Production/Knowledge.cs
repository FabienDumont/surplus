using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// What a country knows how to do, and what it is on the way to finding.
/// Knowing is not using: a country may know the power loom for decades and
/// weave by hand, for want of machines or of a class that can work them.
/// </summary>
public sealed record Knowledge
{
  #region Fields

  private readonly List<string> _known;
  private readonly List<Development> _underWay;

  #endregion

  #region Properties

  public static Knowledge None { get; } = new([], []);

  /// <summary>Technique names, in the order they became known.</summary>
  public IReadOnlyList<string> Known => _known;

  /// <summary>Techniques partly found, in the order work on them began.</summary>
  public IReadOnlyList<Development> UnderWay => _underWay;

  #endregion

  #region Ctors

  private Knowledge(List<string> known, List<Development> underWay)
  {
    _known = known;
    _underWay = underWay;
  }

  #endregion

  #region Methods

  public static Knowledge Of(IEnumerable<string> known, IEnumerable<Development> underWay)
  {
    List<string> knowing = [.. known];
    List<Development> finding = [.. underWay];

    if (knowing.Distinct().Count() != knowing.Count)
    {
      throw new DomainException("A technique cannot be known twice.");
    }

    if (finding.Select(development => development.Technique).Distinct().Count() != finding.Count)
    {
      throw new DomainException("A technique cannot be under way twice.");
    }

    if (finding.Any(development => knowing.Contains(development.Technique)))
    {
      throw new DomainException("What is known is no longer being found out.");
    }

    return new Knowledge(knowing, finding);
  }

  public bool Knows(string technique)
  {
    return _known.Contains(technique);
  }

  /// <summary>The weighed labour spent toward a technique so far; none when it is not under way.</summary>
  public LaborTime Spent(string technique)
  {
    return _underWay.FirstOrDefault(development => development.Technique == technique)?.Spent ?? LaborTime.None;
  }

  /// <summary>The same knowledge with a technique known, and no longer under way.</summary>
  public Knowledge Learnt(string technique)
  {
    return Knows(technique)
      ? this
      : new Knowledge([.. _known, technique], [.. _underWay.Where(development => development.Technique != technique)]);
  }

  /// <summary>The same knowledge with more labour spent toward a technique not yet known.</summary>
  public Knowledge Advanced(string technique, LaborTime hours)
  {
    if (Knows(technique))
    {
      throw new DomainException($"{technique} is already known; there is nothing left to find.");
    }

    var index = _underWay.FindIndex(development => development.Technique == technique);
    List<Development> underWay = [.. _underWay];

    if (index < 0)
    {
      underWay.Add(new Development(technique, hours));
    }
    else
    {
      underWay[index] = new Development(technique, underWay[index].Spent + hours);
    }

    return new Knowledge([.. _known], underWay);
  }

  public bool Equals(Knowledge? other)
  {
    return other is not null && _known.SequenceEqual(other._known) && _underWay.SequenceEqual(other._underWay);
  }

  public override int GetHashCode()
  {
    return _underWay.Aggregate(_known.Aggregate(0, HashCode.Combine), HashCode.Combine);
  }

  #endregion
}
