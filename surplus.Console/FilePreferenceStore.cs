using Surplus.Application.Settings;

namespace Surplus.Console;

/// <summary>
/// Keeps the player's preferences in a small text file in their profile, one
/// <c>key=value</c> per line. A missing or unreadable setting falls back to its
/// default rather than stopping the game.
/// </summary>
public sealed class FilePreferenceStore : IPreferenceStore
{
  #region Fields

  private const string UnitsKey = "units";

  private readonly string _path;

  #endregion

  #region Ctors

  public FilePreferenceStore(string path)
  {
    _path = path;
  }

  #endregion

  #region Methods

  /// <summary>The store in the user's application data folder.</summary>
  public static FilePreferenceStore InUserProfile()
  {
    return new FilePreferenceStore(
      Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "surplus", "preferences.txt"));
  }

  public UserPreferences Load()
  {
    if (!File.Exists(_path))
    {
      return UserPreferences.Default;
    }

    var units = File.ReadAllLines(_path)
      .Select(line => line.Split('=', 2, StringSplitOptions.TrimEntries))
      .Where(pair => pair.Length == 2 && pair[0] == UnitsKey)
      .Select(pair => Enum.TryParse<UnitSystem>(pair[1], true, out var system) ? system : (UnitSystem?)null)
      .LastOrDefault(system => system is not null);

    return units is { } system ? UserPreferences.Of(system) : UserPreferences.Default;
  }

  public void Save(UserPreferences preferences)
  {
    Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    File.WriteAllLines(_path, [$"{UnitsKey}={preferences.Units.ToString().ToLowerInvariant()}"]);
  }

  #endregion
}
