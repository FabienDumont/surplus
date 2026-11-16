using Surplus.Application.Reporting;
using Surplus.Application.Settings;
using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Domain.Simulation.Warfare;
using Surplus.Testing;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Application.Tests.Reporting;

public class DigestTests
{
  #region Fields

  private static readonly UnitOfMeasure Metre = UnitOfMeasure.Of("m");
  private static readonly UnitOfMeasure Tonne = UnitOfMeasure.Of("t");

  private readonly Commodity _cloth = new CommodityBuilder().WithName("Cloth")
    .WithUseValue(new UseValueBuilder().WithSatisfiedWant("clothing").WithUnit(Metre).Build()).Build();

  private readonly Commodity _corn = new CommodityBuilder().WithName("Corn")
    .WithUseValue(new UseValueBuilder().WithSatisfiedWant("nourishment").WithUnit(Tonne).Build()).Build();

  #endregion

  #region Tests

  [Fact]
  public void Urgent_occurrences_are_listed_one_by_one_with_their_date()
  {
    var (world, _) = Kingdom();

    var digest = Digested(
      world, [new PeopleWentWithout("United Kingdom", 0.5m, 3)],
      [new BranchStoodIdle("United Kingdom", "Lancashire", "Cloth")]);

    Assert.Equal(
      [
        "1836-01-02  United Kingdom could provide only 50% of its people's subsistence; 3 died.",
        "1836-01-03  Lancashire's Cloth branch stood idle for want of material (United Kingdom)."
      ],
      digest.Lines.Take(2));
  }

  [Fact]
  public void A_trouble_that_lasts_is_told_once_with_how_often_it_came_back()
  {
    var (world, _) = Kingdom();
    var looms = new BranchStoodIdle("United Kingdom", "Lancashire", "Cloth");

    var digest = Digested(world, [looms], [looms], [looms], [new PeopleWentWithout("United Kingdom", 0.5m, 3)]);

    Assert.Equal(
      [
        "1836-01-02  Lancashire's Cloth branch stood idle for want of material (United Kingdom). The same on 2 more days.",
        "1836-01-05  United Kingdom could provide only 50% of its people's subsistence; 3 died."
      ],
      digest.Lines.Take(2));
  }

  [Fact]
  public void A_trouble_that_came_back_once_says_so_in_the_singular()
  {
    var (world, _) = Kingdom();
    var looms = new BranchStoodIdle("United Kingdom", "Lancashire", "Cloth");

    Assert.EndsWith("The same on 1 more day.", Digested(world, [looms], [looms]).Lines[0]);
  }

  [Fact]
  public void A_value_is_reported_once_from_where_it_started_to_where_it_ended()
  {
    var (world, _) = Kingdom();

    var digest = Digested(world, [Revalued(2m, 2.4m)], [Revalued(2.4m, 2.5m)]);

    Assert.Equal("Cloth: 2.00 → 2.50 h per m", digest.Lines[0]);
  }

  [Fact]
  public void A_value_that_came_back_to_where_it_was_is_not_reported()
  {
    var (world, _) = Kingdom();

    var digest = Digested(world, [Revalued(2m, 2.4m)], [Revalued(2.4m, 2.01m)]);

    Assert.DoesNotContain(digest.Lines, line => line.StartsWith("Cloth"));
  }

  [Fact]
  public void Values_are_shown_in_the_players_units()
  {
    var (world, _) = Kingdom();

    var digest = Digest.Of(Passage(world, [Revalued(2m, 2.5m)]), world, UnitSystem.Imperial);

    // A yard is 0.9144 of a metre, and so holds that share of a metre's labour.
    Assert.Equal("Cloth: 1.83 → 2.29 h per yd", digest.Lines[0]);
  }

  [Fact]
  public void Idle_days_abroad_are_counted_not_listed()
  {
    var (world, _) = Kingdom();
    var idle = new BranchStoodIdle("Russia", "Muscovy", "Cloth");

    var digest = Digested(world, [idle], [idle, new BranchStoodIdle("Russia", "Ukraine", "Corn")], [idle]);

    Assert.Equal(
      ["Russia: Muscovy's Cloth branch stood idle on 3 days.", "Russia: Ukraine's Corn branch stood idle on 1 day."],
      digest.Lines.Take(2));
  }

  [Fact]
  public void Famines_abroad_are_summed()
  {
    var (world, _) = Kingdom();

    var digest = Digested(world, [new PeopleWentWithout("Russia", 0.5m, 100)], [new PeopleWentWithout("Russia", 0.8m, 44)]);

    Assert.Equal("Russia went short on 2 days; 144 died.", digest.Lines[0]);
  }

  [Fact]
  public void Worklessness_and_pauperism_are_summed_per_country()
  {
    var (world, _) = Kingdom();

    var digest = Digested(
      world,
      [new TheWorklessWentWithout("United Kingdom", 3_350, 16), new TheWorklessWentWithout("United States", 100, 1)],
      [new TheWorklessWentWithout("United Kingdom", 3_300, 15), new SankIntoPauperism("United Kingdom", 28)]);

    Assert.Equal(
      [
        "United Kingdom: those with no wage went short on 2 days; 31 died.",
        "United States: those with no wage went short on 1 day; 1 died.",
        "United Kingdom: 28 sank into pauperism."
      ],
      digest.Lines.Take(3));
  }

  [Fact]
  public void Anything_else_worth_reading_is_told_with_its_date()
  {
    var (world, _) = Kingdom();

    var digest = Digested(world, [], [new CometSighted()]);

    Assert.Equal("1836-01-03  A comet was seen over the Channel.", digest.Lines[0]);
  }

  [Fact]
  public void Growth_is_summed_per_country()
  {
    var (world, _) = Kingdom();

    var digest = Digested(
      world, [new PopulationGrew("United Kingdom", 200), new PopulationGrew("Russia", 180)],
      [new PopulationGrew("United Kingdom", 201)]);

    Assert.Equal("Population grew: United Kingdom +401, Russia +180.", digest.Lines[0]);
  }

  [Fact]
  public void Each_open_war_is_summed_up_once_where_it_stands_at_the_end()
  {
    var (world, kingdom, russia, states) = AtWarWithTwo();
    world.DeclareWar(kingdom.Id, russia.Id);
    world.DeclareWar(states.Id, kingdom.Id);

    var digest = Digested(
      world,
      [new TheFrontMoved("United Kingdom", "Russia", 2), new TheFrontMoved("United States", "United Kingdom", 0)],
      [new TheFrontMoved("United Kingdom", "Russia", 4), new TheFrontMoved("United States", "United Kingdom", -3)]);

    Assert.Equal(
      [
        "War: United Kingdom on Russia, +4, United Kingdom prevailing.",
        "War: United States on United Kingdom, -3, United Kingdom prevailing."
      ],
      digest.Lines.Where(line => line.StartsWith("War:")));
  }

  [Fact]
  public void A_front_that_has_not_moved_has_no_one_prevailing()
  {
    var (world, kingdom, russia, _) = AtWarWithTwo();
    world.DeclareWar(kingdom.Id, russia.Id);

    var digest = Digested(world, [new TheFrontMoved("United Kingdom", "Russia", 0)]);

    Assert.Contains("War: United Kingdom on Russia, 0, neither side prevailing.", digest.Lines);
  }

  [Fact]
  public void A_war_ended_during_the_passage_is_told_by_its_peace_not_its_front()
  {
    var (world, kingdom, russia, _) = AtWarWithTwo();
    var war = world.DeclareWar(kingdom.Id, russia.Id);
    world.MakePeace(war.Id, russia.Id, Peace.White());

    var digest = Digested(world, [new TheFrontMoved("United Kingdom", "Russia", 0)]);

    Assert.DoesNotContain(digest.Lines, line => line.StartsWith("War:"));
  }

  [Fact]
  public void Without_growth_there_is_no_line_for_it()
  {
    var (world, _) = Kingdom();

    Assert.DoesNotContain(Digested(world, []).Lines, line => line.StartsWith("Population"));
  }

  [Fact]
  public void The_digest_ends_with_where_the_player_stands()
  {
    var (world, kingdom) = Kingdom();

    var digest = Digest.Of(PassageFrom(world, kingdom.Store), world, UnitSystem.Metric);

    // A hundred proletarians, no branch to employ them, and ten days of cloth and corn.
    Assert.Equal(
      ["United Kingdom: 100 souls, 10 days of subsistence in store, 100 without work.", "Store: unchanged"],
      digest.Lines);
  }

  [Fact]
  public void The_change_in_the_players_store_is_shown_item_by_item()
  {
    var (world, _) = Kingdom();
    var before = Store.Of(Stock.Of(_corn.Id, Quantity.Of(11m, Tonne)), Stock.Of(_cloth.Id, Quantity.Of(408.56m, Metre)));

    var digest = Digest.Of(PassageFrom(world, before), world, UnitSystem.Metric);

    Assert.Equal("Store: Cloth +91.44 m, Corn -1 t", digest.Lines[^1]);
  }

  [Fact]
  public void The_change_in_the_players_store_is_shown_in_their_units()
  {
    var (world, _) = Kingdom();
    var before = Store.Of(Stock.Of(_corn.Id, Quantity.Of(10m, Tonne)), Stock.Of(_cloth.Id, Quantity.Of(408.56m, Metre)));

    var digest = Digest.Of(PassageFrom(world, before), world, UnitSystem.Imperial);

    Assert.Equal("Store: Cloth +100 yd", digest.Lines[^1]);
  }

  [Fact]
  public void A_store_that_was_never_taken_stock_of_is_counted_from_nothing()
  {
    var (world, _) = Kingdom();

    var digest = Digest.Of(PassageFrom(world, null), world, UnitSystem.Metric);

    Assert.Equal("Store: Cloth +500 m, Corn +10 t", digest.Lines[^1]);
  }

  [Fact]
  public void A_country_of_no_one_has_no_one_to_keep()
  {
    var nobody = new CountryBuilder().WithName("United Kingdom").Played()
      .WithProvinces(new ProvinceBuilder().Unpeopled().Build()).Build();
    var world = new WorldBuilder().WithRegister(CommodityRegister.Of(_cloth, _corn)).WithSubsistence(Basket())
      .WithCountries(nobody).Build();

    var digest = Digest.Of(Passage(world, []), world, UnitSystem.Metric);

    Assert.Equal("United Kingdom: 0 souls, no one to keep, 0 without work.", digest.Lines[^2]);
  }

  [Fact]
  public void With_no_one_played_the_digest_says_nothing_of_a_player()
  {
    var world = new WorldBuilder().WithRegister(CommodityRegister.Of(_cloth, _corn)).Build();

    var digest = Digest.Of(Passage(world, [new PopulationGrew("France", 1)]), world, UnitSystem.Metric);

    Assert.Equal(["Population grew: France +1."], digest.Lines);
  }

  [Fact]
  public void A_digest_spans_the_days_of_its_passage_and_says_whether_it_was_cut_short()
  {
    var (world, kingdom) = Kingdom();
    var passage = new Passage([Day(2, []), Day(3, []), Day(4, [])], kingdom.Store, true);

    var digest = Digest.Of(passage, world, UnitSystem.Metric);

    Assert.Equal(GameDate.Of(1836, 1, 2), digest.From);
    Assert.Equal(GameDate.Of(1836, 1, 4), digest.To);
    Assert.True(digest.Interrupted);
  }

  [Fact]
  public void No_days_make_nothing_to_tell()
  {
    var (world, kingdom) = Kingdom();

    Assert.Throws<DomainException>(() => Digest.Of(new Passage([], kingdom.Store, false), world, UnitSystem.Metric));
  }

  [Fact]
  public void Digests_of_the_same_lines_are_the_same_digest()
  {
    var (world, _) = Kingdom();
    var digest = Digested(world, [new PopulationGrew("Russia", 1)]);
    var same = digest with { Lines = [.. digest.Lines] };

    Assert.Equal(digest, same);
    Assert.Equal(digest.GetHashCode(), same.GetHashCode());
    Assert.NotEqual(digest, digest with { Interrupted = true });
    Assert.False(digest.Equals(null));
  }

  #endregion

  #region Methods

  /// <summary>
  /// A played United Kingdom of a hundred proletarians and no branch, holding
  /// ten days of a basket of half a metre of cloth and a hundredth of a tonne of
  /// corn a head.
  /// </summary>
  private (World World, Country Kingdom) Kingdom()
  {
    var kingdom = new CountryBuilder().WithName("United Kingdom").Played()
      .WithProvinces(new ProvinceBuilder().WithClasses(ClassPresence.Of(SocialClass.Proletariat, 100)).Build())
      .Holding(Stock.Of(_cloth.Id, Quantity.Of(500m, Metre)), Stock.Of(_corn.Id, Quantity.Of(10m, Tonne))).Build();
    var world = new WorldBuilder().WithRegister(CommodityRegister.Of(_cloth, _corn)).WithSubsistence(Basket())
      .WithCountries(kingdom).Build();

    return (world, kingdom);
  }

  /// <summary>The played United Kingdom of <see cref="Kingdom" />, with Russia and the United States beside it.</summary>
  private (World World, Country Kingdom, Country Russia, Country States) AtWarWithTwo()
  {
    var (_, kingdom) = Kingdom();
    var russia = new CountryBuilder().WithName("Russia").Build();
    var states = new CountryBuilder().WithName("United States").Build();
    var world = new WorldBuilder().WithRegister(CommodityRegister.Of(_cloth, _corn)).WithSubsistence(Basket())
      .WithCountries(kingdom, russia, states).Build();

    return (world, kingdom, russia, states);
  }

  private Subsistence Basket()
  {
    return Subsistence.Of(Stock.Of(_cloth.Id, Quantity.Of(0.5m, Metre)), Stock.Of(_corn.Id, Quantity.Of(0.01m, Tonne)));
  }

  /// <summary>Days from the second of January on, one per list of occurrences, digested in metric.</summary>
  private static Digest Digested(World world, params Occurrence[][] days)
  {
    return Digest.Of(Passage(world, days), world, UnitSystem.Metric);
  }

  private static Passage Passage(World world, params Occurrence[][] days)
  {
    return PassageFrom(world, world.Played?.Store, days);
  }

  private static Passage PassageFrom(World world, Store? before, params Occurrence[][] days)
  {
    List<Occurrence[]> stretch = days.Length == 0 ? [[]] : [.. days];

    return new Passage([.. stretch.Select((occurrences, index) => Day(index + 2, occurrences))], before, false);
  }

  private static DayReport Day(int dayOfJanuary, IReadOnlyList<Occurrence> occurrences)
  {
    return new DayReport(GameDate.Of(1836, 1, dayOfJanuary), occurrences);
  }

  private static CommodityWasRevalued Revalued(decimal before, decimal after)
  {
    return new CommodityWasRevalued("Cloth", "m", Hours(before), Hours(after));
  }

  private static Value Hours(decimal hours)
  {
    return Value.CrystallisedFrom(LaborTime.FromHours(hours));
  }

  #endregion
}
