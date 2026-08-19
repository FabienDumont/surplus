using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Politics;

/// <summary>
/// The state a country is constituted as: the form power takes, the ideology it
/// avows, and the statutes in force. The three vary independently, which is
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

  #endregion

  #region Ctors

  private Government(GovernmentForm form, Ideology ideology, List<Law> laws)
  {
    Form = form;
    Ideology = ideology;
    _laws = laws;
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

    return new Government(form, ideology, [..laws]);
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

    return new Government(Form, Ideology, [.._laws, law]);
  }

  public Government Repealing(Law law)
  {
    if (!HasEnacted(law))
    {
      throw new DomainException($"{law} is not on the books.");
    }

    return new Government(Form, Ideology, [.._laws.Where(enacted => enacted != law)]);
  }

  public Government TakingForm(GovernmentForm form)
  {
    if (Form == form)
    {
      throw new DomainException($"The state is already constituted as a {form}.");
    }

    return new Government(form, Ideology, [.._laws]);
  }

  public Government Adopting(Ideology ideology)
  {
    if (Ideology == ideology)
    {
      throw new DomainException($"The state already avows {ideology}.");
    }

    return new Government(Form, ideology, [.._laws]);
  }

  public bool Equals(Government? other)
  {
    return other is not null
           && Form == other.Form
           && Ideology == other.Ideology
           && _laws.Count == other._laws.Count
           && _laws.All(other.HasEnacted);
  }

  public override int GetHashCode()
  {
    return _laws.Aggregate(HashCode.Combine(Form, Ideology), (hash, law) => hash ^ law.GetHashCode());
  }

  public override string ToString()
  {
    return $"{Ideology} {Form} ({_laws.Count} laws in force)";
  }

  #endregion
}
