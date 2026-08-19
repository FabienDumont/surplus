using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Domain.Tests.Simulation.Production;

public class UseValueTests
{
  #region Tests

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void A_use_value_must_satisfy_some_want(string blank)
  {
    Assert.Throws<DomainException>(() => UseValue.Of(blank, UnitOfMeasure.Of("yard")));
  }

  [Fact]
  public void A_use_value_reads_as_the_want_it_satisfies()
  {
    Assert.Equal("satisfies the want for warmth", UseValue.Of("warmth", UnitOfMeasure.Of("coat")).ToString());
  }

  [Fact]
  public void Of_keeps_the_trimmed_want_and_its_unit()
  {
    var useValue = UseValue.Of("  warmth  ", UnitOfMeasure.Of("coat"));

    Assert.Equal("warmth", useValue.SatisfiedWant);
    Assert.Equal(UnitOfMeasure.Of("coat"), useValue.Unit);
  }

  [Fact]
  public void Use_values_are_qualitative_and_therefore_incommensurable()
  {
    // Different kinds of usefulness differ in quality, not quantity:
    // the type must never grow an ordering.
    Assert.False(typeof(IComparable<UseValue>).IsAssignableFrom(typeof(UseValue)));
    Assert.False(typeof(IComparable).IsAssignableFrom(typeof(UseValue)));
  }

  [Fact]
  public void Use_values_satisfying_the_same_want_in_the_same_unit_are_of_the_same_kind()
  {
    Assert.Equal(UseValue.Of("warmth", UnitOfMeasure.Of("coat")), UseValue.Of("warmth", UnitOfMeasure.Of("coat")));
    Assert.NotEqual(
      UseValue.Of("warmth", UnitOfMeasure.Of("coat")), UseValue.Of("clothing material", UnitOfMeasure.Of("yard"))
    );
  }

  #endregion
}
