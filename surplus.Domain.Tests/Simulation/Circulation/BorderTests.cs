using Surplus.Domain.Simulation.Circulation;

namespace Surplus.Domain.Tests.Simulation.Circulation;

public class BorderTests
{
  #region Tests

  [Fact]
  public void A_border_runs_both_ways()
  {
    var kent = ProvinceId.New();
    var lancashire = ProvinceId.New();

    Assert.Equal(new Border(kent, lancashire), new Border(lancashire, kent));
    Assert.Equal(new Border(kent, lancashire).GetHashCode(), new Border(lancashire, kent).GetHashCode());
    Assert.NotEqual(new Border(kent, lancashire), new Border(kent, ProvinceId.New()));
    Assert.False(new Border(kent, lancashire).Equals(null));
  }

  [Fact]
  public void A_border_joins_its_two_provinces_and_no_other()
  {
    var kent = ProvinceId.New();
    var lancashire = ProvinceId.New();
    var border = new Border(kent, lancashire);

    Assert.True(border.Joins(kent));
    Assert.True(border.Joins(lancashire));
    Assert.False(border.Joins(ProvinceId.New()));
  }

  #endregion
}
