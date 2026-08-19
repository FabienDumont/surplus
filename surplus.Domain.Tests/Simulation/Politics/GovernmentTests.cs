using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Society;
using Surplus.Testing;

namespace Surplus.Domain.Tests.Simulation.Politics;

public class GovernmentTests
{
  #region Methods

  [Fact]
  public void A_government_is_a_form_an_ideology_and_the_statutes_in_force()
  {
    var government = Government.Of(GovernmentForm.Republic, Ideology.Liberalism, Law.AbolitionOfSlavery);

    Assert.Equal(GovernmentForm.Republic, government.Form);
    Assert.Equal(Ideology.Liberalism, government.Ideology);
    Assert.Equal([Law.AbolitionOfSlavery], government.Laws);
    Assert.True(government.HasEnacted(Law.AbolitionOfSlavery));
    Assert.False(government.HasEnacted(Law.LandReform));
  }

  [Fact]
  public void A_republic_can_rest_on_slavery()
  {
    // The 1836 United States: the freest constitution in the world, and a
    // slave mode of production in half of it.
    var unitedStates = new GovernmentBuilder()
      .WithForm(GovernmentForm.Republic)
      .WithIdeology(Ideology.Liberalism)
      .Build();

    Assert.False(unitedStates.Forbids(ProductionRelation.IsOwned));
  }

  [Fact]
  public void The_same_creed_can_be_worn_by_one_ruler_or_by_a_board_of_them()
  {
    var dictatorship = new GovernmentBuilder()
      .WithForm(GovernmentForm.Dictatorship)
      .WithIdeology(Ideology.Fascism)
      .Build();
    var oligarchy = new GovernmentBuilder()
      .WithForm(GovernmentForm.Oligarchy)
      .WithIdeology(Ideology.Fascism)
      .Build();

    Assert.Equal(dictatorship.Ideology, oligarchy.Ideology);
    Assert.NotEqual(dictatorship.Form, oligarchy.Form);
    Assert.NotEqual(dictatorship, oligarchy);
  }

  [Fact]
  public void A_statute_cannot_be_on_the_books_twice()
  {
    Assert.Throws<DomainException>(
      () => Government.Of(GovernmentForm.Republic, Ideology.Liberalism,
        Law.LandReform, Law.LandReform));

    Assert.Throws<DomainException>(
      () => new GovernmentBuilder().WithLaws(Law.LandReform).Build().Enacting(Law.LandReform));
  }

  [Fact]
  public void A_statute_not_on_the_books_cannot_be_struck_off()
  {
    Assert.Throws<DomainException>(() => new GovernmentBuilder().Build().Repealing(Law.LandReform));
  }

  [Fact]
  public void Enacting_and_repealing_leave_the_constitution_they_were_read_from_untouched()
  {
    var before = new GovernmentBuilder().Build();

    var after = before.Enacting(Law.AbolitionOfSlavery);

    Assert.False(before.HasEnacted(Law.AbolitionOfSlavery));
    Assert.True(after.HasEnacted(Law.AbolitionOfSlavery));
    Assert.False(after.Repealing(Law.AbolitionOfSlavery).HasEnacted(Law.AbolitionOfSlavery));
  }

  [Fact]
  public void A_government_knows_which_relations_its_statutes_forbid()
  {
    var government = new GovernmentBuilder().WithLaws(Law.AbolitionOfSlavery).Build();

    Assert.True(government.Forbids(ProductionRelation.IsOwned));
    Assert.True(government.Forbids(ProductionRelation.OwnsProducers));
    Assert.False(government.Forbids(ProductionRelation.SellsLaborPower));
  }

  [Fact]
  public void The_state_can_be_reconstituted_without_changing_what_it_avows()
  {
    var monarchy = new GovernmentBuilder().WithForm(GovernmentForm.ConstitutionalMonarchy).Build();

    var republic = monarchy.TakingForm(GovernmentForm.Republic);

    Assert.Equal(GovernmentForm.Republic, republic.Form);
    Assert.Equal(monarchy.Ideology, republic.Ideology);
    Assert.Throws<DomainException>(() => republic.TakingForm(GovernmentForm.Republic));
  }

  [Fact]
  public void The_state_can_change_what_it_avows_without_being_reconstituted()
  {
    var liberal = new GovernmentBuilder().WithIdeology(Ideology.Liberalism).Build();

    var fascist = liberal.Adopting(Ideology.Fascism);

    Assert.Equal(Ideology.Fascism, fascist.Ideology);
    Assert.Equal(liberal.Form, fascist.Form);
    Assert.Throws<DomainException>(() => fascist.Adopting(Ideology.Fascism));
  }

  [Fact]
  public void A_government_carries_the_doctrine_of_the_ideology_it_avows()
  {
    var partyState = new GovernmentBuilder().WithIdeology(Ideology.Maoism).Build();

    Assert.Equal(BureaucracyDoctrine.NewBourgeoisie, partyState.Doctrine.BureaucracyDoctrine);
  }

  [Fact]
  public void Governments_alike_in_form_ideology_and_statutes_are_equal()
  {
    var first = Government.Of(GovernmentForm.Republic, Ideology.Liberalism, Law.LandReform, Law.EnclosureActs);
    var second = Government.Of(GovernmentForm.Republic, Ideology.Liberalism, Law.EnclosureActs, Law.LandReform);

    Assert.Equal(first, second);
    Assert.Equal(first.GetHashCode(), second.GetHashCode());
    Assert.NotEqual(first, first.Repealing(Law.LandReform));
    Assert.False(first.Equals(null));
  }

  [Fact]
  public void A_government_reads_as_its_creed_its_form_and_its_statute_book()
  {
    Assert.Equal(
      "Fascism Dictatorship (0 laws in force)",
      new GovernmentBuilder().WithForm(GovernmentForm.Dictatorship).WithIdeology(Ideology.Fascism).Build()
        .ToString());
  }

  #endregion
}
