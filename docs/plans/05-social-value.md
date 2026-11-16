# 05 — Social value

## Goal

Let value follow production. Today a commodity's value is whatever it was declared at, and
`Commodity.Revalue` is never called. After this plan each day's production settles what each commodity
is worth: the labour society actually needed, on average, to make it that day.

This is the mechanism the comments on `Yield` and `Commodity` already describe. A branch that produces
below the average is the one that pulls the average down; nothing else in the model can.

## Read first

- `docs/plans/README.md`, decision 4
- `surplus.Domain/Simulation/Production/Yield.cs`, `ValueComposition.cs`
- `surplus.Domain/Simulation/Commodities/Commodity.cs` (`Revalue`), `CommodityRegister.cs`, `Value.cs`
- `surplus.Domain/Simulation/Production/LaborTime.cs`
- `World.Elapse` as left by plan 02 (it already gathers the day's yields)
- `docs/plans/00-scenario-data.md`, "Values after the first day of production"

## Changes

### `CommodityRegister.cs`

```csharp
/// Settles what each commodity is worth from what it cost to produce this period.
public IReadOnlyList<Revaluation> Revalue(IEnumerable<Yield> yields);
```

For each commodity that appears in the yields:

```
socially necessary labour time per unit =
    sum of yield.Composition.Product.Magnitude over its yields
  / sum of yield.Product.Quantity.Amount over its yields
```

That is the total labour, dead and living, that went into the whole mass, spread over the whole mass:
an average weighted by output, so that a large producer weighs more than a small one. Apply it with
`Commodity.Revalue`.

Rules:

- A commodity nobody produced this period keeps its value.
- All averages are computed before any commodity is revalued. The values that entered today's yields
  (through the constant capital) were yesterday's, and every commodity must be measured against the
  same yesterday.
- Returns one `Revaluation` per commodity whose value was set, whether or not it changed.

### `surplus.Domain/Simulation/Commodities/Revaluation.cs`

`public sealed record Revaluation(CommodityId Commodity, Value Before, Value After)` with a
`decimal RelativeChange` (positive when value rose; `After` against `Before`).

### `World.Elapse`

After every country has worked, call `Register.Revalue` with all the day's yields. For each
revaluation whose value moved by at least 1% in either direction, add:

`Chronicle/CommodityWasRevalued.cs`: `sealed record CommodityWasRevalued(string Commodity, Value
Before, Value After) : Occurrence`, reading e.g. *"Cloth now holds 1.59h of labour per m, down from
2.42h."* (use the commodity's unit name; round to two decimals in the sentence only).

Smaller movements are applied but not reported.

Order inside `Elapse` is now: production in every country, then revaluation.

## Tests

`Simulation/Commodities/CommodityRegisterTests.cs` (extend)

- `A_commodity_is_worth_what_its_production_cost_on_average`
  (two yields of one commodity with different individual values and different sizes; assert the
  weighted, not the simple, average)
- `The_larger_producer_weighs_more_in_the_average`
- `A_commodity_nobody_produced_keeps_its_value`
- `Dead_labour_counts_toward_what_a_thing_is_worth` (a yield with constant capital)
- `Every_commodity_is_measured_against_the_same_yesterday`
  (A is an input of B; both produced; B's new value reflects A's old value, not A's new one)
- `A_revaluation_reports_what_the_value_was_and_what_it_became`

`Simulation/WorldTests.cs` (extend)

- `A_day_of_production_settles_what_commodities_are_worth`
- `A_value_that_barely_moved_is_not_reported`
- `A_cheaper_way_of_producing_lowers_the_value_for_everyone`
  (two countries produce cloth by hand; one re-equips with power looms via `Country.Reequip`; after
  the next day the register's cloth value is lower, and the hand weaver's `Yield.IndividualValue` is
  now above it)

`surplus.Application.Tests/Scenarios/EighteenThirtySixTests.cs` (extend)

- `After_one_day_each_commodity_is_worth_what_the_scenario_predicts` (the table in the data file, four
  decimals)
- `Values_hold_steady_while_nothing_changes` (day 2 and day 30 equal day 1, four decimals)

## Console

`values` already prints current values; no change needed beyond the new occurrence lines showing up
in `next`.

## Out of scope

Prices, money, prices of production, equalisation of profit rates. Necessary labour (plan 06). Any
consequence for producers above the average (see "Known simplifications" in the README).

## Done when

Build and tests are clean and the scenario's values after one day match the data file.
