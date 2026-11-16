# 07 — Player orders

## Goal

Make the game something you play instead of something you watch. Until now a player can only let
days pass and read lists, and every month brings a line per country that grew. Nothing asks for a
decision, and what matters is buried in what doesn't.

This plan does two things:

1. **Orders.** The player can now run the economy and the state of their own country: open, close,
   staff and re-equip branches, fix the working day, enact and repeal laws, change the form of
   government and the ideology. All of these rules already exist in the domain. This plan exposes
   them through `Session` and the console. War orders come with war, in plan 08.
2. **Reporting that tells you what happened.** Time passes in chunks the player chooses. The clock
   stops by itself when something needs the player's attention. A chunk ends with a short digest,
   not a scroll of sentences: what moved, what went wrong, and where the player's country stands
   now.

No new simulation rule is introduced. If an order seems to need one, the order is being given too
much to do.

## Read first

- `docs/plans/README.md`
- `surplus.Application/Session.cs`, `surplus.Console/Terminal.cs`
- `surplus.Application/Settings/Measures.cs`: every quantity or value per unit the digest prints goes
  through `Measures.Shown`.
- `Country.cs`: `Enact`, `Repeal`, `TakeForm`, `Adopt`, `CloseBranch`, `Employ`, `LayOff`, `Reequip`
- `World.cs`: `OpenBranch` (plan 06), `Technique`, `Played`, `Subsistence`
- `Province.cs` and `Branch.cs`: `FixTheWorkingDay`
- Every occurrence in `surplus.Domain/Simulation/Chronicle/`

## Changes

### `Province.cs` and `Country.cs`

```csharp
public void FixTheWorkingDay(CommodityId commodity, LaborTime workingDay);  // Province
public void FixTheWorkingDay(ProvinceId province, CommodityId commodity, LaborTime workingDay);  // Country
```

`Province.FixTheWorkingDay` delegates to the branch, which enforces its own rule. The branch refuses
a day shorter than current necessary labour. So before delegating, lower necessary labour to the new
day when it would exceed it: a Factory Act must not be blocked by yesterday's bookkeeping.
`World.Elapse` resets necessary labour the next day anyway.

### `Session.cs`: orders

One method per order, each acting for the played country and each a one- or two-line delegation.
All of them throw `DomainException` with *"No one is played: the state you sat in is no more."* when
`World.Played` is null.

```csharp
public void Enact(Law law);
public void Repeal(Law law);
public void TakeForm(GovernmentForm form);
public void Adopt(Ideology ideology);

public void OpenBranch(ProvinceId province, string technique, SocialClass workforce, int hands, LaborTime workingDay);
public void CloseBranch(ProvinceId province, CommodityId commodity);
public void Employ(ProvinceId province, CommodityId commodity, int hands);
public void LayOff(ProvinceId province, CommodityId commodity, int hands);
public void Reequip(ProvinceId province, CommodityId commodity, string technique);
public void FixTheWorkingDay(ProvinceId province, CommodityId commodity, LaborTime workingDay);
```

- `OpenBranch` goes through `World.OpenBranch`, so the player never states necessary labour.
- Technique names are resolved with `World.Technique`, which already refuses an unknown name.

### `Session.cs`: passing time until something happens

```csharp
public Passage AdvanceUntil(int days, Func<DayReport, bool> interrupts);
```

- Passes up to `days` days.
- Stops after the first day whose report `interrupts` accepts.
- `Advance(days)` stays as it is. Both run every day through one private method, so the AI's
  monthly turn (plan 12) is added in one place.

`Reporting/Passage.cs`:

```csharp
public sealed record Passage(
  IReadOnlyList<DayReport> Reports, Store? PlayedStoreBefore, bool Interrupted);
```

- `PlayedStoreBefore` is the played country's store before the first day, or null when no one is
  played.
- Content equality on `Reports`.

### `surplus.Application/Reporting/`

**`Importance.cs`.** An `enum Importance { Routine, Notable, Urgent }` and a static
`Significance.Of(Occurrence occurrence, World world)`. "Played" means that the occurrence names
`World.Played`'s country.

| Occurrence             | Importance                              |
|------------------------|-----------------------------------------|
| `PeopleWentWithout`    | Urgent if played, otherwise Notable     |
| `BranchStoodIdle`      | Urgent if played, otherwise Notable     |
| `CommodityWasRevalued` | Notable                                 |
| `PopulationGrew`       | Routine                                 |
| any other type         | Notable                                 |

The fallback matters: an occurrence a later plan adds is shown until that plan classifies it. Plans
08 to 11 extend this table.

**`Digest.cs`.**

```csharp
public sealed record Digest(GameDate From, GameDate To, bool Interrupted, IReadOnlyList<string> Lines);

public static Digest Of(Passage passage, World world, UnitSystem units);
```

`Lines`, in this order:

1. **Every urgent trouble**, once, on the day it began, with how often it came back:
   `1836-02-02  Lancashire's Cloth branch stood idle for want of material (United Kingdom). The same
   on 25 more days.`
2. **Notable occurrences, collapsed:**
   - One line per commodity whose value moved by 1% or more between the first `Before` and the last
     `After` in the passage. Show the value per unit in the player's units:
     `Corn: 120.00 → 130.45 h per t`. A value that rose and fell back to within 1% gets no line.
   - Branches that stood idle elsewhere, one line per country, province and commodity, with the
     number of days: `Russia: Muscovy's Cloth branch stood idle on 3 days.`
   - Famines elsewhere, one line per country: `Russia went short on 5 days; 720 died.`
   - Any other notable occurrence, as its own sentence, prefixed with its date.
3. **Growth, summed:** `Population grew: United Kingdom +400, United States +300, Russia +360.` Only
   countries that grew are listed; with none, no line.
4. **Where the player stands.** Skipped when no one is played.
   - `United Kingdom: 200,400 souls, 34 days of subsistence in store, 133,350 without work.`
     - Days of subsistence: the smallest, over the basket's goods, of held ÷ daily need. This
       replaces the console's own calculation from plan 06, which should call a shared helper.
     - Without work: the sum of `Idle` over every class that is a direct producer, in every
       province. Those who live on others' labour are never counted as idle.
   - `Store: Corn +12,000 t, Cloth +600,000 m, Fibre +90,000 kg, Iron +180 t, Machinery +210`
     - The change from `PlayedStoreBefore`, in register order, in the player's units.
     - A commodity that did not change is left out. With nothing changed, `Store: unchanged`.

When `Interrupted`, the console prints first: `Stopped on 1836-03-12: something needs your attention.`

Numbers use `#,0.##` with the invariant culture, as the console does.

### Console

**Time.**

| Command      | Effect                                                                       |
|--------------|------------------------------------------------------------------------------|
| `next [n]`   | Up to n days (default 1), stopping on anything urgent; prints the digest     |
| `next month` | To the first of the next month, same rule                                    |
| `next year`  | To the first of January of the next year, same rule                          |
| `report`     | Every occurrence of the last passage, in full and dated                      |

`next` interrupts on `Significance.CallsForAttention(report, dayBefore, world)`: an urgent occurrence
that was not already there the day before, judged by `Significance.Concern`. A famine is the same
famine whatever share was provided, and an idle branch is the same branch. The day before is the last
day of the previous passage when there was one. A trouble already under way stopped the clock when it
began, and does not stop it again every day it lasts.

**Listings, numbered.** Things are named by the numbers the listings print, never by typing names
(names contain spaces).

- Countries are numbered in world order.
- Provinces are numbered in the played country's order.
- Commodities are numbered in register order and techniques in catalogue order.
- Enum arguments (`Law`, `GovernmentForm`, `Ideology`, `SocialClass`) are typed by name, ignoring
  case.

| Command                                                 | Effect                                            |
|---------------------------------------------------------|---------------------------------------------------|
| `techniques`                                            | The catalogue, numbered: produces, form, output per hour, instruments and material per hand |
| `laws`                                                  | Laws in force, and those that are not             |
| `country <country#>`                                    | Another country's provinces and stores (read-only) |
| `enact <law>` / `repeal <law>`                          |                                                   |
| `form <governmentForm>` / `adopt <ideology>`            |                                                   |
| `open <province#> <technique#> <class> <hands> [hours]` | Open a branch; the working day defaults to 12     |
| `close <province#> <commodity#>`                        |                                                   |
| `employ <province#> <commodity#> <hands>`               |                                                   |
| `layoff <province#> <commodity#> <hands>`               |                                                   |
| `reequip <province#> <commodity#> <technique#>`         |                                                   |
| `day <province#> <commodity#> <hours>`                  | Fix the working day                               |

**On arrival.** After the opening status, print one line: `You govern the United Kingdom. Type help
to see what you can order.`

**Rules.**

- After a successful order, print one line confirming what changed, as a sentence.
- A `DomainException` prints its message; the domain's messages are written to be read by a player.
- Wrong arity or an unparsable number prints the command's usage line.
- `help` lists everything in four groups: time, looking, economy, state.
- When `Played` is null, the time commands and the read-only commands keep working, and the prompt
  is prefixed with `(no country)`.

## Tests

`surplus.Application.Tests/SessionTests.cs` (extend). One test per order, confirming it reaches the
played country. Keep them short, since the rules themselves are tested in the domain.

- `The_player_enacts_a_law_in_their_own_country`
- `The_player_opens_a_branch_out_of_their_own_stores` (necessary labour is the basket's cost)
- `The_player_takes_on_and_turns_off_hands`
- `The_player_reequips_a_branch_from_the_catalogue`
- `An_unknown_technique_cannot_be_used`
- `The_player_fixes_the_working_day`
- `A_player_whose_state_is_no_more_can_order_nothing`
- `Time_stops_on_the_day_something_interrupts_it`
- `Time_runs_its_full_course_when_nothing_interrupts_it`
- `A_passage_remembers_the_players_store_as_it_was`

`surplus.Application.Tests/Reporting/SignificanceTests.cs`

- `A_famine_at_home_is_urgent_and_abroad_is_notable`
- `An_idle_branch_at_home_is_urgent_and_abroad_is_notable`
- `Growth_is_routine`
- `An_occurrence_nobody_has_classified_is_shown`

`surplus.Application.Tests/Reporting/DigestTests.cs`

- `Urgent_occurrences_are_listed_one_by_one_with_their_date`
- `A_value_is_reported_once_from_where_it_started_to_where_it_ended`
- `A_value_that_came_back_to_where_it_was_is_not_reported`
- `Values_are_shown_in_the_players_units`
- `Idle_days_abroad_are_counted_not_listed`
- `Famines_abroad_are_summed`
- `Growth_is_summed_per_country`
- `The_digest_ends_with_where_the_player_stands`
- `The_change_in_the_players_store_is_shown_in_their_units`
- `A_store_that_did_not_change_says_so`
- `With_no_one_played_the_digest_says_nothing_of_a_player`

`surplus.Application.Tests/Scenarios/EighteenThirtySixTests.cs` (extend)

- `A_month_left_alone_runs_its_course_uninterrupted`
- `A_month_of_the_scenario_digests_into_a_few_lines`
  (advance to 1836-02-01; the digest has the two revaluations, one growth line, and the two lines
  on the United Kingdom, and nothing else)

`Simulation/Countries/ProvinceTests.cs` (extend)

- `A_shorter_day_is_fixed_even_when_it_falls_below_yesterdays_necessary_labour`

## Smoke test

Pipe this in and check that it exits 0 with no usage errors:

```
status
techniques
provinces
next month
employ 2 1 1000
next month
laws
enact EnclosureActs
provinces
day 1 1 14
next year
report
quit
```

Adjust the numbers to whatever the listings actually print. The point is one command from each group,
and that `next month` prints a digest of a few lines, not a line per event.

## Out of scope

- War orders (plan 08) and anything about techniques a country does not know (plan 11).
- New simulation rules: objectives, scores, pressures. See the README.
- Confirmations and undo, command history, colours, a full-screen interface.
- `GiveToPlayer` / `GiveToAi` (switching seats) stays unexposed.

## Done when

Build and tests are clean and the smoke script runs through. A month left alone reads as a digest of
about five lines. The player can do something about what it says: lay off, employ, re-equip,
legislate.
