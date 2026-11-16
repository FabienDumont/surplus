using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Politics;

/// <summary>
/// The state a country is constituted as: the form power takes, the ideology it
/// avows, the statutes in force, and whether it relieves the poor. The three vary independently, which is
/// what lets a republic rest on slavery and a fascist creed be worn by one
/// dictator or by a board of them.
/// Immutable — a constitution is replaced, not edited.
/// </summary>
public sealed record Government
{
  #region Fields

  private readonly List<Law> _laws;

  #endregion

  #region Properties

  public GovernmentForm Form { get; }
  public Ideology Ideology { get; }
  public IReadOnlyList<Law> Laws => _laws;
  public IdeologyProfile Doctrine => Ideology.Profile();

  /// <summary>
  /// Whether the state keeps those with no title of their own: the workers it
  /// has no wage for, and the paupers below them. It is a policy and not a law,
  /// because a law here outlaws a relation of production, and relief outlaws
  /// nothing: it keeps the reserve army alive and leaves it a reserve. The
  /// Old Poor Law did it at the parish's charge; the New Poor Law of 1834 did it
  /// in the workhouse, on the principle of "less eligibility", that relief must
  /// be worse than the worst wage. Relief here is half a basket for that reason.
  /// </summary>
  public bool RelievesThePoor { get; }

  #endregion

  #region Ctors

  private Government(GovernmentForm form, Ideology ideology, List<Law> laws, bool relievesThePoor)
  {
    Form = form;
    Ideology = ideology;
    _laws = laws;
    RelievesThePoor = relievesThePoor;
  }

  #endregion

  #region Methods

  public static Government Of(GovernmentForm form, Ideology ideology, params Law[] laws)
  {
    var duplicate = laws.GroupBy(law => law).FirstOrDefault(group => group.Count() > 1);

    if (duplicate is not null)
    {
      throw new DomainException($"{duplicate.Key} is on the books twice.");
    }

    return new Government(form, ideology, [..laws], false);
  }

  public bool HasEnacted(Law law)
  {
    return _laws.Contains(law);
  }

  /// <summary>Whether any statute in force puts this relation outside the law.</summary>
  public bool Forbids(ProductionRelation relation)
  {
    return _laws.Any(law => law.Forbids(relation));
  }

  public Government Enacting(Law law)
  {
    if (HasEnacted(law))
    {
      throw new DomainException($"{law} is already on the books.");
    }

    return new Government(Form, Ideology, [.._laws, law], RelievesThePoor);
  }

  public Government Repealing(Law law)
  {
    if (!HasEnacted(law))
    {
      throw new DomainException($"{law} is not on the books.");
    }

    return new Government(Form, Ideology, [.._laws.Where(enacted => enacted != law)], RelievesThePoor);
  }

  public Government TakingForm(GovernmentForm form)
  {
    if (Form == form)
    {
      throw new DomainException($"The state is already constituted as a {form}.");
    }

    return new Government(form, Ideology, [.._laws], RelievesThePoor);
  }

  public Government Adopting(Ideology ideology)
  {
    if (Ideology == ideology)
    {
      throw new DomainException($"The state already avows {ideology}.");
    }

    return new Government(Form, ideology, [.._laws], RelievesThePoor);
  }

  /// <summary>The same state, now relieving its poor.</summary>
  public Government Relieving()
  {
    if (RelievesThePoor)
    {
      throw new DomainException("The state already relieves its poor.");
    }

    return new Government(Form, Ideology, [.._laws], true);
  }

  /// <summary>The same state, leaving its poor to themselves.</summary>
  public Government WithdrawingRelief()
  {
    if (!RelievesThePoor)
    {
      throw new DomainException("The state gives its poor no relief to withdraw.");
    }

    return new Government(Form, Ideology, [.._laws], false);
  }

  public bool Equals(Government? other)
  {
    return other is not null
           && Form == other.Form
           && Ideology == other.Ideology
           && RelievesThePoor == other.RelievesThePoor
           && _laws.Count == other._laws.Count
           && _laws.All(other.HasEnacted);
  }

  public override int GetHashCode()
  {
    return _laws.Aggregate(HashCode.Combine(Form, Ideology, RelievesThePoor), (hash, law) => hash ^ law.GetHashCode());
  }

  public override string ToString()
  {
    return $"{Ideology} {Form} ({_laws.Count} laws in force)";
  }

  #endregion
}
