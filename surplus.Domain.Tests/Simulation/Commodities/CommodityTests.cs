using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.SharedKernel;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Commodities;

public class CommodityTests
{
    private static UseValue Clothing() => UseValue.Of("clothing", UnitOfMeasure.Of("coat"));

    [Fact]
    public void A_commodity_unites_a_use_value_and_a_value()
    {
        var coat = Commodity.Produce("  Coat  ", Clothing(), LaborTime.FromHours(20m));

        Assert.Equal("Coat", coat.Name);
        Assert.Equal(Clothing(), coat.UseValue);
        Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(20m)), coat.Value);
    }

    [Fact]
    public void A_commodity_must_have_a_name()
    {
        Assert.Throws<DomainException>(
            () => Commodity.Produce("   ", Clothing(), LaborTime.FromHours(1m)));
    }

    [Fact]
    public void A_useful_thing_owing_nothing_to_labor_is_no_commodity()
    {
        // Air and virgin soil are use-values, not commodities.
        Assert.Throws<DomainException>(
            () => Commodity.Produce("Air", UseValue.Of("breathing", UnitOfMeasure.Of("litre")), LaborTime.None));
    }

    [Fact]
    public void Each_produced_commodity_has_its_own_identity()
    {
        var first = Commodity.Produce("Coat", Clothing(), LaborTime.FromHours(20m));
        var second = Commodity.Produce("Coat", Clothing(), LaborTime.FromHours(20m));

        Assert.NotEqual(first.Id, second.Id);
    }
}
