using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Society;
using Surplus.Testing;

namespace Surplus.Domain.Tests.Simulation.Politics;

/// <summary>
/// What happens when a statute meets a class structure. A law never simply
/// deletes a class: it outlaws a relation, and everyone standing in it becomes
/// something else — decided by who still holds the land when it lands.
/// </summary>
public class EnactmentTests
{
  #region Methods

  [Fact]
  public void Abolition_where_the_planters_keep_the_land_yields_a_class_still_dependent_on_them()
  {
    // The American South after 1865: emancipation pronounced, the plantations
    // intact, and the freed delivered into sharecropping and debt peonage.
    var south = new ProvinceBuilder()
      .WithClasses(
        ClassPresence.Of(SocialClass.Slaves, 4_000),
        ClassPresence.Of(SocialClass.SlaveOwners, 100),
        ClassPresence.Of(SocialClass.Merchants, 50))
      .Build();

    south.Enforce(Law.AbolitionOfSlavery);

    Assert.Equal(0, south.HeadsOf(SocialClass.Slaves));
    Assert.Equal(4_000, south.HeadsOf(SocialClass.Freedmen));

    // And the planter, keeping his acres, is a landlord rather than a ruin.
    Assert.Equal(0, south.HeadsOf(SocialClass.SlaveOwners));
    Assert.Equal(100, south.HeadsOf(SocialClass.Landowners));
    Assert.Equal(4_150, south.Population);
  }

  [Fact]
  public void Abolition_where_the_masters_were_expropriated_yields_smallholders_instead()
  {
    // The same statute, the opposite result, because the class structure it
    // lands in is not the same one.
    var south = new ProvinceBuilder()
      .WithClasses(ClassPresence.Of(SocialClass.Slaves, 4_000))
      .Build();

    south.Enforce(Law.AbolitionOfSlavery);

    Assert.Equal(4_000, south.HeadsOf(SocialClass.Plebeians));
    Assert.Equal(0, south.HeadsOf(SocialClass.Freedmen));
  }

  [Fact]
  public void Emancipation_leaves_the_serfs_working_for_the_men_who_still_own_the_soil()
  {
    // Russia, 1861: personal freedom granted, redemption payments imposed, and
    // the lords left in possession of the land.
    var russia = new ProvinceBuilder()
      .WithClasses(
        ClassPresence.Of(SocialClass.Serfs, 9_000),
        ClassPresence.Of(SocialClass.FeudalLords, 200))
      .Build();

    russia.Enforce(Law.SerfEmancipation);

    Assert.Equal(9_000, russia.HeadsOf(SocialClass.AgriculturalProletariat));
    Assert.Equal(200, russia.HeadsOf(SocialClass.FeudalLords));
  }

  [Fact]
  public void Land_reform_first_makes_emancipation_mean_something_else_entirely()
  {
    var russia = new ProvinceBuilder()
      .WithClasses(
        ClassPresence.Of(SocialClass.Serfs, 9_000),
        ClassPresence.Of(SocialClass.FeudalLords, 200))
      .Build();

    russia.Enforce(Law.LandReform);
    russia.Enforce(Law.SerfEmancipation);

    // With the lords expropriated first, the freed get land rather than wages.
    Assert.Equal(9_000, russia.HeadsOf(SocialClass.Peasantry));
    Assert.Equal(0, russia.HeadsOf(SocialClass.AgriculturalProletariat));
  }

  [Fact]
  public void Enclosure_manufactures_a_proletariat_out_of_men_who_held_their_own_means()
  {
    var england = new ProvinceBuilder()
      .WithClasses(
        ClassPresence.Of(SocialClass.FreePeasants, 5_000),
        ClassPresence.Of(SocialClass.Landowners, 100))
      .Build();

    england.Enforce(Law.EnclosureActs);

    Assert.Equal(5_000, england.HeadsOf(SocialClass.Proletariat));
    Assert.Equal(0, england.HeadsOf(SocialClass.FreePeasants));
  }

  [Fact]
  public void A_statute_meeting_no_one_it_concerns_changes_nothing()
  {
    var province = new ProvinceBuilder().Build();
    var before = province.Composition;

    province.Enforce(Law.AbolitionOfSlavery);

    Assert.Equal(before, province.Composition);
  }

  [Fact]
  public void Enacting_carries_a_statute_into_every_province_at_once()
  {
    var north = new ProvinceBuilder()
      .WithName("Massachusetts")
      .WithClasses(
        ClassPresence.Of(SocialClass.Proletariat, 3_000),
        ClassPresence.Of(SocialClass.Bourgeoisie, 200))
      .Build();
    var south = new ProvinceBuilder()
      .WithName("Carolina")
      .WithClasses(
        ClassPresence.Of(SocialClass.Slaves, 4_000),
        ClassPresence.Of(SocialClass.SlaveOwners, 100))
      .Build();

    var unitedStates = new CountryBuilder().WithProvinces(north, south).Build();

    unitedStates.Enact(Law.AbolitionOfSlavery);

    Assert.True(unitedStates.Government.HasEnacted(Law.AbolitionOfSlavery));
    Assert.Equal(4_000, south.HeadsOf(SocialClass.Freedmen));
    Assert.Equal(3_000, north.HeadsOf(SocialClass.Proletariat));
  }

  [Fact]
  public void One_state_can_rest_on_two_modes_of_production_at_once()
  {
    // The 1836 United States, and the reason the mode sits on the province.
    var north = new ProvinceBuilder()
      .WithClasses(ClassPresence.Of(SocialClass.Proletariat, 3_000))
      .Build();
    var south = new ProvinceBuilder()
      .WithClasses(ClassPresence.Of(SocialClass.Slaves, 4_000))
      .Build();

    var unitedStates = new CountryBuilder().WithProvinces(north, south).Build();

    Assert.Equal(ModeOfProduction.Slave, unitedStates.DominantMode);
    Assert.Equal(7_000, unitedStates.Population);
  }

  [Fact]
  public void Repeal_does_not_lead_the_freed_back_into_the_relation_they_were_taken_out_of()
  {
    var south = new ProvinceBuilder()
      .WithClasses(ClassPresence.Of(SocialClass.Slaves, 4_000))
      .Build();
    var country = new CountryBuilder().WithProvinces(south).Build();

    country.Enact(Law.AbolitionOfSlavery);
    country.Repeal(Law.AbolitionOfSlavery);

    Assert.False(country.Government.HasEnacted(Law.AbolitionOfSlavery));
    Assert.Equal(0, south.HeadsOf(SocialClass.Slaves));
    Assert.Equal(4_000, south.HeadsOf(SocialClass.Plebeians));
  }

  [Fact]
  public void A_turn_to_fascism_leaves_the_base_exactly_where_it_was()
  {
    var province = new ProvinceBuilder()
      .WithClasses(
        ClassPresence.Of(SocialClass.Proletariat, 5_000),
        ClassPresence.Of(SocialClass.Bourgeoisie, 300))
      .Build();
    var italy = new CountryBuilder()
      .WithGovernment(new GovernmentBuilder().WithForm(GovernmentForm.Republic).Build())
      .WithProvinces(province)
      .Build();
    var before = province.Composition;

    italy.TakeForm(GovernmentForm.Dictatorship);
    italy.Adopt(Ideology.Fascism);

    Assert.Equal(GovernmentForm.Dictatorship, italy.Government.Form);
    Assert.Equal(Ideology.Fascism, italy.Government.Ideology);
    Assert.Equal(ModeOfProduction.Capitalist, italy.DominantMode);
    Assert.Equal(before, province.Composition);
  }

  [Fact]
  public void A_statute_already_on_the_books_cannot_be_enacted_twice()
  {
    var country = new CountryBuilder().Build();

    country.Enact(Law.LandReform);

    Assert.Throws<DomainException>(() => country.Enact(Law.LandReform));
    Assert.Throws<DomainException>(() => country.Repeal(Law.EnclosureActs));
  }

  [Fact]
  public void A_country_reads_as_its_creed_its_form_and_its_territory()
  {
    var country = new CountryBuilder()
      .WithGovernment(
        new GovernmentBuilder().WithForm(GovernmentForm.PartyState).WithIdeology(Ideology.MarxismLeninism).Build())
      .Build();

    Assert.Equal("France — MarxismLeninism PartyState, Ai (1 province)", country.ToString());
  }

  #endregion
}
