using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Tests.Simulation.Countries;

public class RationsTests
{
  #region Fields

  private static readonly ProvinceId Lancashire = ProvinceId.New();

  #endregion

  #region Tests

  [Fact]
  public void The_workless_are_counted_across_every_class()
  {
    var rations = new Rations(
      [
        new Ration(Lancashire, SocialClass.Proletariat, 100, 40, 60m, 60m),
        new Ration(Lancashire, SocialClass.Lumpenproletariat, 10, 10, 0m, 0m)
      ],
      1m);

    Assert.Equal(50, rations.Workless);
    Assert.False(rations.StoreFellShort);
  }

  [Fact]
  public void Rations_of_the_same_claims_and_share_are_the_same_rations()
  {
    var rations = new Rations([new Ration(Lancashire, SocialClass.Proletariat, 100, 40, 60m, 30m)], 0.5m);
    var same = new Rations([new Ration(Lancashire, SocialClass.Proletariat, 100, 40, 60m, 30m)], 0.5m);

    Assert.Equal(rations, same);
    Assert.Equal(rations.GetHashCode(), same.GetHashCode());
    Assert.NotEqual(rations, rations with { StoreShare = 1m });
    Assert.False(rations.Equals(null));
    Assert.True(rations.StoreFellShort);
  }

  #endregion
}
