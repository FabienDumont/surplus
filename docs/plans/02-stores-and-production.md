# 02 — Stores and production

## Goal

Make production leave something behind and cost something. Today `Province.Work` returns `Yield`s
that nobody keeps, and a branch's material and instruments are counted in value but never physically
taken from anywhere. After this plan every country has a store; a working day draws its material and
its worn instruments from that store, and lays its product up in it.

## Read first

- `docs/plans/README.md`, decisions 3, 5 and 6
- `surplus.Domain/Simulation/Commodities/Stock.cs`, `CommodityRegister.cs`
- `surplus.Domain/Simulation/Production/MeansOfProduction.cs`, `Branch.cs`, `Yield.cs`, `Quantity.cs`
- `surplus.Domain/Simulation/Countries/Country.cs`, `Province.cs`
- `surplus.Domain/Simulation/Society/ClassComposition.cs` — the model for an immutable record that
  holds a list and compares by content

## Changes

### `surplus.Domain/Simulation/Commodities/Store.cs`

A `sealed record Store`: everything a country has laid up, at most one `Stock` per commodity.
Immutable, like `Stock` itself; equality by content, independent of order.

```csharp
public static readonly Store Empty;
public static Store Of(params Stock[] stocks);      // the same commodity twice is a DomainException

public IReadOnlyCollection<Stock> Stocks { get; }   // exhausted stocks are not listed
public decimal AmountOf(CommodityId commodity);     // 0 when none is held
public bool Holds(IEnumerable<Stock> required);     // sums requirements for the same commodity first

public Store LaidUp(Stock stock);
public Store LaidUp(IEnumerable<Stock> stocks);
public Store Drawn(IEnumerable<Stock> stocks);      // DomainException if anything is lacking
```

`Drawn` either takes everything asked or throws having taken nothing. Unit mismatches surface through
`Quantity`'s own checks; do not duplicate them.

### `MeansOfProduction.cs`

```csharp
/// What one period physically uses up: the material entire, and the worn share of the instruments.
public IReadOnlyList<Stock> ConsumedInOnePeriod();
```

Subjects in full, plus each instrument at `Quantity * Wear`. Entries whose quantity comes to none are
left out. This is the physical twin of `ValueTransferred`, and the doc comment should say so.

### `Country.cs`

- New property `public Store Store { get; private set; }`.
- `Found(...)` keeps its signature and starts with `Store.Empty`.
- `Load(...)` gains a trailing `Store store` parameter.
- `public void LayUp(Stock stock)` — puts something in the store (the scenario stocks the granaries
  with it).
- New method:

```csharp
/// Works every branch of every province for one day, out of the store and into it.
public DayOfProduction WorkOneDay(CommodityRegister register);
```

Rules for `WorkOneDay`:

1. Provinces are taken in the order the country holds them, branches in the order the province holds
   them.
2. For each branch, compute `branch.Means.ConsumedInOnePeriod()`. If the store holds all of it, draw
   it and call `branch.Work(register)`. If not, the branch stands idle: nothing is drawn, nothing is
   produced.
3. Products are laid up only after every branch has had its turn, so that nothing made today is
   consumed today.

### `surplus.Domain/Simulation/Production/DayOfProduction.cs`

`public sealed record DayOfProduction(IReadOnlyList<Yield> Yields, IReadOnlyList<IdleBranch> Idle)`
and `public sealed record IdleBranch(ProvinceId Province, CommodityId Commodity)` (own file).

### `World.Elapse`

For each country in order, call `WorkOneDay(Register)`. For every idle branch add an occurrence:

`Chronicle/BranchStoodIdle.cs`: `sealed record BranchStoodIdle(string Country, string Province,
string Commodity) : Occurrence`, reading e.g. *"Lancashire's Cloth branch stood idle for want of
material (United Kingdom)."*

Keep the day's yields from all countries in a local variable; plan 05 uses them inside `Elapse`.

### `surplus.Testing`

- `CountryBuilder`: add `Holding(params Stock[] stocks)`; `Build()` passes the store to `Load`.
- Add `StoreBuilder` only if tests turn out to need one; `Store.Of` is usually enough.

## Tests

`Simulation/Commodities/StoreTests.cs`

- `An_empty_store_holds_nothing`
- `What_is_laid_up_is_added_to_what_was_already_held`
- `A_store_cannot_be_founded_on_the_same_commodity_twice`
- `Nothing_can_be_drawn_that_is_not_there` (and the store is unchanged afterwards)
- `A_store_holds_a_requirement_only_if_it_holds_all_of_it`
- `Two_requirements_for_one_commodity_are_counted_together`
- `A_stock_drawn_to_nothing_is_no_longer_listed`
- `Two_stores_holding_the_same_things_are_the_same_store`

`Simulation/Production/MeansOfProductionTests.cs` (extend)

- `A_period_uses_up_the_whole_of_the_material_and_the_worn_share_of_the_instruments`
- `Bare_hands_use_up_nothing`

`Simulation/Countries/CountryTests.cs` (extend)

- `A_country_is_founded_with_empty_stores`
- `A_days_product_is_laid_up_in_the_countrys_store`
- `A_branch_draws_its_material_from_the_store`
- `A_branch_draws_the_worn_share_of_its_instruments_from_the_store`
- `A_branch_without_its_material_stands_idle_and_takes_nothing`
- `What_is_produced_today_cannot_be_consumed_today` (branch A makes what branch B needs, store empty:
  B is idle on day one and works on day two)
- `A_saved_country_is_reloaded_with_its_stores`

`Simulation/WorldTests.cs` (extend)

- `A_day_sets_every_country_to_work`
- `A_branch_that_stood_idle_is_reported`

## Out of scope

Consumption by people (plan 06). Revaluing commodities (plan 05). Any command that changes a branch
(plan 03). Leave the existing `Province.Work(register)` as it is; it stays the store-less primitive.

## Done when

Build and tests are clean. A test world left to elapse several days accumulates product in its stores
and runs down the materials it uses.
