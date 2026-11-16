# 14 — Weak spots

## Goal

Fix what building plans 05 to 12 showed to be wrong or brittle. These are not the deliberate gaps the
README lists as known simplifications, which are future features. They are places where the model
does something it should not, hides something it should tell, or rests on a guess about its own
state.

Each section stands alone. Execute them in the order given. Each ends with its own tests, and the build
and the tests must be clean after each one. Report each section's deviations separately.

Two sections, 5 and 6, add state that a save must carry. This plan therefore raises the save file's
`Version` to 2. Version-1 files are refused with a readable message: there is no migration, as plan
13 intends.

## Read first

- `docs/plans/README.md`, "Known simplifications"
- The "As built" sections of plans 10, 11 and 12
- `surplus.Domain/Simulation/Circulation/Traffic.cs`, `Countries/Country.cs`, `Countries/Province.cs`,
  `Production/Branch.cs`
- `surplus.Application/Session.cs`, `Reporting/`, `Saving/`
- `surplus.Console/Terminal.cs`

## 1. Scarce carts are shared, not taken by the first good

**What is wrong.** When a province has fewer carts than its sends need, `Traffic` serves the goods in
register order. The first good, corn, takes every cart and nothing else moves. In the five-year run
behind plan 12 this cut Ukraine's cloth entirely, and 567 died within nine days. A real shortage of
carts should slow every flow, not stop all but one.

**Change.** Circulate in two passes per shipping province:

1. Work out every send it would make today, across all its borders and goods, as if carts were
   unlimited: the amount, and the loads it takes.
2. If the loads exceed the carriage held, scale every send by `carriage held ÷ loads wanted`, then
   draw and lay up.

Report `CarriageFellShort` once per province and good scaled down, as now.

**Tests** (`TrafficTests`)

- `Scarce_carts_slow_every_good_alike`: two goods wanted, half the carts needed; each moves half.
- `Enough_carts_move_everything_wanted`
- `A_good_that_takes_no_room_is_never_held_back_by_carts`

## 2. Goods pass through a province on their way

**What is wrong.** A province sends only toward a neighbour below its own target. A province that has
enough therefore never passes goods on, and a want two borders away is never met. The README
records it as "no transit". It is harmless with the 1836 scenario's pairs of provinces, but wrong the
moment a country has three provinces in a row.

**Change.** A province's target for a good becomes:

- its own ten days;
- plus, for each other neighbour in the same country, what that neighbour lacks of its own target
  today.

So a middle province draws in what lies beyond it, and sends it on the next day. Goods still move
one border a day. Remove the "no transit" line from the README's known simplifications.

**Tests** (`TrafficTests`)

- `Goods_reach_a_province_two_borders_away_in_two_days`
- `A_middle_province_draws_in_only_what_lies_beyond_it`
- `Transit_does_not_cross_into_another_country`

## 3. The clock stops when the player's state is no more

**What is wrong.** When the played country is annexed, `World.Played` becomes null, so the peace that
ended it is not urgent to anyone. The clock runs on, and the player learns of it from the summary,
if at all.

**Change.** `Session.AdvanceUntil` remembers whether someone was played when it began. It stops after
the first day on which no one is played any more. The digest's first line then reads `Your state is
no more: <the peace sentence>.`

**Tests** (`SessionTests`, `DigestTests`)

- `Time_stops_on_the_day_the_players_state_is_annexed`
- `The_digest_says_first_that_the_players_state_is_no_more`

## 4. Every sentence the player reads is in the player's units

**What is wrong.** Occurrence sentences are written in the domain, so they are always metric. The
digest converts its own lines, but `report` prints the domain's sentences, so an imperial player
reads tonnes there.

**Change.** A `Reporting/Chronicler.cs` renders every occurrence that carries a measure in the
player's units, through `Measures.Shown`; others it leaves as their own sentence. The console's
`report` and the digest's dated lines use it. The domain's `ToString` stays as it is, metric, for
logs and tests.

**Tests** (`ChroniclerTests`)

- `A_revaluation_reads_in_the_players_units`
- `An_occurrence_with_no_measure_reads_as_its_own_sentence`

## 5. A branch knows the technique it is worked by

**What is wrong.** A branch keeps its means and its output per hour, but not which technique they
came from. Plan 11 therefore guesses the technique twice:

- **in development**, by crediting labour to whatever grows out of the commodity a branch makes;
- **in conquest**, by matching output per hour.

Two techniques with the same product and the same rate would be confused. Development would also
credit the hand loom's labour to a technique that grows out of the power loom.

**Change.**

- `Branch` gains `string Technique`, set by `Branch.Open` and `Reequip` (both take it) and kept by
  `Load`.
- The scenario's `Work` passes it.
- `Country.Develop` credits a technique only from branches worked by one of its prerequisites.
- Cession teaches the victor the techniques the ceded branches are worked by, by name.
- The match on output per hour is removed, and so are the scenario tests that rely on it. Replace
  them with tests on `Branch.Technique`.

This is state, so it is saved: see the version note above.

**Tests**

- `A_branch_is_worked_by_the_technique_it_was_opened_or_reequipped_with` (`BranchTests`)
- `Labour_counts_toward_what_grows_out_of_the_technique_worked_not_the_commodity_made`
  (`CountryTests`)
- `A_ceded_province_teaches_the_techniques_its_branches_are_worked_by` (`WorldTests`)
- `A_branch_and_its_technique_survive_the_save` (`SaveFileTests`)

## 6. Small classes die, grow and sink by fractions, not never

**What is wrong.** Deaths, growth and pauperism are each `floor(heads × rate)`, worked out again
every day or month. A class small enough that the figure stays below one never changes:

- America's 87 unemployed never die;
- a class of 999 never grows;
- a reserve army under 100 never sinks into pauperism.

This is a rounding artefact, not a rule of the model.

**Change.** Each province keeps, per class, the fraction left over from each of the three
reckonings. That is a small value object, `Remainders`, held on `Province` and loaded by
`Province.Load`. Each reckoning adds the exact figure to the carried fraction and applies the whole
part. Over enough days the 87 die at 0.87 a day, as the rate says.

This is state, so it is saved.

**Tests** (`CountryTests`, `SaveFileTests`)

- `A_class_too_small_to_lose_a_head_in_a_day_loses_one_over_days`
- `A_class_too_small_to_grow_in_a_month_grows_over_months`
- `What_is_carried_over_survives_the_save`
- Update `A_class_too_small_to_multiply_stays_as_it_is`, which states the artefact as a rule.

## 7. The first day is quiet

**What is wrong.** The scenario's opening values are guesses (Corn 120 h a tonne, Cloth 2 h a metre),
so day one reports two revaluations that only correct the scenario's arithmetic. A player reads them
as news.

**Change.** Open the scenario at the values one day of production settles on. These are Corn
130.4536 h a tonne, Cloth 2.4175 h a metre, and Fibre 2, Iron 100, Machinery 2,100 and Carriage 10 as
before. Use the full precision of `1,812,000 / 13,890` and `703,500 / 291,000`, written as
fractions in the scenario, not the rounded figures. Update plan 00's table and the tests that expect
the day-one revaluation.

**Tests** (`EighteenThirtySixTests`)

- `The_first_day_moves_no_value`
- `Values_hold_steady_from_the_first_day`

## 8. A famine names the province it is in

**What is wrong.** Since plan 10 a country can be well fed in one province and starving in another,
but `PeopleWentWithout` gives one share for the whole country: the worst province's. The player is
not told where.

**Change.** `Rations` keeps one share per province. A province whose share falls below 1 is reported
on its own line, `ProvinceWentWithout(Country, Province, Provided, Deaths)`: *"In Russia, Ukraine
could provide only 52% of its people's subsistence; 284 died."* This replaces `PeopleWentWithout`
for that case. Significance and the digest treat it as they treat a famine now, by country and
province.

**Tests** (`CountryTests`, `WorldTests`, `DigestTests`)

- `Each_province_reports_its_own_share`
- `A_famine_names_its_province`
- `Famines_abroad_are_summed_by_province`

## 9. Countries read as their names are spoken

**What is wrong.** Sentences build country names in, so "United Kingdom grew by 200 souls" drops the
article English needs. Plans 06 to 08 all noted this.

**Change.** `Country.Found` and `Load` take an optional `string? Article` ("the" for the United
Kingdom and the United States). Occurrences that begin with a country name use it, capitalised at the
start of a sentence ("The United Kingdom grew by 200 souls."). The country's `Name` is unchanged, and
so is everything matched on it.

**Tests**

- `A_country_named_with_an_article_reads_with_it` (`CountryTests`)
- `A_sentence_that_begins_with_a_country_capitalises_its_article` (one occurrence test per type that
  begins with one)

## 10. The console is tested

**What is wrong.** `Terminal` is the only class with no test project. Every check on it so far has been
a smoke script piped in by hand.

**Change.** Add `surplus.Console.Tests` (xUnit, referencing `surplus.Console` and `surplus.Testing`;
no new package). Drive `Terminal` through a `StringReader` and `StringWriter` and an in-memory
`IPreferenceStore`. Write one test per command group, asserting on what it prints:

- time;
- looking;
- economy;
- state;
- war;
- units;
- usage errors and unknown commands.

Turn each plan's smoke script into a test that asserts no usage line and no unknown-command line was
printed.

## Out of scope

The scenario's balance: Britain's corn glut since plan 09, and a played country that already knows
every technique. Those belong to the "Goals and pressure" plan in the README, not to fixes.

## Done when

All ten sections are in. Build and tests are clean, the save version is 2, and version-1 files are
refused with a readable message.
