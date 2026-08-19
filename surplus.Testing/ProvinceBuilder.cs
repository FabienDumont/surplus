using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Testing;

/// <summary>
/// Builds a <see cref="Province" />, by default a feudal Île-de-France.
/// </summary>
public sealed class ProvinceBuilder
{
  #region Fields

  private ClassComposition _composition = new ClassCompositionBuilder().Build();
  private ProvinceId _id = ProvinceId.New();
  private string _name = "Île-de-France";

  #endregion

  #region Methods

  public ProvinceBuilder WithId(ProvinceId id)
  {
    _id = id;

    return this;
  }

  public ProvinceBuilder WithName(string name)
  {
    _name = name;

    return this;
  }

  public ProvinceBuilder WithComposition(ClassComposition composition)
  {
    _composition = composition;

    return this;
  }

  public ProvinceBuilder WithClasses(params ClassPresence[] presences) =>
    WithComposition(ClassComposition.Of(presences));

  /// <summary>Shorthand for a province whose population has yet to be settled.</summary>
  public ProvinceBuilder Unpeopled() => WithComposition(ClassComposition.Empty);

  public Province Build() => Province.Load(_id, _name, _composition);

  #endregion
}
