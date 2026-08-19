using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Tests.Simulation.Society;

public class SocialClassesTests
{
  #region Methods

  [Fact]
  public void Every_class_history_has_thrown_up_is_registered()
  {
    // The register is exhaustive by construction: a class added to the enum
    // without a profile is a class the simulation cannot reason about.
    foreach (var socialClass in Enum.GetValues<SocialClass>())
    {
      Assert.Equal(socialClass, socialClass.Profile().Class);
    }
  }

  [Fact]
  public void Every_class_is_native_to_at_least_one_mode_of_production()
  {
    foreach (var socialClass in Enum.GetValues<SocialClass>())
    {
      Assert.NotEmpty(socialClass.Profile().NativeModes);
    }
  }

  [Theory]
  [InlineData(SocialClass.Slaves, ProductionRelation.IsOwned, IncomeSource.Maintenance)]
  [InlineData(SocialClass.SlaveOwners, ProductionRelation.OwnsProducers, IncomeSource.Profit)]
  [InlineData(SocialClass.Serfs, ProductionRelation.BoundToTheLand, IncomeSource.OwnProduct)]
  [InlineData(SocialClass.FeudalLords, ProductionRelation.OwnsLand, IncomeSource.Rent)]
  [InlineData(SocialClass.Clergy, ProductionRelation.OwnsLand, IncomeSource.Tithe)]
  [InlineData(SocialClass.GuildMasters, ProductionRelation.HoldsOwnMeans, IncomeSource.OwnProduct)]
  [InlineData(SocialClass.Journeymen, ProductionRelation.SellsLaborPower, IncomeSource.Wages)]
  [InlineData(SocialClass.Proletariat, ProductionRelation.SellsLaborPower, IncomeSource.Wages)]
  [InlineData(SocialClass.Bourgeoisie, ProductionRelation.OwnsCapital, IncomeSource.Profit)]
  [InlineData(SocialClass.Lumpenproletariat, ProductionRelation.OutsideProduction, IncomeSource.Plunder)]
  public void A_class_is_defined_by_its_relation_to_the_conditions_of_production(
    SocialClass socialClass, ProductionRelation relation, IncomeSource income)
  {
    var profile = socialClass.Profile();

    Assert.Equal(relation, profile.Relation);
    Assert.Equal(income, profile.Income);
  }

  [Fact]
  public void The_feudal_mode_bears_more_than_lords_and_serfs()
  {
    var feudal = ModeOfProduction.Feudal.Classes();

    // The Manifesto's own list: "feudal lords, vassals, guild-masters,
    // journeymen, apprentices, serfs" — with the Church beside them as the
    // age's greatest landowner, and the town's free peasants and traders.
    Assert.Contains(SocialClass.FeudalLords, feudal);
    Assert.Contains(SocialClass.Vassals, feudal);
    Assert.Contains(SocialClass.Clergy, feudal);
    Assert.Contains(SocialClass.Serfs, feudal);
    Assert.Contains(SocialClass.FreePeasants, feudal);
    Assert.Contains(SocialClass.GuildMasters, feudal);
    Assert.Contains(SocialClass.Journeymen, feudal);
    Assert.Contains(SocialClass.Apprentices, feudal);
    Assert.Contains(SocialClass.Merchants, feudal);
    Assert.Contains(SocialClass.Usurers, feudal);
  }

  [Fact]
  public void Merchants_and_usurers_are_older_than_the_capitalist_mode()
  {
    // Marx's antediluvian forms of capital: they precede the capitalist mode
    // and attach themselves to whatever mode they find.
    Assert.Contains(SocialClass.Merchants, ModeOfProduction.Slave.Classes());
    Assert.Contains(SocialClass.Merchants, ModeOfProduction.Feudal.Classes());
    Assert.Contains(SocialClass.Merchants, ModeOfProduction.Capitalist.Classes());
    Assert.Contains(SocialClass.Usurers, ModeOfProduction.Feudal.Classes());
  }

  [Fact]
  public void The_capitalist_mode_bears_the_three_great_classes()
  {
    // Capital III, ch. 52: owners of labour-power, owners of capital and
    // landowners, whose sources of income are wages, profit and ground-rent.
    var capitalist = ModeOfProduction.Capitalist.Classes();

    Assert.Contains(SocialClass.Proletariat, capitalist);
    Assert.Contains(SocialClass.Bourgeoisie, capitalist);
    Assert.Contains(SocialClass.Landowners, capitalist);
  }

  [Fact]
  public void Neither_end_of_history_bears_an_appropriating_class()
  {
    foreach (var mode in new[] { ModeOfProduction.PrimitiveCommunal, ModeOfProduction.Communist })
    {
      Assert.All(mode.Classes(), socialClass => Assert.False(socialClass.Profile().AppropriatesSurplus));
    }
  }

  [Fact]
  public void The_producing_classes_are_those_whose_labour_the_rest_live_on()
  {
    Assert.True(SocialClass.Slaves.Profile().IsDirectProducer);
    Assert.True(SocialClass.Serfs.Profile().IsDirectProducer);
    Assert.True(SocialClass.Proletariat.Profile().IsDirectProducer);
    Assert.True(SocialClass.FreePeasants.Profile().IsDirectProducer);
    Assert.True(SocialClass.AssociatedProducers.Profile().IsDirectProducer);

    Assert.False(SocialClass.FeudalLords.Profile().IsDirectProducer);
    Assert.False(SocialClass.Bourgeoisie.Profile().IsDirectProducer);
    Assert.False(SocialClass.Lumpenproletariat.Profile().IsDirectProducer);
  }

  [Fact]
  public void The_appropriating_classes_are_those_living_on_labour_they_did_not_perform()
  {
    Assert.True(SocialClass.SlaveOwners.Profile().AppropriatesSurplus);
    Assert.True(SocialClass.FeudalLords.Profile().AppropriatesSurplus);
    Assert.True(SocialClass.Clergy.Profile().AppropriatesSurplus);
    Assert.True(SocialClass.Bourgeoisie.Profile().AppropriatesSurplus);
    Assert.True(SocialClass.Merchants.Profile().AppropriatesSurplus);

    Assert.False(SocialClass.Serfs.Profile().AppropriatesSurplus);
    Assert.False(SocialClass.Proletariat.Profile().AppropriatesSurplus);
  }

  [Fact]
  public void Whether_the_bureaucracy_is_a_class_is_left_unsettled()
  {
    // Caste or new class is a question Marxists have never agreed on, so the
    // simulation declines to appropriate on its behalf.
    var bureaucracy = SocialClass.Bureaucracy.Profile();

    Assert.Equal(ProductionRelation.AdministersCommonProperty, bureaucracy.Relation);
    Assert.False(bureaucracy.AppropriatesSurplus);
    Assert.False(bureaucracy.IsDirectProducer);
  }

  [Fact]
  public void A_class_knows_which_modes_bear_it()
  {
    Assert.True(SocialClass.Serfs.Profile().IsNativeTo(ModeOfProduction.Feudal));
    Assert.False(SocialClass.Serfs.Profile().IsNativeTo(ModeOfProduction.Capitalist));
  }

  [Fact]
  public void A_profile_reads_as_its_relation_and_its_revenue()
  {
    Assert.Equal("Proletariat (SellsLaborPower, lives on Wages)", SocialClass.Proletariat.Profile().ToString());
  }

  [Fact]
  public void You_cannot_arm_those_you_own()
  {
    // Arming the chattel is what being chattel rules out. Rome came to it only
    // after Cannae, and the Confederacy only in 1865, when raising the question
    // was already the answer.
    Assert.False(SocialClass.Slaves.Profile().CanBeArmed);
  }

  [Fact]
  public void Every_other_class_can_be_put_under_arms()
  {
    var unarmable = Enum.GetValues<SocialClass>()
      .Where(socialClass => !socialClass.Profile().CanBeArmed)
      .ToList();

    Assert.Equal([SocialClass.Slaves], unarmable);
  }

  [Fact]
  public void The_classes_a_state_leans_on_are_the_ones_it_arms()
  {
    // The feudal levy, the mass conscript army, and the reactionary mob that
    // Bonaparte drew out of the lumpenproletariat: all of them armable.
    Assert.True(SocialClass.Serfs.Profile().CanBeArmed);
    Assert.True(SocialClass.Proletariat.Profile().CanBeArmed);
    Assert.True(SocialClass.FeudalLords.Profile().CanBeArmed);
    Assert.True(SocialClass.Lumpenproletariat.Profile().CanBeArmed);
  }

  #endregion
}
