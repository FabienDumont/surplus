using MarxAttack.Domain.Simulation.Production;
using MarxAttack.Domain.SharedKernel;

namespace MarxAttack.Domain.Tests.Simulation.Production;

public class LaborTimeTests
{
    [Fact]
    public void FromHours_stores_the_given_duration()
    {
        var laborTime = LaborTime.FromHours(8m);

        Assert.Equal(8m, laborTime.Hours);
    }

    [Fact]
    public void FromHours_rejects_negative_duration()
    {
        Assert.Throws<DomainException>(() => LaborTime.FromHours(-1m));
    }

    [Fact]
    public void None_contains_no_labor()
    {
        Assert.True(LaborTime.None.IsNone);
        Assert.True(LaborTime.FromHours(0m).IsNone);
        Assert.False(LaborTime.FromHours(1m).IsNone);
    }

    [Fact]
    public void Addition_sums_durations()
    {
        var total = LaborTime.FromHours(3m) + LaborTime.FromHours(5m);

        Assert.Equal(LaborTime.FromHours(8m), total);
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
    public void Labor_times_are_comparable_by_duration()
    {
        Assert.True(LaborTime.FromHours(2m).CompareTo(LaborTime.FromHours(1m)) > 0);
        Assert.True(LaborTime.FromHours(1m).CompareTo(LaborTime.FromHours(2m)) < 0);
        Assert.Equal(0, LaborTime.FromHours(2m).CompareTo(LaborTime.FromHours(2m)));
    }

    [Fact]
    public void Equal_durations_are_the_same_labor_time()
    {
        Assert.Equal(LaborTime.FromHours(2m), LaborTime.FromHours(2m));
    }
}
