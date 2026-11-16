namespace Surplus.Application.Settings;

/// <summary>
/// How one player likes the game shown to them. Preferences belong to the
/// person and not to the game: they are kept apart from any save, and changing
/// them changes nothing in the world, only what is read off it.
/// </summary>
public sealed record UserPreferences
{
  #region Fields

  /// <summary>What a player sees before they have chosen anything.</summary>
  public static readonly UserPreferences Default = new(UnitSystem.Metric);

  #endregion

  #region Properties

  public UnitSystem Units { get; }

  #endregion

  #region Ctors

  private UserPreferences(UnitSystem units)
  {
    Units = units;
  }

  #endregion

  #region Methods

  public static UserPreferences Of(UnitSystem units)
  {
    return new UserPreferences(units);
  }

  public UserPreferences Measuring(UnitSystem units)
  {
    return new UserPreferences(units);
  }

  #endregion
}
