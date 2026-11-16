# 11 — Known techniques

## Goal

Make knowing how to produce something a fact about a country, not about the world. Today every
country can install every technique in the catalogue on the first day. After this plan each country
knows some of them. It finds out the others by working in the trades they grow out of, at a pace set
by its mode of production. It can also take knowledge from another state by conquest.

This is the technology tree, read through Marx. In Marx nobody researches technology: the productive
forces develop out of the labour process. The relations under which that labour is done decide how
hard development is driven. Capital is compelled to cheapen its product or be undersold, which is the
whole movement of relative surplus-value (Capital I, part IV). A feudal lord takes a fixed rent and
has little reason to change how the corvée is worked. A slave economy keeps crude, heavy tools
because a slave has no stake in sparing a good one. So there is no research budget and no research
slider here. Labour performed is what moves development, and the mode of production weighs it.

Knowing and using stay two different things, as they always have been: the Romans knew the water mill
and barely used it. A country may know a technique and not be able to work it. It needs the
instruments in store (plan 03), and it needs a class that can work it: a factory cannot be staffed by
serfs.

## As built

Where the build departs from the text below:

- **`Country.Load` takes the knowledge after the provinces.** Plan 10 removed the store parameter
  this text puts it after.
- **`CountryBuilder` defaults to knowing nothing.** It gains `Knowing(names...)` and
  `WithKnowledge(...)`.
- **Old tests now follow the new rules; the rules were not weakened.** Tests that opened branches
  were given the knowledge they need. Those that set serfs or journeymen to power looms now use
  proletarians, a class that can work them.
- **The refusal names the class as the code spells it:** *"Serfs cannot work by Power loom: no mode of
  production that throws up Serfs has ever borne MachineIndustry."*
- **The console's known / under way / out of reach reading is `Reporting/Mastery.Of`** in the
  application layer, so it can be tested. In 1836 the played United Kingdom knows all eight
  techniques, so its column reads "known" throughout.

## Read first

- `docs/plans/README.md`, decisions 7 and 8
- `docs/plans/00-scenario-data.md`, the techniques table and "What each country knows"
- `surplus.Domain/Simulation/Production/Technique.cs`, `ProductiveForm.cs`, `ProductiveFormProfile.cs`
  and `ProductiveForms.cs`. A form already knows the modes of production that have borne it.
- `surplus.Domain/Simulation/Society/ClassProfile.cs` and `SocialClasses.cs`. A class already knows the
  modes that throw it up.
- `surplus.Domain/Simulation/Countries/Country.cs`: `OpenBranch`, `Reequip`, `WorkOneDay`
- `surplus.Domain/Simulation/Countries/Province.cs`: `Mode`
- `World.cs` as left by plan 10: `Assemble`, `Elapse`, `MakePeace`
- `surplus.Application/Reporting/Significance.cs` and the console's `techniques` listing from plan 07

## Changes

### `Production/Technique.cs`

```csharp
/// The techniques this one grows out of, by name, in the order given. Empty for an elementary technique.
public IReadOnlyList<string> Requires { get; }

/// The labour that must be spent in the trades it grows out of before this way of working is found.
public LaborTime Development { get; }

/// Whether this technique grows out of no other: a country either knows it from the start or never finds it.
public bool IsElementary => Requires.Count == 0;

/// The same technique, found by working in the trades of those it is developed from.
public Technique DevelopedFrom(LaborTime development, params string[] prerequisites);

/// Whether a class can work by this technique.
public bool CanBeWorkedBy(SocialClass workforce);
```

- `Technique.Of` is unchanged and gives an elementary technique: no prerequisites, `Development` none.
- `DevelopedFrom` throws `DomainException` when:
  - no prerequisite is named;
  - `development` is none;
  - a prerequisite is named twice;
  - the technique names itself.
- `CanBeWorkedBy` is true when some mode of production is native both to the workforce's class and
  to this technique's form. Read both from the profiles that already exist. The doc comment says why:
  - A form of the labour process is borne only by certain relations of production.
  - Serfs are bound to the land and owe labour by coercion. Machine industry presupposes labour-power
    bought by the day, so it cannot stand on serfdom.
  - This is the fetter that productive forces meet in old relations. The class has to change before
    the machine can be installed.
- Equality and the hash include `Requires`, in order, and `Development`. `ToString` is unchanged.

### `Production/TechnicalProgress.cs`

A static register in the style of `ProductiveForms`: one table of how hard each mode of production
drives the development of technique, with the reasons in comments.

```csharp
public static decimal Drive(this ModeOfProduction mode);
```

| Mode              | Drive | Comment to write                                                         |
|-------------------|-------|--------------------------------------------------------------------------|
| PrimitiveCommunal | 0.02  | Production for use and by custom; what works is handed down, not pushed  |
| Slave             | 0.05  | The producer has no stake in the instrument; tools are made crude to survive them |
| Feudal            | 0.1   | Rent is fixed by custom and taken by force; the lord gains little from changing the labour process |
| Capitalist        | 1     | Competition compels each capital to cut its labour time or be undersold  |
| Socialist         | 1     | Set equal to capital's pending a model of planning; say so in the comment |
| Communist         | 1     | Likewise                                                                  |

These are tuned figures, not measurements. The comment says so, and says that only their ratios
matter.

### `Production/Knowledge.cs`

A `sealed record Knowledge`: what a country knows how to do, and what it is on the way to finding.
Content equality.

```csharp
public static Knowledge None { get; }
public static Knowledge Of(IEnumerable<string> known, IEnumerable<Development> underWay);

/// Technique names, in the order they became known.
public IReadOnlyList<string> Known { get; }

/// Techniques partly found, in the order work on them began.
public IReadOnlyList<Development> UnderWay { get; }

public bool Knows(string technique);
public LaborTime Spent(string technique);                     // none when not under way
public Knowledge Learnt(string technique);                    // already known: an equal instance
public Knowledge Advanced(string technique, LaborTime hours); // already known: DomainException
```

- `Of` throws when a name is known twice, is under way twice, or is both known and under way.
- `Learnt` appends the name to `Known` and drops it from `UnderWay`.
- `Advanced` adds to an existing entry, or appends a new one.

`Production/Development.cs`: `sealed record Development(string Technique, LaborTime Spent)`.

### `Countries/Country.cs`

```csharp
public Knowledge Knowledge { get; private set; }

public bool Knows(Technique technique);

/// Comes to know a technique by any means but working: the scenario, or conquest.
public void Learn(Technique technique);

/// Credits a day's labour to the techniques it brings closer, and returns those found today.
public IReadOnlyList<Technique> Develop(IReadOnlyList<Technique> catalogue, DayOfProduction day);
```

- `Found` starts with `Knowledge.None`.
- `Load` gains a `Knowledge` parameter after `store`, and `CountryBuilder` gets `WithKnowledge`.
  Existing builder defaults should know every technique a test hands them, or set the knowledge
  explicitly. Do not weaken the new rules to keep old tests green.
- `OpenBranch` and `Reequip` refuse, before anything else is checked:
  - a technique the country does not know: *"Russia does not know how to work by Power loom."*;
  - a workforce that cannot work it: *"Serfs cannot work by Power loom: no mode of production that
    throws up serfs has ever borne MachineIndustry."*
- `Develop` works through the day's branches in two passes. The first pass credits labour:
  - Go province by province, in order, and through each province's branches in order, skipping any
    branch in `day.Idle`.
  - A branch's labour today is `Hands × WorkingDay × province.Mode.Drive()`.
  - Credit that labour, once, to every technique in catalogue order that meets all of these:
    - it is not known and not elementary;
    - every one of its prerequisites is known;
    - one of its prerequisites produces what this branch produces.
- The second pass learns: every technique whose `Spent` has reached its `Development` is learnt, in
  catalogue order, and returned.
- A technique learnt today brings no other within reach until tomorrow, because all crediting is done
  before any learning.
- The doc comment carries the theory:
  - Invention comes out of the trades that prepare it. The machine is first built by manufacture,
    and only later by machines.
  - Hours are counted in the trades the technique grows out of, not in a laboratory, and they are
    weighed by the relations they were worked under.

### `World.cs`

- **Assemble** adds these rejections:
  - A prerequisite that is not a technique listed **earlier** in the catalogue: *"Power loom cannot be
    developed from Machine shop: no technique by that name comes before it."* This forbids cycles as
    well.
  - A country that knows a name that is not in the catalogue: *"Russia knows Steam hammer, which is not
    a technique anyone has."*
  - `Load` checks neither.
- **Elapse** keeps each country's `DayOfProduction` from step 1. A new daily step comes after provision
  and deaths: every country develops, and each technique found is reported. The full order of a day is
  now:
  1. work;
  2. revalue;
  3. necessary labour;
  4. circulation (plan 10);
  5. provision and deaths;
  6. development;
  7. on the first of the month, fighting, pauperism and growth.

  Development counts the labour of carting branches like any other. No technique grows out of Carting
  yet.
- **MakePeace**: conquest carries knowledge. This happens after `ConcludeWith`, with no throwing:
  - **Cession:** for each ceded province, the winner learns every technique the loser knows that
    produces what one of that province's branches produces, at that branch's output per hour. That is
    the technique the branch is actually worked by. The winner takes the workshops and the hands that
    know them.
  - **Annexation:** the winner learns everything the loser knew, in the loser's order. What the loser
    had only half found is lost with it.

### `Chronicle/TechniqueWasDeveloped.cs`

`(string Country, string Technique, string Commodity)`, e.g. *"Russia has found out how to work by
Large farming, a new way of making Corn."*

### Scenario

`docs/plans/00-scenario-data.md` now carries a "Develops from" column and a "What each country knows"
section. In `EighteenThirtySix`:

- The catalogue follows the table's new order, which puts every technique after the ones it requires.
- These techniques are built with `DevelopedFrom`:
  - Large farming: 50,000,000 h, from Peasant tillage;
  - Machine shop: 1,000,000 h, from Iron working;
  - Power loom: 20,000,000 h, from Hand loom and Machine shop.
- Each country learns its starting techniques with `Learn` before the world is assembled. Since
  plan 10 the catalogue also holds Carting, an elementary technique every country knows.

### Console

`techniques`, numbered since plan 07, gains a column for the played country. After what each
technique produces, its form, output per hour, instruments and material, it shows one of:

- `known`;
- `under way, 42%` (spent over development, rounded down);
- `out of reach until Machine shop is known` for one whose prerequisites are not all known;
- `not known` for an elementary one it lacks.

`Significance` classifies `TechniqueWasDeveloped` as Urgent when the played country found it, and
Notable otherwise.

## Tests

`Simulation/Production/TechniqueTests.cs` (extend)

- `An_elementary_technique_grows_out_of_no_other`
- `A_technique_developed_from_nothing_is_no_development`
- `A_technique_that_costs_no_labour_to_find_is_no_development`
- `A_technique_cannot_grow_out_of_the_same_one_twice`
- `A_technique_cannot_grow_out_of_itself`
- `Techniques_developed_differently_are_different_techniques`
- `A_proletarian_can_work_a_power_loom`
- `A_serf_cannot_work_a_power_loom`
- `A_slave_cannot_work_a_power_loom`
- `A_serf_can_work_by_hand_and_in_cooperation` (Handicraft and Cooperation both stand on the feudal mode)

`Simulation/Production/TechnicalProgressTests.cs`

- `Capital_drives_development_ten_times_as_hard_as_the_lord`
- `Every_mode_of_production_drives_development_somewhat` (all above zero)

`Simulation/Production/KnowledgeTests.cs`

- `A_country_that_has_learnt_nothing_knows_nothing`
- `What_is_known_cannot_also_be_under_way`
- `The_same_technique_cannot_be_known_twice`
- `Labour_spent_toward_a_technique_adds_up`
- `A_technique_learnt_is_no_longer_under_way`
- `What_is_already_known_cannot_be_advanced`
- `Learning_what_is_known_changes_nothing`

`Simulation/Countries/CountryTests.cs` (extend)

- `A_country_cannot_open_a_branch_by_a_technique_it_does_not_know`
- `A_country_cannot_reequip_by_a_technique_it_does_not_know`
- `A_country_cannot_set_a_class_to_a_technique_it_cannot_work`
- `A_known_technique_may_still_be_out_of_use` (knowing a technique opens no branch and draws nothing)
- `A_days_labour_brings_closer_the_techniques_it_grows_into`
- `Labour_in_an_unrelated_trade_brings_nothing_closer`
- `A_technique_whose_prerequisites_are_not_all_known_is_out_of_reach`
- `An_idle_branch_brings_nothing_closer`
- `A_capitalist_province_finds_out_ten_times_faster_than_a_feudal_one`
  (1,000 hands on iron, 12-hour day: Machine shop at 1,000,000 h is found on day 84 under capital
  and on day 834 under feudalism)
- `A_technique_is_found_when_enough_labour_has_gone_into_it`
- `A_technique_found_today_brings_no_other_within_reach_until_tomorrow`

`Simulation/WorldTests.cs` (extend)

- `A_technique_must_grow_out_of_one_listed_before_it`
- `A_country_cannot_know_a_technique_no_one_has`
- `A_day_reports_the_techniques_found_in_it`
- `A_ceded_province_brings_its_victor_the_techniques_worked_in_it`
- `A_ceded_province_brings_nothing_its_branches_do_not_use`
- `An_annexing_victor_learns_everything_the_annexed_knew`

`surplus.Application.Tests/SessionTests.cs` (extend)

- `A_technique_the_country_has_not_found_cannot_be_used` (in the catalogue, not known to the played
  country; the domain's message reaches the player)

`surplus.Application.Tests/Reporting/SignificanceTests.cs` (extend)

- `A_technique_found_at_home_is_urgent`

`surplus.Application.Tests/Scenarios/EighteenThirtySixTests.cs` (extend)

- `Each_country_knows_what_the_scenario_says`
- `Every_branch_in_the_scenario_works_by_a_technique_its_country_knows`
  (matched by commodity and output per hour)
- `Every_branch_in_the_scenario_is_worked_by_a_class_that_can_work_its_technique`
- `Nothing_is_found_out_in_a_year_left_alone`
- `Russia_finds_out_large_farming_on_its_673rd_day`
  (62,000 serfs on corn, 12 hours, drive 0.1: 74,400 h a day against 50,000,000 h. If deaths or a
  change of hands move the day, the plan's arithmetic is wrong: report it rather than tune the test.)

## Out of scope

Knowledge travelling by trade, migration or espionage. Techniques that are forgotten. Development
driven by labour shortages, the rate of profit or competition between countries, beyond the flat
weight of the mode. A player or AI choice of what to develop: development goes wherever labour is
done, and is chosen only by choosing what to work at. New techniques beyond the scenario's eight.
Earlier start dates (see the README).

## Done when

Build and tests are clean:

- A year of the scenario left alone finds nothing out.
- Left alone long enough, Russia finds out large farming.
- Russia can then reequip its corn branches to large farming, still with serfs.
- Russia can never set serfs to a power loom, whatever it comes to know.
