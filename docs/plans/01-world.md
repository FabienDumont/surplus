# 01 — World

## Goal

Give the simulation a root. Today countries, the commodity register and wars are separate aggregates
that nothing holds together, so nothing can act on "everything". After this plan there is a `World`
that owns them and a single entry point, `World.Elapse`, that later plans fill with what a day does.

This plan adds structure only. `Elapse` does nothing yet beyond returning an empty report.

## Read first

- `docs/plans/README.md` (house style and decisions)
- `surplus.Domain/GameEngine/Game.cs`, `GameDate.cs` — note the comment: the game knows nothing of
  the simulation
- `surplus.Domain/Simulation/Countries/Country.cs`, `Province.cs`
- `surplus.Domain/Simulation/Commodities/CommodityRegister.cs`
- `surplus.Domain/Simulation/Warfare/War.cs`
- `surplus.Testing/CountryBuilder.cs`, `ProvinceBuilder.cs`, `WarBuilder.cs`
- `surplus.Domain.Tests/Simulation/Countries/CountryTests.cs` for test style

## Changes

### `surplus.Domain/Simulation/World.cs` — namespace `Surplus.Domain.Simulation`

A `sealed class World`. It is the aggregate root of the simulation side, as `Game` is of the engine
side.

```csharp
public CommodityRegister Register { get; }
public IReadOnlyList<Country> Countries { get; }
public IReadOnlyList<War> Wars { get; }

/// The country the human sits in, or null when the human's state has ceased to exist.
public Country? Played { get; }

public static World Assemble(CommodityRegister register, IEnumerable<Country> countries);
public static World Load(CommodityRegister register, IEnumerable<Country> countries, IEnumerable<War> wars);

public Country Country(CountryId id);                 // throws DomainException if unknown
public Country CountryGoverning(ProvinceId province); // throws DomainException if no one governs it

public DayReport Elapse(GameDate today);
```

`Assemble` rules, each a `DomainException`:

- a world holds at least one country;
- no country appears twice (by id);
- no province is governed by two countries (by id);
- at most one country is played by the human.

`Load` asserts nothing. `Elapse` returns `new DayReport(today, [])` for now.

The simulation may depend on `GameDate` (it needs the calendar). `Game` must still not depend on
anything under `Simulation`.

### `surplus.Domain/Simulation/Chronicle/` — namespace `Surplus.Domain.Simulation.Chronicle`

- `Occurrence.cs`: `public abstract record Occurrence;` Something that happened on a day and that a
  player would want to be told. Later plans add sealed subtypes, each overriding `ToString()` with
  the sentence a player reads. Occurrences carry display names (strings) and figures, not ids.
- `DayReport.cs`: `public sealed record DayReport(GameDate Date, IReadOnlyList<Occurrence> Occurrences)`
  with content equality over the list, and a convenience `IEnumerable<T> Of<T>() where T : Occurrence`.

### `surplus.Testing/WorldBuilder.cs`

Builds a `World` through `World.Load`. Defaults: a register with one default `CommodityBuilder`
commodity, one default `CountryBuilder` country, no wars. Methods: `WithRegister`, `WithCountries`,
`WithWars`. Every later plan that adds something to `World` adds a `With...` here, and every test that
needs a world goes through this builder, so that a new constructor parameter costs one line.

## Tests — `surplus.Domain.Tests/Simulation/WorldTests.cs`

- `A_world_holds_the_countries_and_commodities_it_was_assembled_from`
- `A_world_with_no_country_in_it_is_no_world`
- `The_same_country_cannot_be_in_a_world_twice`
- `A_province_cannot_be_governed_by_two_countries_at_once`
- `Only_one_country_can_be_played_by_the_human`
- `A_world_knows_which_country_the_human_plays` (and null when none is played)
- `A_country_is_found_by_its_identity` / `An_unknown_country_is_not_found` (throws)
- `A_world_knows_who_governs_a_province` / throws when no one does
- `A_saved_world_is_reloaded_exactly_as_it_was_left`
- `A_day_on_which_nothing_happens_is_reported_as_such` (`Elapse` returns the date and no occurrences)

## Out of scope

Declaring wars, anything economic, the application layer. `World` gets no method that mutates a
country in this plan.

## Done when

Build and tests are clean, the 306 existing tests still pass, and `World` appears nowhere outside the
domain and its tests.
