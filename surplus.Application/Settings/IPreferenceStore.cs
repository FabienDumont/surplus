namespace Surplus.Application.Settings;

/// <summary>
/// Where a player's preferences are kept between runs. The application says
/// what is kept; each front-end decides where, be it a file, a browser's
/// storage or a user account.
/// </summary>
public interface IPreferenceStore
{
  /// <summary>The preferences last saved, or the defaults if none ever were.</summary>
  UserPreferences Load();

  void Save(UserPreferences preferences);
}
