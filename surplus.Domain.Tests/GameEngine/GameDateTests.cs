using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Tests.GameEngine;

public class GameDateTests
{
  #region Tests

  [Fact]
  public void Dates_are_ordered_chronologically()
  {
    Assert.True(GameDate.Of(1848, 2, 21).CompareTo(GameDate.Of(1836, 1, 1)) > 0);
    Assert.True(GameDate.Of(1836, 1, 1).CompareTo(GameDate.Of(1848, 2, 21)) < 0);
    Assert.Equal(0, GameDate.Of(1836, 1, 1).CompareTo(GameDate.Of(1836, 1, 1)));
  }

  [Fact]
  public void NextDay_moves_one_day_forward()
  {
    Assert.Equal(GameDate.Of(1836, 1, 2), GameDate.Of(1836, 1, 1).NextDay());
    Assert.Equal(GameDate.Of(1837, 1, 1), GameDate.Of(1836, 12, 31).NextDay());
  }

  [Fact]
  public void Of_builds_a_calendar_date()
  {
    var date = GameDate.Of(1836, 1, 1);

    Assert.Equal(new DateOnly(1836, 1, 1), date.Date);
    Assert.Equal("1836-01-01", date.ToString());
  }

  [Theory]
  [InlineData(1836, 13, 1)]
  [InlineData(1836, 2, 30)]
  [InlineData(0, 1, 1)]
  public void Of_rejects_impossible_dates(int year, int month, int day)
  {
    Assert.Throws<DomainException>(() => GameDate.Of(year, month, day));
  }

  #endregion
}
