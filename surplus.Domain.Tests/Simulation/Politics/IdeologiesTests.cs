using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Tests.Simulation.Politics;

public class IdeologiesTests
{
  #region Methods

  [Fact]
  public void Every_ideology_is_registered()
  {
    foreach (var ideology in Enum.GetValues<Ideology>())
    {
      Assert.Equal(ideology, ideology.Profile().Ideology);
    }
  }

  [Fact]
  public void An_ideology_serves_a_class_whatever_it_says_of_itself()
  {
    Assert.Equal(SocialClass.FeudalLords, Ideology.Traditionalism.Profile().ServesClass);
    Assert.Equal(SocialClass.Bourgeoisie, Ideology.Liberalism.Profile().ServesClass);
    Assert.Equal(SocialClass.Proletariat, Ideology.MarxismLeninism.Profile().ServesClass);
  }

  [Fact]
  public void Maoism_rests_on_the_peasantry_rather_than_the_industrial_workers()
  {
    // The distinguishing claim of the Chinese road, and not a slight: the
    // revolution is carried by the countryside surrounding the cities.
    Assert.Equal(SocialClass.Peasantry, Ideology.Maoism.Profile().ServesClass);
  }

  [Fact]
  public void Fascism_drives_toward_no_mode_of_production_of_its_own()
  {
    // It is capital's rule by other means: the base is left exactly where it
    // was, and only the manner of ruling over it changes.
    var fascism = Ideology.Fascism.Profile();

    Assert.Equal(ModeOfProduction.Capitalist, fascism.DrivesToward);
    Assert.Equal(SocialClass.Financiers, fascism.ServesClass);
    Assert.False(fascism.RecognisesClassStruggle);
  }

  [Fact]
  public void Reform_administers_the_base_it_means_to_outgrow()
  {
    Assert.Equal(ModeOfProduction.Capitalist, Ideology.SocialDemocracy.Profile().DrivesToward);
    Assert.True(Ideology.SocialDemocracy.Profile().RecognisesClassStruggle);
  }

  [Fact]
  public void The_tendencies_disagree_about_the_bureaucracy_and_none_is_privileged()
  {
    // Four irreconcilable readings, each recorded as the position of the
    // tendency that holds it. The engine rules on none of them.
    Assert.Equal(BureaucracyDoctrine.OrganOfTheWorkersState, Ideology.MarxismLeninism.Profile().BureaucracyDoctrine);
    Assert.Equal(BureaucracyDoctrine.NewBourgeoisie, Ideology.Maoism.Profile().BureaucracyDoctrine);
    Assert.Equal(BureaucracyDoctrine.ParasiticCaste, Ideology.Trotskyism.Profile().BureaucracyDoctrine);
    Assert.Equal(BureaucracyDoctrine.StateCapitalism, Ideology.CouncilCommunism.Profile().BureaucracyDoctrine);
  }

  [Fact]
  public void No_single_reading_of_the_bureaucracy_is_the_registers_default()
  {
    // If one doctrine were baked in as the truth, every socialist tendency
    // would carry it. They do not, and that is the point.
    var socialist = Enum.GetValues<Ideology>()
      .Select(ideology => ideology.Profile())
      .Where(profile => profile.BureaucracyDoctrine is not BureaucracyDoctrine.NotAtIssue)
      .ToList();

    Assert.Equal(4, socialist.Select(profile => profile.BureaucracyDoctrine).Distinct().Count());
  }

  [Fact]
  public void The_tendencies_holding_a_doctrine_can_be_listed()
  {
    Assert.Equal([Ideology.MarxismLeninism], Ideologies.Holding(BureaucracyDoctrine.OrganOfTheWorkersState));
    Assert.Contains(Ideology.Anarchism, Ideologies.Holding(BureaucracyDoctrine.StateCapitalism));
    Assert.Contains(Ideology.Liberalism, Ideologies.Holding(BureaucracyDoctrine.NotAtIssue));
  }

  [Fact]
  public void The_tendencies_that_act_consciously_on_the_laws_of_motion_move_a_class_structure_fastest()
  {
    var strongest = Enum.GetValues<Ideology>()
      .Select(ideology => ideology.Profile())
      .GroupBy(profile => profile.MobilisingPower)
      .OrderByDescending(group => group.Key)
      .First()
      .Select(profile => profile.Ideology)
      .ToList();

    Assert.Equal([Ideology.MarxismLeninism, Ideology.Maoism], strongest);
  }

  [Fact]
  public void Letting_a_market_do_the_work_moves_it_slowest()
  {
    Assert.True(
      Ideology.MarxismLeninism.Profile().MobilisingPower > Ideology.Liberalism.Profile().MobilisingPower);
    Assert.True(
      Ideology.Maoism.Profile().MobilisingPower > Ideology.SocialDemocracy.Profile().MobilisingPower);
  }

  [Fact]
  public void A_profile_reads_as_the_class_it_serves_and_the_mode_it_drives_toward()
  {
    Assert.Equal(
      "Liberalism (serves Bourgeoisie, drives toward Capitalist)",
      Ideology.Liberalism.Profile().ToString());
  }

  #endregion
}
