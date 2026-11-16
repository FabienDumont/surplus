# 06 — Subsistence and population

## Goal

Make people eat. After this plan every head in every country takes a daily basket out of the store;
where the store cannot provide it people die; where it can, the population grows. And the part of the
working day that reproduces the labourer stops being a number someone typed and becomes what
`Branch.NecessaryLabor`'s own comment says it should be: the value of what they actually consume.

That last step closes the loop the whole model is built around. Cheaper corn means less necessary
labour, means more surplus labour out of the same day, without anyone working a minute longer.

## Read first

- `docs/plans/README.md`, decisions 9 and 10
- `docs/plans/00-scenario-data.md`, "Subsistence" and "Figures the tests can check"
- `surplus.Domain/Simulation/Production/Branch.cs` — `NecessaryLabor`, `CostsToKeepAHand`,
  `RejectImpossibleDay`
- `surplus.Domain/Simulation/Countries/Province.cs` — `Grow`, `Decline`,
  `DismissThoseWhoAreNoLongerHere`
- `surplus.Domain/Simulation/Society/ClassComposition.cs`, `ClassProfile.cs` (`AppropriatesSurplus`)
- `World.Elapse` as left by plan 05

## Changes

### `surplus.Domain/Simulation/Society/Subsistence.cs`

A `sealed record Subsistence`: what one person must have in one day to be there the next. Content
equality.

```csharp
public IReadOnlyList<Stock> PerHead { get; }

public static Subsistence Of(params Stock[] perHead);  // empty, or a commodity twice: DomainException
public IReadOnlyList<Stock> For(int heads);            // the basket so many times over
```

The doc comment should note that the basket is customary, not physiological ("a historical and moral
element"), which is why it is data and not a constant.

### `World.cs`

- New property `Subsistence Subsistence`; `Assemble` and `Load` gain the parameter; `WorldBuilder`
  gets `WithSubsistence`. `Assemble` rejects a basket holding a commodity that is not registered or
  whose department is not `Department.MeansOfConsumption`.
- `public LaborTime CostOfSubsistence()` — the value of one basket at today's values
  (`Register.ValueOf(Subsistence.PerHead).Magnitude`).
- New thin command, so that callers never have to supply necessary labour themselves:

```csharp
public void OpenBranch(
  CountryId country, ProvinceId province, Technique technique, SocialClass workforce, int hands,
  LaborTime workingDay);
```

  It calls `Country.OpenBranch` with necessary labour set to the lesser of `CostOfSubsistence()` and
  `workingDay`.

### `Country.cs`

```csharp
/// Gives everyone their day's subsistence, so far as the store can. Returns the share provided, 0 to 1.
public decimal Provision(Subsistence subsistence);

/// Those who went without die of it, a few each day. Returns how many.
public int LoseTheUnprovided(decimal provided);

/// A fed people multiplies. Returns by how many.
public int Multiply();
```

**Provision.** Need is `subsistence.For(Population)`. The share provided is the smallest, over the
basket's goods, of `held / needed`, capped at 1; a country of nobody is fully provided. Draw that
share of every good in the basket (so a people short of cloth also eats less corn: the basket is taken
whole or in proportion, never good by good).

**LoseTheUnprovided.** When `provided < 1`: for every province and every class present there that does
**not** appropriate surplus, deaths are `floor(heads × (1 − provided) × 0.01)`. Apply with
`Province.Decline` where the figure is above zero. Those who live on others' labour do not starve
first, and the model says so by leaving them out. Keep the 1% as a named constant with a comment.

**Multiply.** For every province and every class present: growth is `floor(heads × 0.001)`. Apply with
`Province.Grow` where above zero. Named constant.

### `World.Elapse` — the full order of a day

1. Every country works (`WorkOneDay`).
2. Commodities are revalued.
3. Necessary labour is reset: for every branch in the world,
   `branch.CostsToKeepAHand(lesser of CostOfSubsistence() and branch.WorkingDay)`. The cap matters:
   `CostsToKeepAHand` rightly refuses a day too short to reproduce the labourer, and a famine in
   value terms must not crash the day.
4. Every country provisions its people; where the share is below 1, it loses the unprovided.
5. On the first day of a month (`today.Date.Day == 1`): every country that was fully provided today
   multiplies.

Occurrences:

- `Chronicle/PeopleWentWithout.cs`: `(string Country, decimal Provided, int Deaths)`, e.g.
  *"Russia could provide only 92% of its people's subsistence; 144 died."*
- `Chronicle/PopulationGrew.cs`: `(string Country, int Heads)`, e.g. *"The United Kingdom grew by 198
  souls."* Not emitted when the figure is zero.

### Scenario and console

- `EighteenThirtySix.Build()` passes the basket from the data file to `World.Assemble`.
- `Session` needs no change.
- Console `status` adds, per country, days of subsistence in store (the smallest, over the basket's
  goods, of `held / daily need`). `provinces` shows each branch's necessary labour and rate of
  surplus-value (`ValueComposition.RateOfSurplusValue`, guarded: it throws when v is none).

## Tests

`Simulation/Society/SubsistenceTests.cs`

- `A_basket_with_nothing_in_it_keeps_no_one_alive`
- `The_same_thing_cannot_be_in_the_basket_twice`
- `So_many_heads_need_the_basket_so_many_times_over`

`Simulation/Countries/CountryTests.cs` (extend)

- `A_provided_people_takes_its_subsistence_out_of_the_store`
- `A_people_short_of_one_thing_goes_short_of_everything_in_proportion`
- `A_country_of_no_one_wants_for_nothing`
- `Those_who_went_without_die_a_few_each_day`
- `Those_who_live_on_the_labour_of_others_do_not_starve_first`
- `The_dead_are_struck_off_the_branches_they_worked_in`
  (deaths exceed the idle of a class: the branch's hands fall)
- `A_fully_provided_people_loses_no_one`
- `A_people_multiplies_class_by_class`
- `A_class_too_small_to_multiply_stays_as_it_is`

`Simulation/WorldTests.cs` (extend)

- `The_basket_can_only_hold_articles_of_consumption`
- `Necessary_labour_is_the_labour_contained_in_the_basket`
- `Cheaper_subsistence_shortens_necessary_labour_and_lengthens_surplus_labour`
  (make corn cheaper by re-equipping its branch; next day `NecessaryLabor` is lower on a branch that
  produces something else entirely)
- `Necessary_labour_never_exceeds_the_working_day`
- `A_people_grows_only_on_the_first_of_the_month`
- `A_people_that_went_without_today_does_not_grow`
- `A_famine_is_reported` / `Growth_is_reported`
- `A_branch_opened_through_the_world_pays_what_subsistence_costs`

`surplus.Application.Tests/Scenarios/EighteenThirtySixTests.cs` (extend)

- `After_one_day_necessary_labour_is_what_the_scenario_predicts` (4.4701h on every branch)
- `Nobody_goes_without_in_a_year_left_alone` (365 days, no `PeopleWentWithout`, and still no
  `BranchStoodIdle`)
- `Every_country_is_more_populous_after_a_year`

Revisit `Each_country_produces_what_the_scenario_says_in_a_day` from plan 04: Corn and Cloth are now
consumed, so the store delta is production less the population's subsistence.

## Out of scope

Different baskets per class, luxury consumption, wages, migration, birth and death rates that differ
by class or mode. Do not make the unprovided transform into another class; they die.

## Done when

Build and tests are clean, and a year of the scenario left alone ends with more people than it began
with and nobody having gone without.
