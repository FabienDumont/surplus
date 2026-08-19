using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Domain.Tests.Simulation.Production;

public class UnitOfMeasureTests
{
  #region Tests

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void A_unit_must_have_a_name(string blank)
  {
    Assert.Throws<DomainException>(() => UnitOfMeasure.Of(blank));
  }

  [Fact]
  public void Of_keeps_the_trimmed_name()
  {
    var unit = UnitOfMeasure.Of("  yard  ");

    Assert.Equal("yard", unit.Name);
    Assert.Equal("yard", unit.ToString());
  }

  [Fact]
  public void Units_with_the_same_name_are_the_same_unit()
  {
    Assert.Equal(UnitOfMeasure.Of("yard"), UnitOfMeasure.Of("yard"));
    Assert.NotEqual(UnitOfMeasure.Of("yard"), UnitOfMeasure.Of("coat"));
  }

  #endregion
}
