using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Tests.Simulation.Politics;

public class LawsTests
{
  #region Methods

  [Fact]
  public void Every_law_forbids_something()
  {
    foreach (var law in Enum.GetValues<Law>())
    {
      Assert.NotEmpty(law.Forbids());
    }
  }

  [Fact]
  public void Abolition_strikes_at_both_sides_of_the_relation()
  {
    Assert.True(Law.AbolitionOfSlavery.Forbids(ProductionRelation.IsOwned));
    Assert.True(Law.AbolitionOfSlavery.Forbids(ProductionRelation.OwnsProducers));
    Assert.False(Law.AbolitionOfSlavery.Forbids(ProductionRelation.SellsLaborPower));
  }

  [Fact]
  public void Enclosure_is_a_law_and_the_most_candid_of_them()
  {
    // It extinguishes customary right, and those who held by it are left with
    // nothing to sell but themselves.
    Assert.True(Law.EnclosureActs.Forbids(ProductionRelation.HoldsOwnMeans));
  }

  [Theory]
  [InlineData(Law.SerfEmancipation, ProductionRelation.BoundToTheLand)]
  [InlineData(Law.LandReform, ProductionRelation.OwnsLand)]
  [InlineData(Law.NationalisationOfIndustry, ProductionRelation.OwnsCapital)]
  [InlineData(Law.AbolitionOfWageLabour, ProductionRelation.SellsLaborPower)]
  public void Each_law_puts_its_relation_outside_the_law(Law law, ProductionRelation relation)
  {
    Assert.True(law.Forbids(relation));
  }

  [Fact]
  public void What_the_freed_become_depends_on_who_kept_the_land()
  {
    // The same statute, opposite results. This is the whole of the mechanism.
    Assert.Equal(SocialClass.Freedmen, Laws.Frees(ProductionRelation.IsOwned, true));
    Assert.Equal(SocialClass.Plebeians, Laws.Frees(ProductionRelation.IsOwned, false));

    Assert.Equal(SocialClass.AgriculturalProletariat, Laws.Frees(ProductionRelation.BoundToTheLand, true));
    Assert.Equal(SocialClass.Peasantry, Laws.Frees(ProductionRelation.BoundToTheLand, false));
  }

  [Fact]
  public void The_planter_keeping_his_acres_becomes_a_landlord()
  {
    Assert.Equal(SocialClass.Landowners, Laws.Frees(ProductionRelation.OwnsProducers, true));
  }

  [Fact]
  public void Every_relation_has_a_resolution_under_either_condition()
  {
    // A relation with no successor is a statute that could strand a population.
    foreach (var relation in Enum.GetValues<ProductionRelation>())
    {
      Assert.NotEqual(default, Laws.Frees(relation, true));
      Assert.NotEqual(default, Laws.Frees(relation, false));
    }
  }

  #endregion
}
