using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Warfare;
using Surplus.Testing;

namespace Surplus.Domain.Tests.Simulation.Warfare;

public class WarTests
{
  #region Methods

  [Fact]
  public void A_declared_war_has_not_yet_gone_anywhere()
  {
    var aggressor = CountryId.New();
    var defender = CountryId.New();

    var war = War.Declare(aggressor, defender);

    Assert.Equal(aggressor, war.Aggressor);
    Assert.Equal(defender, war.Defender);
    Assert.Equal(0, war.Momentum);
    Assert.Null(war.Prevailing);
    Assert.Null(war.Peace);
    Assert.False(war.IsOver);
  }

  [Fact]
  public void A_state_cannot_make_war_on_itself()
  {
    var france = CountryId.New();

    Assert.Throws<DomainException>(() => War.Declare(france, france));
  }

  [Fact]
  public void Each_war_has_its_own_identity()
  {
    var aggressor = CountryId.New();
    var defender = CountryId.New();

    Assert.NotEqual(War.Declare(aggressor, defender).Id, War.Declare(aggressor, defender).Id);
  }

  [Fact]
  public void A_saved_war_is_reloaded_exactly_as_it_was_left()
  {
    var id = WarId.New();
    var aggressor = CountryId.New();
    var defender = CountryId.New();

    var war = new WarBuilder()
      .WithId(id)
      .WithAggressor(aggressor)
      .WithDefender(defender)
      .WithMomentum(40)
      .Build();

    Assert.Equal(id, war.Id);
    Assert.Equal(aggressor, war.Aggressor);
    Assert.Equal(defender, war.Defender);
    Assert.Equal(40, war.Momentum);
  }

  [Fact]
  public void An_even_match_moves_no_ground_at_all()
  {
    var war = War.Declare(CountryId.New(), CountryId.New());

    war.Fight(1_000, 1_000);

    Assert.Equal(0, war.Momentum);
    Assert.Null(war.Prevailing);
  }

  [Fact]
  public void Ground_is_given_up_in_proportion_to_being_outweighed()
  {
    var war = War.Declare(CountryId.New(), CountryId.New());

    war.Fight(2_000, 1_000);

    Assert.Equal(10, war.Momentum);
  }

  [Fact]
  public void The_defender_prevails_when_it_is_the_heavier_side()
  {
    var war = War.Declare(CountryId.New(), CountryId.New());

    war.Fight(1_000, 2_000);

    Assert.Equal(-10, war.Momentum);
    Assert.Equal(war.Defender, war.Prevailing);
  }

  [Fact]
  public void A_state_with_no_one_to_field_collapses_fastest()
  {
    var war = War.Declare(CountryId.New(), CountryId.New());

    war.Fight(1_000, 0);

    Assert.Equal(20, war.Momentum);
    Assert.Equal(war.Aggressor, war.Prevailing);
  }

  [Fact]
  public void Two_states_that_can_field_no_one_fight_no_war()
  {
    var war = War.Declare(CountryId.New(), CountryId.New());

    war.Fight(0, 0);

    Assert.Equal(0, war.Momentum);
  }

  [Fact]
  public void A_state_cannot_field_fewer_than_no_one()
  {
    var war = War.Declare(CountryId.New(), CountryId.New());

    Assert.Throws<DomainException>(() => war.Fight(-1, 1_000));
    Assert.Throws<DomainException>(() => war.Fight(1_000, -1));
  }

  [Fact]
  public void No_amount_of_winning_carries_past_breaking_the_enemy_outright()
  {
    var war = War.Declare(CountryId.New(), CountryId.New());

    for (var turn = 0; turn < 10; turn++)
    {
      war.Fight(1_000, 0);
    }

    Assert.Equal(100, war.Momentum);
  }

  [Theory]
  [InlineData(0, 0)]
  [InlineData(20, 0)]
  [InlineData(25, 1)]
  [InlineData(-50, 2)]
  [InlineData(100, 4)]
  public void What_is_within_reach_is_what_the_fighting_has_earned(int momentum, int provinces)
  {
    Assert.Equal(provinces, new WarBuilder().WithMomentum(momentum).Build().ProvincesWithinReach);
  }

  [Fact]
  public void A_white_peace_is_always_within_reach_because_either_side_may_simply_stop()
  {
    Assert.True(War.Declare(CountryId.New(), CountryId.New()).Permits(Peace.White()));
    Assert.True(new WarBuilder().Overwhelming().Build().Permits(Peace.White()));
  }

  [Fact]
  public void Nothing_is_taken_that_was_not_won()
  {
    var stalemate = War.Declare(CountryId.New(), CountryId.New());

    Assert.False(stalemate.Permits(Peace.Ceding(ProvinceId.New())));
    Assert.False(stalemate.Permits(Peace.Annexation()));
  }

  [Fact]
  public void A_narrow_victory_takes_a_province_and_no_more()
  {
    var war = new WarBuilder().WithMomentum(30).Build();

    Assert.True(war.Permits(Peace.Ceding(ProvinceId.New())));
    Assert.False(war.Permits(Peace.Ceding(ProvinceId.New(), ProvinceId.New())));
    Assert.False(war.Permits(Peace.Annexation()));
  }

  [Fact]
  public void Only_a_state_broken_outright_may_be_swallowed_whole()
  {
    Assert.False(new WarBuilder().WithMomentum(99).Build().Permits(Peace.Annexation()));
    Assert.True(new WarBuilder().Overwhelming().Build().Permits(Peace.Annexation()));
  }

  [Fact]
  public void A_defender_who_has_prevailed_may_take_from_the_aggressor()
  {
    var war = new WarBuilder().WithMomentum(-100).Build();

    Assert.Equal(war.Defender, war.Prevailing);
    Assert.True(war.Permits(Peace.Annexation()));
  }

  [Fact]
  public void Concluding_a_war_puts_the_terms_on_the_record()
  {
    var war = new WarBuilder().WithMomentum(30).Build();
    var peace = Peace.Ceding(ProvinceId.New());

    war.ConcludeWith(peace);

    Assert.True(war.IsOver);
    Assert.Equal(peace, war.Peace);
  }

  [Fact]
  public void Terms_the_fighting_has_not_earned_cannot_be_imposed()
  {
    var war = War.Declare(CountryId.New(), CountryId.New());

    Assert.Throws<DomainException>(() => war.ConcludeWith(Peace.Annexation()));
  }

  [Fact]
  public void A_war_already_brought_to_an_end_cannot_be_ended_again()
  {
    var war = new WarBuilder().WithPeace(Peace.White()).Build();

    Assert.Throws<DomainException>(() => war.ConcludeWith(Peace.White()));
  }

  [Fact]
  public void There_is_no_fighting_once_there_is_a_peace_on_the_table()
  {
    var war = new WarBuilder().WithPeace(Peace.White()).Build();

    Assert.Throws<DomainException>(() => war.Fight(1_000, 0));
  }

  [Theory]
  [InlineData(0, "war in progress (momentum 0)")]
  [InlineData(30, "war in progress (momentum +30)")]
  [InlineData(-30, "war in progress (momentum -30)")]
  public void A_war_reads_as_how_far_it_has_gone(int momentum, string expected)
  {
    Assert.Equal(expected, new WarBuilder().WithMomentum(momentum).Build().ToString());
  }

  [Fact]
  public void A_concluded_war_reads_as_what_it_settled_for()
  {
    Assert.Equal(
      "war concluded on white peace",
      new WarBuilder().WithPeace(Peace.White()).Build().ToString());
  }

  #endregion
}
