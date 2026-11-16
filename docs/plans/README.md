# Plans: from a domain vocabulary to the basis of a game

These plans take `surplus` from its current state (a tested domain model that nothing sets in motion)
to a playable basis: a world that ticks day by day, an economy that closes its loop, wars that move
borders, a console to play from, an AI in the other seats, and save files.

Each plan is written to be executed on its own by an implementer who has not seen the others being
written. Execute them **in order**: each one builds on the types the previous ones introduce.

| #  | Plan                                                   | What exists afterwards                                         |
|----|--------------------------------------------------------|----------------------------------------------------------------|
| 00 | [Scenario data](00-scenario-data.md)                   | Reference numbers only, no code. Read when a plan points to it |
| 01 | [World](01-world.md)                                   | One aggregate holding countries, commodities, wars; `Elapse`   |
| 02 | [Stores and production](02-stores-and-production.md)   | Products are kept; inputs are physically consumed              |
| 03 | [Techniques and accumulation](03-techniques-and-accumulation.md) | Branches are opened, staffed and re-equipped out of the store |
| 04 | [First playable](04-first-playable.md)                 | Application layer, the 1836 scenario, a read-only console      |
| 05 | [Social value](05-social-value.md)                     | Values are settled daily from what production actually cost    |
| 06 | [Subsistence and population](06-subsistence-and-population.md) | People eat, starve, grow; necessary labour is derived     |
| 07 | [Player orders](07-player-orders.md)                   | The player runs their economy and state; time passes in digests that stop on trouble |
| 08 | [War](08-war.md)                                       | Wars are fought from class structure and redraw the map; the player can wage them |
| 09 | [Revenues and subsistence](09-revenues-and-subsistence.md) | Each class eats through its own revenue; the unemployed have no title; poor relief, pauperism |
| 10 | [Circulation](10-circulation.md)                       | Stores per province; goods cross borders at the pace the mode allows, carried by carters |
| 11 | [Known techniques](11-known-techniques.md)             | Each country knows some techniques and finds out others by working |
| 12 | [AI](12-ai.md)                                         | AI countries provision themselves and end the wars they win    |
| 13 | [Save and load](13-save-and-load.md)                   | A game survives the process                                    |

After plan 04 the game runs, and after plan 07 it can be played. Plans 05 to 13 each add one thing
to a running game.

### Planned, not yet written

- **Goals and pressure.** Even with orders, a scenario that feeds itself at rest gives the player no
  reason to act. Two things are missing. One is objectives or a score, read off the model and not
  bolted on: accumulation, the rate of surplus-value, the share of the world's production. The other
  is a scenario that is not balanced at rest, for example a Britain whose people outgrow its corn
  unless it acts. Design this after plan 07, once orders exist to answer the pressure with.
- **A real interface.** A full-screen terminal interface or a small web page instead of scrolling
  text, once there is something worth playing in it.
- **Earlier start dates.** Scenarios set in the 1st or 11th century, with their own commodities,
  techniques and starting knowledge per country. This needs a decision on the time scale, because a
  day per period makes centuries unplayable. The options are a coarser period (a season or a year)
  or a speed control. It builds on plan 11 and should follow plan 13, so that saves already carry
  what a country knows.

## Baseline

At the time of writing: `dotnet test surplus.slnx` passes 306 tests, all in `surplus.Domain.Tests`.
`surplus.Application` holds an empty `Class1`, `surplus.Console` prints `Hello, World!`, and neither
references the domain. Target framework is `net10.0`; tests use xUnit 2.9.

## How to execute a plan

1. Read the plan top to bottom, then read every file listed under **Read first**. The existing code is
   the authority on style and on how the domain already behaves.
2. Implement the **Changes**, obeying the **Rules**. Signatures given in a plan are the contract;
   private helpers, local names and file-internal structure are yours to choose.
3. Write the **Tests** listed. They are the minimum; add more where a rule has an edge the list missed.
4. Run `dotnet build surplus.slnx` and `dotnet test surplus.slnx`. Both must be clean: zero warnings
   introduced, zero failures, including all tests that existed before.
5. Report what was done, anything the plan got wrong about the code, and any decision you had to make
   that the plan did not cover. Do not commit unless asked.

If a plan contradicts the code as it stands (a signature that differs, a method that does not exist),
trust the code, adapt, and say so in the report. If a plan asks for something that would break a rule
the domain already enforces, stop and report instead of removing the rule.

## House style (non-negotiable, the codebase is uniform)

- **Formatting.** 2-space indentation. Braces on every `if` body. Inside every type, members are grouped
  in `#region Fields`, `#region Properties`, `#region Ctors`, `#region Methods` (in that order, omitting
  empty ones). Enums and marker types get no regions. File-scoped namespaces, one type per file,
  folder path equals namespace.
- **Construction.** Constructors are private. Types are created through named static factories that
  speak the domain (`Country.Found`, `Province.Establish`, `Branch.Open`, `War.Declare`,
  `Stock.Of`). Entities and aggregates also get a `Load(...)` factory that takes a full snapshot and
  asserts **no** invariant; it exists for persistence and for test builders.
- **Failure.** A broken domain rule throws `DomainException` (in `Surplus.Domain.SharedKernel`) with
  a full-sentence message. Never `ArgumentException`, never a `bool` return for a rule violation.
- **Immutability.** Value objects are `sealed record` with get-only properties; operations return a
  new instance and are named as past participles (`Grown`, `Declined`, `Enacting`, `Accumulated`).
  A record that holds a list overrides `Equals` and `GetHashCode` to compare by content (see
  `ClassComposition`, `Government`, `Peace`). Entities are `sealed class` with `private set`.
- **Identifiers.** Strong ids are `Id<TTag>` with a global alias (`CountryId`, `ProvinceId`, ...)
  declared in `surplus.Domain/SharedKernel/StrongIds/`. Projects outside the domain get the aliases by
  linking `StrongIds.GlobalUsings.cs` in their `.csproj` (see `surplus.Testing.csproj`).
- **Numbers.** Quantities, hours and rates are `decimal`. Head counts are `int`. Never `double`.
- **Doc comments.** Every public type and every non-obvious public member carries a `/// <summary>`
  that says *why the model is shaped this way*, usually by naming the piece of Marx's theory it
  encodes, in the dry voice the existing comments use. Read `Branch.cs`, `Yield.cs` and `Province.cs`
  before writing one. A comment that only restates the member name is worse than none.
- **Tests.** One test class per type, mirroring the source path. Test names are full sentences with
  underscores, stating the rule and not the method:
  `A_country_without_territory_is_no_country`, not `Cede_Throws_WhenLastProvince`. Arrange with the
  builders in `surplus.Testing` (add or extend a builder whenever a new type or property needs one;
  builders call `Load` so tests can start from any state). No mocking library, no new NuGet packages.
- **Determinism.** No `Random`, no `DateTime.Now`, no dependence on dictionary enumeration order where
  the order can change an outcome. The same world advanced the same number of days must end in the
  same state, always. Plan 13 tests this.

## Design decisions already made

These were settled when the plans were written. Do not reopen them while implementing; if one proves
unworkable, report it.

1. **One period is one day.** `Branch.Work` is called once per elapsed day; `Wear` is a daily rate.
2. **The world is the simulation's root.** `Game` stays ignorant of the simulation. `World` holds
   everything else and exposes `Elapse(GameDate)`. The application layer advances the clock and then
   tells the world a day has passed.
3. **Stores are national** until plan 10. A country holds one `Store`. Provinces produce into it
   and draw from it. From plan 10 each province keeps its own, and goods cross borders inside a
   country by carriage. There is **no trade between countries**: each country lives on what it makes.
4. **Value is world-wide.** The single `CommodityRegister` stays single. A commodity has one value,
   the average over every branch in the world that produced it that day.
5. **Inputs are all-or-nothing.** A branch that cannot draw its whole day's material from the store
   stands idle that day and produces nothing.
6. **Instruments are replaced in kind.** Each working day a branch draws from the store the worn share
   of its instruments as well as its raw material. Instruments once installed never return to the
   store: closing a branch or laying hands off scraps them.
7. **Means scale with hands.** A branch's means of production are proportional to the hands it
   employs. Taking on hands requires the extra instruments from the store.
8. **Techniques are a catalogue.** How much a branch yields per hour is never typed in by a player.
   It comes from a `Technique` in the world's catalogue, and from plan 11 on only from one the
   country knows. Techniques are found by labour done in the trades they grow out of, never by a
   research budget.
9. **Everyone takes the same basket.** One subsistence basket per head per day, the same for every
   class. From plan 09 a head eats only by a title its revenue gives it, so a worker without a wage
   has none. The surplus product is whatever is left in the store.
10. **Months matter.** Fighting and population growth happen on the first day of each month.
    Everything else happens daily.
11. **Reports are sentences.** `World.Elapse` returns a `DayReport` listing `Occurrence` records whose
    `ToString()` is the line a player reads.

## Known simplifications (leave them alone)

These are deliberate gaps. Do not fix them in passing; they are the work that follows the basis.

- A world-wide value is not consistent with goods that cross no border between countries: a market
  value presupposes a market. Trade between countries is the first thing to build after these plans.
- Carriage adds labour, but its value is not added to the goods it carried. Corn in Lancashire is
  worth what corn in Kent is worth, because value stays world-wide and local value needs a market.
- The surplus product lands in a common store, and the appropriators are only served first from it.
  There is no money, and wages and profit are titles to the basket, not sums paid. Every class takes
  the same basket, so the appropriators consume no luxuries.
- A wage feeds one head. Families and dependants are not modelled.
- Lengthening the working day raises output without raising the material consumed, because a branch's
  material is fixed per hand per day.
- A province holds one branch per commodity with one workforce class, so a province cannot run a
  handicraft and a factory in the same trade side by side.
- Either belligerent may take a white peace at any time, including the one that is losing.
- Knowledge of a technique moves between countries only by conquest. The only thing that drives
  development is the mode of production, applied as a flat weight. Pressures within a mode, such as
  labour shortages or the rate of profit, do not speed it up.
- Class structure changes only through laws, starvation and growth. The ruin of petty producers by
  cheaper competitors, the reason the `Yield` comments give for the whole movement, is not simulated.
