using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Domain.Tests.Simulation.Production;

public class QuantityTests
{
  #region Fields

  private static readonly UnitOfMeasure Coat = UnitOfMeasure.Of("coat");
  private static readonly UnitOfMeasure Yard = UnitOfMeasure.Of("yard");

  #endregion

  #region Tests

  [Fact]
  public void A_quantity_cannot_be_negative()
  {
    Assert.Throws<DomainException>(() => Quantity.Of(-1m, Yard));
    Assert.Throws<DomainException>(() => Quantity.Of(1m, Yard) * -1m);
  }

  [Fact]
  public void A_quantity_reads_as_its_amount_in_its_own_unit()
  {
    Assert.Equal("20 yard", Quantity.Of(20m, Yard).ToString());
  }

  [Fact]
  public void An_exhausted_heap_is_still_measured_in_its_unit()
  {
    var nothing = Quantity.NoneOf(Yard);

    Assert.True(nothing.IsNone);
    Assert.Equal(Yard, nothing.Unit);
  }

  [Fact]
  public void Nothing_can_be_taken_beyond_what_is_there()
  {
    Assert.Throws<DomainException>(() => Quantity.Of(5m, Yard) - Quantity.Of(6m, Yard));
    Assert.Equal(Quantity.NoneOf(Yard), Quantity.Of(5m, Yard) - Quantity.Of(5m, Yard));
  }

  [Fact]
  public void Quantities_of_different_use_values_are_incommensurable()
  {
    var linen = Quantity.Of(20m, Yard);
    var coats = Quantity.Of(1m, Coat);

    Assert.Throws<DomainException>(() => linen + coats);
    Assert.Throws<DomainException>(() => linen - coats);
    Assert.Throws<DomainException>(() => linen.RatioTo(coats));
    Assert.Throws<DomainException>(() => linen.CompareTo(coats));
    Assert.NotEqual(Quantity.Of(1m, Yard), Quantity.Of(1m, Coat));
  }

  [Fact]
  public void Quantities_of_the_same_use_value_add_up()
  {
    Assert.Equal(Quantity.Of(30m, Yard), Quantity.Of(20m, Yard) + Quantity.Of(10m, Yard));
  }

  [Fact]
  public void Quantities_of_the_same_use_value_are_ordered_by_their_amount()
  {
    var less = Quantity.Of(1m, Yard);
    var more = Quantity.Of(2m, Yard);

    Assert.True(more.CompareTo(less) > 0);
    Assert.True(less.CompareTo(more) < 0);
    Assert.Equal(0, less.CompareTo(Quantity.Of(1m, Yard)));
    Assert.True(more.CompareTo(null) > 0);
  }

  [Fact]
  public void Quantities_of_the_same_use_value_stand_in_proportion()
  {
    Assert.Equal(4m, Quantity.Of(20m, Yard).RatioTo(Quantity.Of(5m, Yard)));
    Assert.Throws<DomainException>(() => Quantity.Of(20m, Yard).RatioTo(Quantity.NoneOf(Yard)));
  }

  [Fact]
  public void Scaling_a_quantity_leaves_the_use_value_what_it_was()
  {
    var scaled = Quantity.Of(20m, Yard) * 3m;

    Assert.Equal(Quantity.Of(60m, Yard), scaled);
    Assert.Equal(Yard, scaled.Unit);
  }

  #endregion
}
