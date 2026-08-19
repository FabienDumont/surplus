using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Society;
using Surplus.Testing;

namespace Surplus.Domain.Tests.Simulation.Society;

public class ClassCompositionTests
{
  #region Methods

  [Fact]
  public void A_class_present_must_have_someone_standing_in_it()
  {
    Assert.Throws<DomainException>(() => ClassPresence.Of(SocialClass.Serfs, 0));
    Assert.Throws<DomainException>(() => ClassPresence.Of(SocialClass.Serfs, -1));
  }

  [Fact]
  public void A_presence_reads_as_its_numbers_and_its_class()
  {
    Assert.Equal("900 Serfs", ClassPresence.Of(SocialClass.Serfs, 900).ToString());
  }

  [Fact]
  public void A_presence_knows_what_its_class_is_economically()
  {
    Assert.Equal(
      ProductionRelation.BoundToTheLand,
      ClassPresence.Of(SocialClass.Serfs, 900).Profile.Relation);
  }

  [Fact]
  public void A_class_cannot_be_counted_twice_in_the_same_composition()
  {
    Assert.Throws<DomainException>(
      () => ClassComposition.Of(
        ClassPresence.Of(SocialClass.Serfs, 900),
        ClassPresence.Of(SocialClass.Serfs, 100)));
  }

  [Fact]
  public void A_composition_counts_its_population()
  {
    var composition = new ClassCompositionBuilder().Build();

    Assert.Equal(915, composition.Population);
    Assert.Equal(900, composition.HeadsOf(SocialClass.Serfs));
    Assert.Equal(0, composition.HeadsOf(SocialClass.Proletariat));
    Assert.True(composition.Holds(SocialClass.Serfs));
    Assert.False(composition.Holds(SocialClass.Proletariat));
  }

  [Fact]
  public void An_empty_composition_holds_no_one()
  {
    Assert.Equal(0, ClassComposition.Empty.Population);
    Assert.Equal("no one", ClassComposition.Empty.ToString());
  }

  [Fact]
  public void A_composition_separates_those_who_produce_from_those_who_appropriate()
  {
    var composition = new ClassCompositionBuilder().Build();

    Assert.Equal(900, composition.DirectProducers);
    Assert.Equal(15, composition.Appropriators);
  }

  [Fact]
  public void A_society_is_antagonistic_when_a_class_lives_on_anothers_surplus_labour()
  {
    Assert.True(new ClassCompositionBuilder().Build().IsAntagonistic);
    Assert.False(new ClassCompositionBuilder().Build().IsClassless);
  }

  [Fact]
  public void A_society_of_associated_producers_is_classless()
  {
    var communist = new ClassCompositionBuilder()
      .Of(ClassPresence.Of(SocialClass.AssociatedProducers, 1_000))
      .Build();

    Assert.True(communist.IsClassless);
    Assert.False(communist.IsAntagonistic);
  }

  [Fact]
  public void Classes_the_prevailing_mode_does_not_bear_are_survivals()
  {
    var prussia = new ClassCompositionBuilder()
      .Of(
        ClassPresence.Of(SocialClass.Proletariat, 500),
        ClassPresence.Of(SocialClass.Bourgeoisie, 20))
      .With(SocialClass.Serfs, 300)
      .Build();

    // The dead generations weighing on the living: a serf under capitalism.
    Assert.Equal([SocialClass.Serfs], prussia.SurvivalsUnder(ModeOfProduction.Capitalist));
    Assert.Empty(prussia.SurvivalsUnder(ModeOfProduction.Capitalist).Except([SocialClass.Serfs]));
  }

  [Fact]
  public void A_class_grows_by_someone_or_not_at_all()
  {
    var composition = new ClassCompositionBuilder().Build();

    Assert.Equal(1_000, composition.Grown(SocialClass.Serfs, 100).HeadsOf(SocialClass.Serfs));
    Assert.Equal(50, composition.Grown(SocialClass.Merchants, 50).HeadsOf(SocialClass.Merchants));
    Assert.Throws<DomainException>(() => composition.Grown(SocialClass.Serfs, 0));
  }

  [Fact]
  public void A_class_declines_by_someone_or_not_at_all()
  {
    var composition = new ClassCompositionBuilder().Build();

    Assert.Equal(800, composition.Declined(SocialClass.Serfs, 100).HeadsOf(SocialClass.Serfs));
    Assert.Throws<DomainException>(() => composition.Declined(SocialClass.Serfs, 0));
  }

  [Fact]
  public void A_class_cannot_lose_more_than_it_has()
  {
    Assert.Throws<DomainException>(() => new ClassCompositionBuilder().Build().Declined(SocialClass.Serfs, 901));
  }

  [Fact]
  public void A_class_reduced_to_no_one_leaves_the_composition()
  {
    var without = new ClassCompositionBuilder().Build().Declined(SocialClass.Clergy, 10);

    Assert.False(without.Holds(SocialClass.Clergy));
    Assert.Equal(2, without.Presences.Count);
  }

  [Fact]
  public void Primitive_accumulation_is_one_class_transformed_into_another()
  {
    // Peasants expropriated from the soil become free in the double sense:
    // free of feudal ties, and free of any means of production.
    var before = new ClassCompositionBuilder()
      .Of(ClassPresence.Of(SocialClass.FreePeasants, 1_000))
      .Build();

    var after = before.Transformed(SocialClass.FreePeasants, SocialClass.Proletariat, 400);

    Assert.Equal(600, after.HeadsOf(SocialClass.FreePeasants));
    Assert.Equal(400, after.HeadsOf(SocialClass.Proletariat));
    Assert.Equal(before.Population, after.Population);
  }

  [Fact]
  public void A_class_cannot_be_transformed_into_itself()
  {
    Assert.Throws<DomainException>(
      () => new ClassCompositionBuilder().Build().Transformed(SocialClass.Serfs, SocialClass.Serfs, 10));
  }

  [Fact]
  public void Transforming_leaves_the_composition_it_was_read_from_untouched()
  {
    var before = new ClassCompositionBuilder().Build();

    before.Transformed(SocialClass.Serfs, SocialClass.Proletariat, 100);

    Assert.Equal(900, before.HeadsOf(SocialClass.Serfs));
  }

  [Fact]
  public void Compositions_holding_the_same_classes_in_the_same_numbers_are_equal()
  {
    var lordFirst = ClassComposition.Of(
      ClassPresence.Of(SocialClass.FeudalLords, 5),
      ClassPresence.Of(SocialClass.Serfs, 900));
    var serfsFirst = ClassComposition.Of(
      ClassPresence.Of(SocialClass.Serfs, 900),
      ClassPresence.Of(SocialClass.FeudalLords, 5));

    Assert.Equal(lordFirst, serfsFirst);
    Assert.Equal(lordFirst.GetHashCode(), serfsFirst.GetHashCode());
    Assert.NotEqual(lordFirst, ClassComposition.Empty);
    Assert.NotEqual(lordFirst, serfsFirst.Grown(SocialClass.Serfs, 1));
    Assert.False(lordFirst.Equals(null));
  }

  [Fact]
  public void A_composition_reads_as_its_classes_largest_first()
  {
    Assert.Equal("900 Serfs, 10 Clergy, 5 FeudalLords", new ClassCompositionBuilder().Build().ToString());
  }

  [Fact]
  public void The_prevailing_mode_is_read_off_the_classes_present()
  {
    Assert.Equal(ModeOfProduction.Feudal, new ClassCompositionBuilder().Build().PrevailingMode);

    Assert.Equal(
      ModeOfProduction.Slave,
      ClassComposition.Of(ClassPresence.Of(SocialClass.Slaves, 4_000)).PrevailingMode);

    Assert.Equal(
      ModeOfProduction.Communist,
      ClassComposition.Of(ClassPresence.Of(SocialClass.AssociatedProducers, 1_000)).PrevailingMode);
  }

  [Fact]
  public void A_structure_with_no_one_in_it_prevails_under_no_ones_ownership()
  {
    Assert.Equal(ModeOfProduction.PrimitiveCommunal, ClassComposition.Empty.PrevailingMode);
  }

  [Fact]
  public void Classes_at_home_in_several_epochs_count_as_the_weak_evidence_they_are()
  {
    // Merchants sit in three modes at once, so a hundred of them weigh a third
    // as much for each — never enough to outvote a class that belongs to one.
    var composition = ClassComposition.Of(
      ClassPresence.Of(SocialClass.Merchants, 120),
      ClassPresence.Of(SocialClass.Serfs, 60));

    Assert.Equal(ModeOfProduction.Feudal, composition.PrevailingMode);
  }

  [Fact]
  public void A_structure_that_does_not_prove_it_has_moved_on_has_not_moved_on()
  {
    // The proletariat is at home under capitalism and under a workers' state
    // alike, so a province of nothing but wage-labourers is read as the earlier
    // of the two. Socialism has to be shown, not assumed.
    var composition = ClassComposition.Of(ClassPresence.Of(SocialClass.Proletariat, 3_000));

    Assert.Equal(ModeOfProduction.Capitalist, composition.PrevailingMode);
  }

  [Fact]
  public void A_bureaucracy_beside_the_workers_is_what_distinguishes_a_workers_state()
  {
    var composition = ClassComposition.Of(
      ClassPresence.Of(SocialClass.Proletariat, 3_000),
      ClassPresence.Of(SocialClass.Bureaucracy, 400));

    Assert.Equal(ModeOfProduction.Socialist, composition.PrevailingMode);
  }

  [Fact]
  public void A_feudal_structure_can_arm_very_nearly_everyone_in_it()
  {
    Assert.Equal(915, new ClassCompositionBuilder().Build().Armable);
  }

  [Fact]
  public void A_slave_structure_can_arm_almost_no_one()
  {
    // Four thousand people, and a hundred of them may be given a musket. The
    // mode of production settles the size of the army without any rule saying so.
    var plantation = ClassComposition.Of(
      ClassPresence.Of(SocialClass.Slaves, 4_000),
      ClassPresence.Of(SocialClass.SlaveOwners, 100));

    Assert.Equal(4_100, plantation.Population);
    Assert.Equal(100, plantation.Armable);
  }

  [Fact]
  public void Emancipation_is_also_a_recruiting_measure()
  {
    // The same people, no longer owned, are now the reserve the state can call on.
    var before = ClassComposition.Of(ClassPresence.Of(SocialClass.Slaves, 4_000));

    var after = before.Transformed(SocialClass.Slaves, SocialClass.Freedmen, 4_000);

    Assert.Equal(0, before.Armable);
    Assert.Equal(4_000, after.Armable);
  }

  [Fact]
  public void A_structure_with_no_one_in_it_can_arm_no_one()
  {
    Assert.Equal(0, ClassComposition.Empty.Armable);
  }

  #endregion
}
