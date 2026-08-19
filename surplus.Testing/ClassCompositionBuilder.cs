using Surplus.Domain.Simulation.Society;

namespace Surplus.Testing;

/// <summary>
/// Builds a <see cref="ClassComposition" />. Defaults to a feudal village: a
/// lord and his priest living on the surplus of nine hundred serfs.
/// </summary>
public sealed class ClassCompositionBuilder
{
  #region Fields

  private readonly List<ClassPresence> _presences =
  [
    ClassPresence.Of(SocialClass.Serfs, 900),
    ClassPresence.Of(SocialClass.FeudalLords, 5),
    ClassPresence.Of(SocialClass.Clergy, 10)
  ];

  #endregion

  #region Methods

  /// <summary>Replaces the default composition outright.</summary>
  public ClassCompositionBuilder Of(params ClassPresence[] presences)
  {
    _presences.Clear();
    _presences.AddRange(presences);

    return this;
  }

  public ClassCompositionBuilder With(SocialClass socialClass, int heads)
  {
    _presences.RemoveAll(presence => presence.Class == socialClass);
    _presences.Add(ClassPresence.Of(socialClass, heads));

    return this;
  }

  public ClassComposition Build() => ClassComposition.Of([.._presences]);

  #endregion
}
