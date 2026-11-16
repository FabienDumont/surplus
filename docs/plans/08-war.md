# 08 — War

## Goal

Connect war to the world. `War` already knows how momentum moves and what peace it permits, but it is
handed two bare integers and nothing applies the peace it concludes. After this plan a war is declared
between two countries of the world, fought monthly with what each society can put under arms, and
ended by a peace that actually moves provinces or erases a state. The player can declare war and
make peace from the console.

## Read first

- `docs/plans/README.md`, decision 10
- `surplus.Domain/Simulation/Warfare/War.cs`, `Peace.cs`, `PeaceTerms.cs` — read the comments; the
  rules below lean on `Prevailing`, `ProvincesWithinReach`, `Permits`, `ConcludeWith`
- `surplus.Domain/Simulation/Countries/Country.cs` — `Annex`, `Cede` and why the last province cannot
  be ceded
- `surplus.Domain/Simulation/Society/ClassComposition.cs` — `Armable`
- `World.cs` as left by plan 06
- `surplus.Application/Session.cs`, `Reporting/Significance.cs` and `Reporting/Digest.cs` as left by
  plan 07, and the console's numbered listings

## Changes

### `Country.cs`

```csharp
/// Those this country could put under arms: read off its class structure, province by province.
public int Armable { get; }
```

Sum of `province.Composition.Armable`.

### `World.cs`

```csharp
public War DeclareWar(CountryId aggressor, CountryId defender);
public void MakePeace(WarId war, CountryId proposer, Peace peace);
public IReadOnlyList<War> WarsOf(CountryId country);   // open wars only
```

**DeclareWar.** Both countries must be in the world. Two countries already at war with each other (an
open war, in either direction) cannot open a second. `War.Declare` already refuses a state warring on
itself. The new war is added to `Wars` and returned.

**MakePeace.** Check everything before changing anything, so a refused peace leaves the world exactly
as it was:

1. The war exists and is not over; the proposer is one of its two belligerents.
2. `PeaceTerms.White`: either belligerent may propose it.
3. `Cession` and `Annexation`: only the side that is `Prevailing` may propose. The other side is the
   loser.
4. `Cession`: every ceded province is governed by the loser, and the loser keeps at least one province
   (a peace that would take the last is refused with a message pointing to annexation).
5. `War.ConcludeWith(peace)` — it enforces `Permits` itself.
6. Apply the terms:
   - **White:** nothing moves.
   - **Cession:** for each ceded province, `winner.Annex(loser.Cede(id))`.
   - **Annexation:** every province of the loser passes to the winner; everything in the loser's
     store is laid up in the winner's; every other open war the loser is in is concluded with a white
     peace; the loser is removed from the world. If the loser was the played country, `Played` is
     null from then on.

Do step 5 last among the checks and first among the effects: once `ConcludeWith` has succeeded,
nothing after it may throw.

Concluded wars stay in `Wars` as a record. A removed country's id may still appear in old wars; lookups
by that id throw as for any unknown country, and code that walks `Wars` must only resolve the
belligerents of open ones.

**Elapse.** New step at the end of the day, on the first of a month only: for every open war,
`war.Fight(aggressor.Armable, defender.Armable)`, then an occurrence:

`Chronicle/TheFrontMoved.cs`: `(string Aggressor, string Defender, int Momentum)`, e.g. *"The United
Kingdom's war on the United States stands at +18."* Emitted every month a war is open, moved or not.

The full order of a day is now: work, revalue, necessary labour, provision and deaths, then on the
first of the month growth and fighting.

### `surplus.Testing`

`WorldBuilder.WithWars` already exists from plan 01. Extend `CountryBuilder` only if a test needs it.

## Tests

`Simulation/Countries/CountryTests.cs` (extend)

- `A_country_can_arm_everyone_it_does_not_own`

`Simulation/WorldTests.cs` (extend)

- `War_is_declared_between_two_countries_of_the_world`
- `War_cannot_be_declared_on_a_country_that_is_not_there`
- `Two_countries_already_at_war_cannot_go_to_war_again` (either direction)
- `Two_countries_at_peace_again_can_go_to_war_again`
- `A_war_is_fought_on_the_first_of_each_month_and_on_no_other_day`
- `A_war_is_fought_with_what_each_society_can_put_under_arms`
  (unequal `Armable`: momentum moves toward the heavier side by what `War.Fight` gives)
- `A_slave_society_fights_with_fewer_than_it_counts` (slaves are not armable)
- `The_front_is_reported_every_month`
- `A_war_that_is_over_is_no_longer_fought`
- `Either_side_may_simply_stop` (white peace from the losing side succeeds; nothing moves)
- `Only_the_side_that_is_prevailing_dictates_terms`
- `A_cession_moves_the_ceded_provinces_to_the_victor`
- `A_province_the_loser_does_not_govern_cannot_be_ceded` (world unchanged, war still open)
- `A_cession_cannot_take_a_countrys_last_province` (world unchanged, war still open)
- `A_peace_the_fighting_has_not_earned_is_refused` (world unchanged, war still open)
- `An_annexed_country_ceases_to_exist`
- `The_victor_takes_the_annexed_countrys_provinces_and_stores`
- `The_other_wars_of_an_annexed_country_end_with_it`
- `When_the_humans_country_is_annexed_no_one_is_played`
- `A_stranger_to_a_war_cannot_end_it`

## Session and reporting

New orders on `Session`, following plan 07's rule that each order acts for the played country and is
refused when no one is played:

```csharp
public War DeclareWarOn(CountryId defender);
public void MakePeace(WarId war, Peace peace);
```

The war's occurrences come with their own types, so that `Significance` can tell them apart.
`World.DeclareWar` and `World.MakePeace` return or record them as follows:

- `Chronicle/WarWasDeclared.cs`: `(string Aggressor, string Defender)`, e.g. *"Russia has declared war
  on the United States."* `DeclareWar` adds it to the next day's report.
- `Chronicle/PeaceWasMade.cs`: `(string Winner, string Loser, string Terms)`, e.g. *"The United
  Kingdom and the United States have made peace: Carolina is ceded to the United Kingdom."* The
  same applies to `MakePeace`.

Keep these pending occurrences in `World` (`World.Pending`, and a parameter of `World.Load`) and emit
them first in the next `Elapse`. Plan 13 must then
save them; say so in the report.

Extend the table in `Significance`:

| Occurrence       | Importance                                   |
|------------------|----------------------------------------------|
| `WarWasDeclared` | Urgent if the played country is the defender, otherwise Notable |
| `PeaceWasMade`   | Urgent if the played country is the loser, otherwise Notable |

What the player did needs no stopping for; what was done to them does. The one who proposed a white
peace stands as its `Winner`.
| `TheFrontMoved`  | Routine                                      |

`Digest` adds one line per open war, from the last `TheFrontMoved` of the passage: `War: United
Kingdom on United States, +18, the United Kingdom prevailing.`

## Console

`status` lists open wars with momentum and who is prevailing.

| Command                                                   | Effect                                      |
|-----------------------------------------------------------|---------------------------------------------|
| `wars`                                                    | Open wars with numbers, momentum, provinces within reach |
| `war <country#>`                                          | Declare war                                 |
| `peace <war#> white`                                      |                                             |
| `peace <war#> cede <province name>[, <province name>...]` | Enemy provinces by name, comma-separated    |
| `peace <war#> annex`                                      |                                             |

Ceded provinces are the one place names are typed, because the enemy's provinces have no number in
the player's own listing. Match them against the loser's provinces, ignoring case. `help` gains a
fifth group, war.

Add to `surplus.Application.Tests`:

- `The_player_declares_war_and_makes_peace` (`SessionTests`)
- `A_war_declared_on_the_player_is_urgent` and `The_front_is_routine` (`SignificanceTests`)
- `Each_open_war_is_summed_up_once` (`DigestTests`)

Extend plan 07's smoke script with `war 2`, `next month`, `wars`, `peace 1 white`.

## Out of scope

Armies as entities, battles, casualties (war kills no one in this plan), occupation, war goals,
alliances, adjacency (any country may fight any other), the cost of war to production. Do not add any
of these; `War`'s comment explains why generals are not modelled, and the rest is later work.

## Done when

Build and tests are clean. In a test, a far stronger country that declares war and waits long enough
can annex its enemy, and the world then holds one country fewer.
