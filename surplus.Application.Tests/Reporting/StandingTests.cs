using Surplus.Application.Reporting;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Testing;

namespace Surplus.Application.Tests.Reporting;

public class StandingTests
{
  #region Tests

  [Fact]
  public void Only_wage_earners_without_a_branch_are_without_work()
  {
    var weaving = new BranchBuilder().WorkedBy(SocialClass.Proletariat, 60).Build();
    var lancashire = new ProvinceBuilder()
      .WithClasses(
        ClassPresence.Of(SocialClass.Proletariat, 100), ClassPresence.Of(SocialClass.PettyBourgeoisie, 50),
        ClassPresence.Of(SocialClass.Bourgeoisie, 5), ClassPresence.Of(SocialClass.Lumpenproletariat, 7))
      .Working(weaving).Build();
    var kingdom = new CountryBuilder().WithProvinces(lancashire).Build();

    // Forty proletarians out of work; the craftsmen live off their workshops and the paupers off nothing.
    Assert.Equal(40, Standing.WithoutWork(kingdom));
  }

  [Fact]
  public void A_province_is_kept_as_long_as_its_own_store_lasts()
  {
    var cloth = CommodityId.New();
    var metre = UnitOfMeasure.Of("m");
    var basket = Subsistence.Of(Stock.Of(cloth, Quantity.Of(0.5m, metre)));
    var lancashire = new ProvinceBuilder().WithClasses(ClassPresence.Of(SocialClass.Proletariat, 100))
      .Holding(Stock.Of(cloth, Quantity.Of(150m, metre))).Build();

    Assert.Equal(3m, Standing.DaysOfSubsistence(lancashire, basket));
    Assert.Null(Standing.DaysOfSubsistence(new ProvinceBuilder().Unpeopled().Build(), basket));
  }

  [Fact]
  public void A_class_lives_by_the_wage_when_the_wage_is_its_only_title()
  {
    Assert.True(Standing.LivesByTheWage(ClassPresence.Of(SocialClass.AgriculturalProletariat, 1)));
    Assert.False(Standing.LivesByTheWage(ClassPresence.Of(SocialClass.Peasantry, 1)));
    Assert.False(Standing.LivesByTheWage(ClassPresence.Of(SocialClass.Lumpenproletariat, 1)));
  }

  #endregion
}
