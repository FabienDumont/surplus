using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Domain.Tests.Simulation.Production;

public class LaborTimeTests
{
  #region Tests

  [Fact]
  public void A_labor_time_reads_as_a_duration_of_labor()
  {
    Assert.Equal("8h of labour", LaborTime.FromHours(8m).ToString());
  }

  [Fact]
  public void Addition_sums_durations()
  {
    var total = LaborTime.FromHours(3m) + LaborTime.FromHours(5m);

    Assert.Equal(LaborTime.FromHours(8m), total);
  }

  [Fact]
  public void Equal_durations_are_the_same_labor_time()
  {
    Assert.Equal(LaborTime.FromHours(2m), LaborTime.FromHours(2m));
  }

  [Fact]
  public void FromHours_rejects_negative_duration()
  {
    Assert.Throws<DomainException>(() => LaborTime.FromHours(-1m));
  }

  [Fact]
  public void FromHours_stores_the_given_duration()
  {
    var laborTime = LaborTime.FromHours(8m);

    Assert.Equal(8m, laborTime.Hours);
  }

  [Fact]
  public void Labor_times_are_comparable_by_duration()
  {
    Assert.True(LaborTime.FromHours(2m).CompareTo(LaborTime.FromHours(1m)) > 0);
    Assert.True(LaborTime.FromHours(1m).CompareTo(LaborTime.FromHours(2m)) < 0);
    Assert.Equal(0, LaborTime.FromHours(2m).CompareTo(LaborTime.FromHours(2m)));
  }

  [Fact]
  public void Labor_times_are_homogeneous_and_stand_in_definite_proportions()
  {
    var ratio = LaborTime.FromHours(20m).RatioTo(LaborTime.FromHours(4m));

    Assert.Equal(5m, ratio);
  }

  [Fact]
  public void No_proportion_can_be_formed_with_no_labor_at_all()
  {
    Assert.Throws<DomainException>(() => LaborTime.FromHours(1m).RatioTo(LaborTime.None));
  }

  [Fact]
  public void None_contains_no_labor()
  {
    Assert.True(LaborTime.None.IsNone);
    Assert.True(LaborTime.FromHours(0m).IsNone);
    Assert.False(LaborTime.FromHours(1m).IsNone);
  }

  [Fact]
  public void Labor_spread_over_a_number_of_things_falls_to_each_of_them()
  {
    Assert.Equal(LaborTime.FromHours(2m), LaborTime.FromHours(240m) / 120m);
    Assert.Throws<DomainException>(() => LaborTime.FromHours(240m) / 0m);
    Assert.Throws<DomainException>(() => LaborTime.FromHours(240m) / -2m);
  }

  [Fact]
  public void Scaling_repeats_the_same_labor_so_many_times_over()
  {
    Assert.Equal(LaborTime.FromHours(6m), LaborTime.FromHours(2m) * 3m);
    Assert.Equal(LaborTime.None, LaborTime.FromHours(2m) * 0m);
    Assert.Throws<DomainException>(() => LaborTime.FromHours(2m) * -1m);
  }

  [Fact]
  public void Subtraction_takes_labor_out_of_a_span_that_holds_it()
  {
    Assert.Equal(LaborTime.FromHours(6m), LaborTime.FromHours(12m) - LaborTime.FromHours(6m));
    Assert.Equal(LaborTime.None, LaborTime.FromHours(6m) - LaborTime.FromHours(6m));
    Assert.Throws<DomainException>(() => LaborTime.FromHours(6m) - LaborTime.FromHours(7m));
  }



  #endregion
}
