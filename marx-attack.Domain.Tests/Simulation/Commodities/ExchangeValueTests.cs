using MarxAttack.Domain.Simulation.Commodities;
using MarxAttack.Domain.Simulation.Production;
using MarxAttack.Domain.SharedKernel;

namespace MarxAttack.Domain.Tests.Simulation.Commodities;

public class ExchangeValueTests
{
    private static Commodity Linen(decimal hoursPerYard = 1m) =>
        Commodity.Produce(
            "Linen",
            UseValue.Of("clothing material", UnitOfMeasure.Of("yard")),
            LaborTime.FromHours(hoursPerYard));

    private static Commodity Coat(decimal hoursPerCoat = 20m) =>
        Commodity.Produce(
            "Coat",
            UseValue.Of("warmth", UnitOfMeasure.Of("coat")),
            LaborTime.FromHours(hoursPerCoat));

    [Fact]
    public void Exchange_value_is_the_proportion_between_two_values()
    {
        // 20 hours congealed in a coat, 1 hour per yard of linen:
        var exchangeValue = ExchangeValue.Between(Coat(), Linen());

        Assert.Equal(20m, exchangeValue.Proportion);
    }

    [Fact]
    public void Exchange_value_reads_as_the_classic_equation()
    {
        Assert.Equal(
            "1 coat of Coat = 20 yard of Linen",
            ExchangeValue.Between(Coat(), Linen()).ToString());
    }

    [Fact]
    public void Reversing_the_relation_inverts_the_proportion()
    {
        Assert.Equal(0.05m, ExchangeValue.Between(Linen(), Coat()).Proportion);
    }

    [Fact]
    public void Commodities_of_equal_value_exchange_one_for_one()
    {
        var exchangeValue = ExchangeValue.Between(Coat(10m), Linen(10m));

        Assert.Equal(1m, exchangeValue.Proportion);
    }

    [Fact]
    public void A_commodity_cannot_express_its_value_in_its_own_body()
    {
        var coat = Coat();

        Assert.Throws<DomainException>(() => ExchangeValue.Between(coat, coat));
    }

    [Fact]
    public void Exchange_value_relates_use_values_of_different_kinds()
    {
        // "x linen = y linen" is no expression of value.
        Assert.Throws<DomainException>(() => ExchangeValue.Between(Linen(1m), Linen(2m)));
    }

    [Fact]
    public void The_relative_and_equivalent_commodities_are_kept_in_the_relation()
    {
        var coat = Coat();
        var linen = Linen();

        var exchangeValue = ExchangeValue.Between(coat, linen);

        Assert.Same(coat, exchangeValue.Relative);
        Assert.Same(linen, exchangeValue.Equivalent);
    }
}
