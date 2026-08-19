using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Politics;
using Surplus.Testing;

namespace Surplus.Domain.Tests.Simulation.Countries;

public class CountryTests
{
  #region Tests

  [Fact]
  public void A_country_already_played_cannot_be_given_to_the_player_again()
  {
    Assert.Throws<DomainException>(new CountryBuilder().Played().Build().GiveToPlayer);
  }

  [Fact]
  public void A_country_already_run_by_the_ai_cannot_be_given_to_it_again()
  {
    Assert.Throws<DomainException>(new CountryBuilder().Build().GiveToAi);
  }

  [Fact]
  public void A_country_cannot_cede_a_province_it_does_not_govern()
  {
    var france = new CountryBuilder().Build();

    Assert.Throws<DomainException>(() => france.Cede(ProvinceId.New()));
  }

  [Fact]
  public void A_country_governs_the_provinces_it_holds_and_no_others()
  {
    var normandie = new ProvinceBuilder().WithName("Normandie").Build();
    var france = new CountryBuilder().WithProvinces(normandie).Build();

    Assert.True(france.Governs(normandie.Id));
    Assert.False(france.Governs(ProvinceId.New()));
  }

  [Fact]
  public void A_country_is_founded_on_the_territory_it_holds_from_the_start()
  {
    var capital = new ProvinceBuilder().WithName("Île-de-France").Build();

    var france = Country.Found("  France  ", CountryControl.Ai, new GovernmentBuilder().Build(), capital);

    Assert.Equal("France", france.Name);
    Assert.Equal(CountryControl.Ai, france.Control);
    Assert.Equal([capital], france.Provinces);
  }

  [Fact]
  public void A_country_is_played_either_by_the_player_or_by_the_ai()
  {
    Assert.True(new CountryBuilder().Played().Build().IsPlayed);
    Assert.False(new CountryBuilder().Build().IsPlayed);
  }

  [Fact]
  public void A_country_must_have_a_name()
  {
    Assert.Throws<DomainException>(() => Country.Found("   ", CountryControl.Ai, new GovernmentBuilder().Build(), new ProvinceBuilder().Build()));
  }

  [Fact]
  public void A_country_reads_as_its_name_its_control_and_the_size_of_its_territory()
  {
    var one = new CountryBuilder().WithProvinces(new ProvinceBuilder().Build()).Build();
    var two = new CountryBuilder().Played().WithProvinces(new ProvinceBuilder().Build(), new ProvinceBuilder().Build())
      .Build();

    Assert.Equal("France — Liberalism Republic, Ai (1 province)", one.ToString());
    Assert.Equal("France — Liberalism Republic, Player (2 provinces)", two.ToString());
  }

  [Fact]
  public void A_country_without_territory_is_no_country()
  {
    var capital = new ProvinceBuilder().Build();
    var france = new CountryBuilder().WithProvinces(capital).Build();

    Assert.Throws<DomainException>(() => france.Cede(capital.Id));
    Assert.Equal([capital], france.Provinces);
  }

  [Fact]
  public void A_province_cannot_be_annexed_twice_by_the_same_country()
  {
    var normandie = new ProvinceBuilder().WithName("Normandie").Build();
    var france = new CountryBuilder().WithProvinces(normandie).Build();

    Assert.Throws<DomainException>(() => france.Annex(normandie));
  }

  [Fact]
  public void A_saved_country_is_reloaded_exactly_as_it_was_left()
  {
    var id = CountryId.New();
    var normandie = new ProvinceBuilder().WithName("Normandie").Build();
    var bretagne = new ProvinceBuilder().WithName("Bretagne").Build();

    var france = new CountryBuilder().WithId(id).WithName("France").Played().WithProvinces(normandie, bretagne).Build();

    Assert.Equal(id, france.Id);
    Assert.Equal("France", france.Name);
    Assert.Equal(CountryControl.Player, france.Control);
    Assert.Equal([normandie, bretagne], france.Provinces);
  }

  [Fact]
  public void Annexing_brings_a_province_under_the_countrys_rule()
  {
    var normandie = new ProvinceBuilder().WithName("Normandie").Build();
    var bretagne = new ProvinceBuilder().WithName("Bretagne").Build();
    var france = new CountryBuilder().WithProvinces(normandie).Build();

    france.Annex(bretagne);

    Assert.Equal([normandie, bretagne], france.Provinces);
  }

  [Fact]
  public void Ceding_gives_a_province_up()
  {
    var normandie = new ProvinceBuilder().WithName("Normandie").Build();
    var bretagne = new ProvinceBuilder().WithName("Bretagne").Build();
    var france = new CountryBuilder().WithProvinces(normandie, bretagne).Build();

    var ceded = france.Cede(bretagne.Id);

    Assert.Equal(bretagne, ceded);
    Assert.Equal([normandie], france.Provinces);
  }

  [Fact]
  public void Each_country_has_its_own_identity()
  {
    var first = Country.Found("France", CountryControl.Ai, new GovernmentBuilder().Build(), new ProvinceBuilder().Build());
    var second = Country.Found("France", CountryControl.Ai, new GovernmentBuilder().Build(), new ProvinceBuilder().Build());

    Assert.NotEqual(first.Id, second.Id);
  }

  [Fact]
  public void The_player_can_take_a_country_over_and_hand_it_back()
  {
    var france = new CountryBuilder().Build();

    france.GiveToPlayer();
    Assert.Equal(CountryControl.Player, france.Control);

    france.GiveToAi();
    Assert.Equal(CountryControl.Ai, france.Control);
  }

  #endregion
}
