using Surplus.Application.Settings;

namespace Surplus.Application.Tests.Settings;

public class UserPreferencesTests
{
  #region Tests

  [Fact]
  public void A_player_who_has_chosen_nothing_reads_metric_units()
  {
    Assert.Equal(UnitSystem.Metric, UserPreferences.Default.Units);
  }

  [Fact]
  public void Choosing_another_system_of_measures_leaves_the_old_preferences_as_they_were()
  {
    var metric = UserPreferences.Of(UnitSystem.Metric);

    var imperial = metric.Measuring(UnitSystem.Imperial);

    Assert.Equal(UnitSystem.Imperial, imperial.Units);
    Assert.Equal(UnitSystem.Metric, metric.Units);
    Assert.Equal(UserPreferences.Of(UnitSystem.Imperial), imperial);
  }

  #endregion
}
