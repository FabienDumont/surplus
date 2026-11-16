# 04 — First playable

## Goal

Run the game. After this plan `dotnet run --project surplus.Console` starts the 1836 scenario and lets
a person advance the calendar and look at the world. Nothing can be commanded yet (plan 07); this is
the moment the model first moves in front of someone.

Three pieces: the application layer that joins the clock to the world, the scenario, and a read-only
console.

## Read first

- `docs/plans/README.md`
- `docs/plans/00-scenario-data.md` — every number in the scenario comes from there
- `surplus.Domain/GameEngine/Game.cs` — `Resume`, `Pause`, `AdvanceOneDay` and what each refuses
- `surplus.Domain/Simulation/World.cs` as left by plans 01 to 03
- `surplus.Testing/surplus.Testing.csproj` — how a project links `StrongIds.GlobalUsings.cs`
- All `.csproj` files and `surplus.slnx`

## Changes

### Project wiring

- `surplus.Application.csproj`: reference `surplus.Domain`; link `StrongIds.GlobalUsings.cs`.
- `surplus.Application.Tests.csproj`: reference `surplus.Application`, `surplus.Domain`,
  `surplus.Testing`; link `StrongIds.GlobalUsings.cs`.
- `surplus.Console.csproj`: reference `surplus.Application`; link `StrongIds.GlobalUsings.cs`.
- Delete `surplus.Application/Class1.cs`.

### `surplus.Application/Session.cs` — namespace `Surplus.Application`

A `sealed class Session`: one game being played, the clock and the world together. This is the
orchestration `Game`'s own comment assigns to the application layer.

```csharp
public Game Game { get; }
public World World { get; }

public static Session Begin(Scenario scenario);        // Game.Start(scenario.Name, scenario.StartDate)
public static Session Resume(Game game, World world);  // for loading, plan 11

/// Lets so many days pass and reports each of them.
public IReadOnlyList<DayReport> Advance(int days);
```

`Advance` rejects fewer than one day (`DomainException`). It resumes the game, then for each day calls
`Game.AdvanceOneDay()` followed by `World.Elapse(Game.CurrentDate)`, then pauses the game again, so a
session is always paused between calls. If the game was somehow already running, do not call `Resume`
a second time.

### `surplus.Application/Scenarios/`

- `Scenario.cs`: `public sealed record Scenario(string Name, GameDate StartDate, World World)`.
- `EighteenThirtySix.cs`: `public static class EighteenThirtySix` with `public static Scenario
  Build()`. Builds exactly what `00-scenario-data.md` describes, **through the domain's real
  factories** so every invariant is exercised: `Commodity.Produce`, `CommodityRegister.Of`,
  `Technique.Of`, `Province.Establish`, `Branch.Open` plus `Province.Open`, `Country.Found` plus
  `Annex` for the second province, `Country.LayUp` for the stores, `World.Assemble`.

  Open the starting branches directly with `Branch.Open(..., technique.EquipFor(hands), ...)` and
  `Province.Open`, not with `Country.OpenBranch`: their instruments are already installed when the
  game begins and are not taken from the starting store.

  The subsistence basket in the data file belongs to plan 06; leave it out here.

  Keep the file readable as data: one small private method per country, the numbers written with
  digit separators (`60_000`), no clever loops.

### `surplus.Console`

Replace `Program.cs` with a command loop. Keep `Program.cs` to composition only; put the loop in a
class that reads from a `TextReader` and writes to a `TextWriter` so it can be driven by a test or a
pipe.

Commands for this plan:

| Command        | Effect                                                                              |
|----------------|-------------------------------------------------------------------------------------|
| `next [n]`     | Advance `n` days (default 1). Print each occurrence, then the new date              |
| `status`       | Date; each country with control, government, population, dominant mode              |
| `provinces`    | The played country's provinces: mode, classes with heads, branches with hands and what stands idle of each employed class |
| `stores`       | The played country's store, one line per commodity, with its value in hours         |
| `values`       | Every commodity and its current value                                               |
| `help`         | The command list                                                                    |
| `quit`         | Leave                                                                               |

Rules:

- Print the status once at start, then prompt with the current date, e.g. `1836-01-01> `.
- Format every number with `CultureInfo.InvariantCulture`, thousands grouped, at most four decimals.
- An unknown command or a bad argument prints one line and prompts again. A `DomainException` prints
  its message and prompts again. Nothing else is caught.
- End of input is the same as `quit`.
- When many days are advanced at once, print occurrences as they come but never more than one line
  per occurrence. No blank screens, no colours, no cursor control.

## Tests

`surplus.Application.Tests/SessionTests.cs`

- `A_session_begins_paused_on_the_scenarios_start_date`
- `Advancing_moves_the_calendar_by_as_many_days_as_asked`
- `Every_day_advanced_is_reported_in_order`
- `A_session_is_left_paused_after_advancing`
- `Fewer_than_one_day_cannot_be_advanced`

`surplus.Application.Tests/Scenarios/EighteenThirtySixTests.cs` — check against the "Figures the tests
can check" section of the data file:

- `The_scenario_opens_on_the_first_of_january_1836`
- `The_human_plays_the_united_kingdom_and_the_machine_the_rest`
- `Each_country_has_the_population_the_scenario_gives_it`
- `Each_province_stands_in_the_mode_its_classes_put_it_in`
- `Each_country_produces_what_the_scenario_says_in_a_day` (advance one day; compare store deltas of
  goods nobody consumes yet, i.e. Corn and Cloth rise by exactly the day's production, Fibre, Iron and
  Machinery by production less productive consumption)
- `No_branch_stands_idle_in_a_year_left_alone` (advance 365 days; no `BranchStoodIdle`)

## Smoke test

```
"status`nnext 30`nstores`nprovinces`nvalues`nquit" | dotnet run --project surplus.Console
```

Must exit with code 0, show 1836-01-31 after `next 30`, and show stores larger than the scenario's
starting figures for Corn and Cloth.

## Out of scope

Commands that change anything (plans 07 and 08). The AI (plan 10). Saving (plan 11). A test project for the
console.

## Done when

Build and tests are clean and the smoke test behaves as described.
