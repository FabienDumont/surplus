using Surplus.Application.Saving;
using Surplus.Application.Scenarios;
using Surplus.Application.Tests.Reporting;
using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Circulation;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Society;
using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Warfare;
using Surplus.Testing;

namespace Surplus.Application.Tests.Saving;

public class SaveFileTests
{
  #region Tests

  [Fact]
  public void A_saved_session_is_read_back_exactly_as_it_was_left()
  {
    var session = Begun();
    session.Advance(45);

    var written = SaveFile.Write(session);

    Assert.Equal(written, SaveFile.Write(SaveFile.Read(written)));
  }

  [Fact]
  public void A_session_read_back_goes_on_exactly_as_the_original_would_have()
  {
    var original = Begun();
    original.Advance(45);
    var copy = SaveFile.Read(SaveFile.Write(original));

    original.Advance(400);
    copy.Advance(400);

    // This is also the determinism test for the whole simulation.
    Assert.Equal(SaveFile.Write(original), SaveFile.Write(copy));
  }

  [Fact]
  public void A_saved_session_keeps_its_date_and_its_name()
  {
    var session = Begun();
    session.Advance(40);

    var read = SaveFile.Read(SaveFile.Write(session));

    Assert.Equal(GameDate.Of(1836, 2, 10), read.Game.CurrentDate);
    Assert.Equal("1836", read.Game.Name);
    Assert.Equal(session.Game.Id, read.Game.Id);
    Assert.Equal(GameStatus.Paused, read.Game.Status);
  }

  [Fact]
  public void A_war_in_progress_survives_the_save()
  {
    var session = Begun();
    var war = session.DeclareWarOn(Named(session, "Russia").Id);
    session.Advance(70);

    var read = SaveFile.Read(SaveFile.Write(session)).World.Wars.Single();

    Assert.Equal(war.Id, read.Id);
    Assert.Equal(war.Momentum, read.Momentum);
    Assert.False(read.IsOver);
  }

  [Theory]
  [InlineData(PeaceTerms.White)]
  [InlineData(PeaceTerms.Cession)]
  [InlineData(PeaceTerms.Annexation)]
  public void A_concluded_war_survives_the_save_with_its_peace(PeaceTerms terms)
  {
    var prussia = new CountryBuilder().WithName("Prussia").Build();
    var france = new CountryBuilder().WithName("France")
      .WithProvinces(new ProvinceBuilder().WithName("Alsace").Build(), new ProvinceBuilder().WithName("Paris").Build()).Build();
    var peace = terms switch
    {
      PeaceTerms.White => Peace.White(),
      PeaceTerms.Cession => Peace.Ceding(france.Provinces[0].Id),
      _ => Peace.Annexation()
    };
    var war = new WarBuilder().WithAggressor(prussia.Id).WithDefender(france.Id).Overwhelming().WithPeace(peace).Build();
    var session = Session.Begin(new Scenario("1870", GameDate.Of(1870, 7, 19),
      new WorldBuilder().WithCountries(prussia, france).WithWars(war).Build()));

    var read = SaveFile.Read(SaveFile.Write(session)).World.Wars.Single();

    Assert.Equal(peace, read.Peace);
    Assert.Equal(100, read.Momentum);
  }

  [Fact]
  public void A_declaration_waiting_for_the_next_day_survives_the_save()
  {
    var session = Begun();
    session.DeclareWarOn(Named(session, "Russia").Id);

    var read = SaveFile.Read(SaveFile.Write(session));

    Assert.Equal([new WarWasDeclared("United Kingdom", "Russia")], read.World.Pending);
    Assert.Equal(new WarWasDeclared("United Kingdom", "Russia"), read.Advance(1)[0].Occurrences[0]);
  }

  [Fact]
  public void A_treaty_waiting_for_the_next_day_survives_the_save()
  {
    var session = Begun();
    var war = session.DeclareWarOn(Named(session, "Russia").Id);
    session.Advance(1);
    session.MakePeace(war.Id, Peace.White());

    var read = SaveFile.Read(SaveFile.Write(session));

    Assert.Equal([new PeaceWasMade("United Kingdom", "Russia", "nothing changes hands")], read.World.Pending);
  }

  [Fact]
  public void A_reequipped_branch_survives_the_save()
  {
    var session = Begun();
    var kent = session.World.Played!.Provinces.Single(province => province.Name == "Kent");
    var corn = session.World.Register.Commodities.Single(commodity => commodity.Name == "Corn").Id;
    session.Reequip(kent.Id, corn, "Peasant tillage");

    var read = SaveFile.Read(SaveFile.Write(session));

    var branch = read.World.Played!.Provinces.Single(province => province.Name == "Kent").Branches
      .Single(candidate => candidate.Produces == corn);
    Assert.Equal(session.World.Technique("Peasant tillage").OutputPerHour, branch.OutputPerHour);
    Assert.Equal(ProductiveForm.Handicraft, branch.Form);
  }

  [Fact]
  public void What_a_country_knows_and_is_finding_out_survives_the_save()
  {
    var session = Begun();
    session.Advance(100);
    var russia = Named(session, "Russia");

    var read = Named(SaveFile.Read(SaveFile.Write(session)), "Russia");

    Assert.Equal(russia.Knowledge, read.Knowledge);
    Assert.True(read.Knowledge.Spent("Large farming").CompareTo(LaborTime.None) > 0);
  }

  [Fact]
  public void Each_provinces_store_and_the_borders_survive_the_save()
  {
    var session = Begun();
    session.Advance(30);

    var read = SaveFile.Read(SaveFile.Write(session));

    Assert.Equal(session.World.Borders, read.World.Borders);
    Assert.Equal(session.World.Haulage, read.World.Haulage);
    Assert.Equal(
      session.World.Countries.SelectMany(country => country.Provinces).Select(province => province.Store),
      read.World.Countries.SelectMany(country => country.Provinces).Select(province => province.Store));
  }

  [Fact]
  public void A_governments_poor_relief_survives_the_save()
  {
    var session = Begun();
    session.WithdrawRelief();

    var read = SaveFile.Read(SaveFile.Write(session));

    Assert.False(read.World.Played!.Government.RelievesThePoor);
    Assert.True(Named(SaveFile.Read(SaveFile.Write(Begun())), "United Kingdom").Government.RelievesThePoor);
  }

  [Fact]
  public void An_annexed_country_stays_gone_after_the_save()
  {
    var prussia = new CountryBuilder().WithName("Prussia").Build();
    var france = new CountryBuilder().WithName("France").Build();
    var war = new WarBuilder().WithAggressor(prussia.Id).WithDefender(france.Id).Overwhelming().Build();
    var world = new WorldBuilder().WithCountries(prussia, france).WithWars(war).Build();
    world.MakePeace(war.Id, prussia.Id, Peace.Annexation());

    var read = SaveFile.Read(SaveFile.Write(Session.Begin(new Scenario("1870", GameDate.Of(1870, 7, 19), world))));

    Assert.Equal(["Prussia"], read.World.Countries.Select(country => country.Name));
    Assert.Equal(Peace.Annexation(), read.World.Wars.Single().Peace);
  }

  [Fact]
  public void A_session_in_which_no_one_is_played_survives_the_save()
  {
    var session = Session.Begin(new Scenario("1836", GameDate.Of(1836, 1, 1), new WorldBuilder().Build()));

    var read = SaveFile.Read(SaveFile.Write(session));

    Assert.Null(read.World.Played);
    Assert.Single(read.World.Countries);
  }

  [Fact]
  public void The_same_state_is_always_written_the_same_way()
  {
    var session = Begun();
    session.Advance(10);

    Assert.Equal(SaveFile.Write(session), SaveFile.Write(session));
  }

  [Theory]
  [InlineData("this is not json")]
  [InlineData("")]
  [InlineData("{\"Name\": \"1836\"}")]
  [InlineData("null")]
  public void What_is_not_a_save_file_cannot_be_read(string json)
  {
    var refusal = Assert.Throws<DomainException>(() => SaveFile.Read(json));

    Assert.Equal("That is not a save file this game can read.", refusal.Message);
  }

  [Fact]
  public void A_save_file_of_an_unknown_version_cannot_be_read()
  {
    var written = SaveFile.Write(Begun()).Replace("\"Version\": 1", "\"Version\": 2");

    var refusal = Assert.Throws<DomainException>(() => SaveFile.Read(written));

    Assert.Equal("That save file is of version 2, and this game reads only version 1.", refusal.Message);
  }

  [Fact]
  public void A_save_file_whose_contents_the_domain_refuses_cannot_be_read()
  {
    var written = SaveFile.Write(Begun()).Replace("\"Name\": \"United Kingdom\"", "\"Name\": \"\"").Replace("\"Unit\": \"t\"", "\"Unit\": \" \"");

    var refusal = Assert.Throws<DomainException>(() => SaveFile.Read(written));

    Assert.StartsWith("That save file holds something this game cannot rebuild: ", refusal.Message);
  }

  [Fact]
  public void An_occurrence_of_a_kind_unknown_to_the_game_cannot_be_read()
  {
    var session = Begun();
    session.DeclareWarOn(Named(session, "Russia").Id);
    var written = SaveFile.Write(session).Replace("\"Kind\": \"WarWasDeclared\"", "\"Kind\": \"Comet\"");

    Assert.Throws<DomainException>(() => SaveFile.Read(written));
  }

  [Fact]
  public void An_occurrence_that_is_never_left_waiting_is_refused_rather_than_lost()
  {
    var world = World.Load(
      CommodityRegister.Of(), [], Subsistence.Load([]), [new CountryBuilder().Build()], [], Haulage.None, [],
      [new CometSighted()]);
    var session = Session.Begin(new Scenario("1836", GameDate.Of(1836, 1, 1), world));

    Assert.Throws<DomainException>(() => SaveFile.Write(session));
  }

  #endregion

  #region Methods

  private static Session Begun()
  {
    return Session.Begin(EighteenThirtySix.Build());
  }

  private static Country Named(Session session, string name)
  {
    return session.World.Countries.Single(country => country.Name == name);
  }

  #endregion
}
