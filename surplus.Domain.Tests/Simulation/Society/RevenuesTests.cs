using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Tests.Simulation.Society;

public class RevenuesTests
{
  #region Tests

  [Theory]
  [InlineData(IncomeSource.Rent)]
  [InlineData(IncomeSource.Profit)]
  [InlineData(IncomeSource.MerchantProfit)]
  [InlineData(IncomeSource.Interest)]
  [InlineData(IncomeSource.Tithe)]
  public void The_appropriators_are_served_before_anyone_else(IncomeSource income)
  {
    Assert.Equal(Access.First, income.Title());
  }

  [Theory]
  [InlineData(IncomeSource.OwnProduct)]
  [InlineData(IncomeSource.Maintenance)]
  [InlineData(IncomeSource.Distribution)]
  public void Those_who_live_off_their_own_product_or_are_kept_have_a_title_whether_employed_or_not(IncomeSource income)
  {
    Assert.Equal(Access.Whole, income.Title());
  }

  [Fact]
  public void A_wage_is_a_title_only_for_labour_power_actually_bought()
  {
    Assert.Equal(Access.Employed, IncomeSource.Wages.Title());
  }

  [Fact]
  public void Plunder_is_no_title_at_all()
  {
    Assert.Equal(Access.None, IncomeSource.Plunder.Title());
  }

  [Fact]
  public void Every_revenue_gives_some_title_or_says_it_gives_none()
  {
    Assert.All(Enum.GetValues<IncomeSource>(), income => Assert.True(Enum.IsDefined(income.Title())));
    Assert.Throws<DomainException>(() => ((IncomeSource)999).Title());
  }

  #endregion
}
