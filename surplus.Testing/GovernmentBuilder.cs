using Surplus.Domain.Simulation.Politics;

namespace Surplus.Testing;

/// <summary>
/// Builds a <see cref="Government" />. Defaults to a liberal republic with
/// nothing yet on the books — the July Monarchy's neighbours, roughly.
/// </summary>
public sealed class GovernmentBuilder
{
  #region Fields

  private GovernmentForm _form = GovernmentForm.Republic;
  private Ideology _ideology = Ideology.Liberalism;
  private List<Law> _laws = [];
  private bool _relieves;

  #endregion

  #region Methods

  public GovernmentBuilder WithForm(GovernmentForm form)
  {
    _form = form;

    return this;
  }

  public GovernmentBuilder WithIdeology(Ideology ideology)
  {
    _ideology = ideology;

    return this;
  }

  public GovernmentBuilder WithLaws(params Law[] laws)
  {
    _laws = [..laws];

    return this;
  }

  /// <summary>A state that keeps its poor on relief.</summary>
  public GovernmentBuilder RelievingThePoor()
  {
    _relieves = true;

    return this;
  }

  public Government Build()
  {
    var government = Government.Of(_form, _ideology, [.._laws]);

    return _relieves ? government.Relieving() : government;
  }

  #endregion
}
