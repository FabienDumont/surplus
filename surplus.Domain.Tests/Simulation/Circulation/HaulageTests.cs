using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Circulation;

namespace Surplus.Domain.Tests.Simulation.Circulation;

public class HaulageTests
{
  #region Fields

  private static readonly CommodityId Carriage = CommodityId.New();
  private static readonly CommodityId Cloth = CommodityId.New();
  private static readonly CommodityId Corn = CommodityId.New();

  #endregion

  #region Tests

  [Fact]
  public void Each_commodity_takes_up_its_own_room()
  {
    var haulage = Haulage.Of(Carriage, [new Bulk(Corn, 1m), new Bulk(Cloth, 0.0005m)]);

    Assert.Equal(Carriage, haulage.Carriage);
    Assert.Equal(1m, haulage.LoadsPer(Corn));
    Assert.Equal(0.0005m, haulage.LoadsPer(Cloth));
  }

  [Fact]
  public void A_commodity_nobody_listed_takes_up_none()
  {
    Assert.Equal(0m, Haulage.Of(Carriage, [new Bulk(Corn, 1m)]).LoadsPer(Cloth));
  }

  [Fact]
  public void Bulk_cannot_be_negative_or_given_twice()
  {
    Assert.Throws<DomainException>(() => Haulage.Of(Carriage, [new Bulk(Corn, -1m)]));
    Assert.Throws<DomainException>(() => Haulage.Of(Carriage, [new Bulk(Corn, 1m), new Bulk(Corn, 2m)]));
  }

  [Fact]
  public void Carriage_does_not_carry_itself()
  {
    Assert.Throws<DomainException>(() => Haulage.Of(Carriage, [new Bulk(Carriage, 1m)]));
  }

  [Fact]
  public void Without_haulage_nothing_needs_carrying()
  {
    Assert.Null(Haulage.None.Carriage);
    Assert.Equal(0m, Haulage.None.LoadsPer(Corn));
  }

  [Fact]
  public void Haulages_of_the_same_carriage_and_bulk_are_the_same_haulage()
  {
    var haulage = Haulage.Of(Carriage, [new Bulk(Corn, 1m)]);
    var same = Haulage.Of(Carriage, [new Bulk(Corn, 1m)]);

    Assert.Equal(haulage, same);
    Assert.Equal(haulage.GetHashCode(), same.GetHashCode());
    Assert.NotEqual(haulage, Haulage.None);
    Assert.False(haulage.Equals(null));
  }

  #endregion
}
