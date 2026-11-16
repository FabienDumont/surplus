# 10 — Save and load

## Goal

Let a game outlive the process. The domain was built for this from the start: every aggregate has a
`Load` factory that takes a snapshot and asserts nothing. What is missing is the snapshot itself.
After this plan a session can be written to a JSON string and read back into a session that behaves
identically from then on.

## Read first

- `docs/plans/README.md`, "Determinism"
- Every `Load` factory and every value-object factory in the domain; the snapshot must be able to
  feed all of them. In particular: `Game.Load`, `World.Load`, `Country.Load`, `Province.Load`,
  `Branch.Load`, `War.Load`, `Commodity.Load`, `CommodityRegister.Of`, `Store.Of`, `Technique.Of`,
  `Subsistence.Of`, `Government.Of`, `ClassComposition.Of`, `ClassPresence.Of`,
  `MeansOfProduction.SetInMotion`, `Stock.Of`, `Quantity.Of`, `UnitOfMeasure.Of`, `UseValue.Of`,
  `Value.CrystallisedFrom`, `LaborTime.FromHours`, `Peace.White` / `Ceding` / `Annexation`,
  `Id<T>.From`, `GameDate.Of`
- `surplus.Application/Session.cs` — `Resume(game, world)`

## Changes

### `surplus.Application/Saving/`

- `SaveFile.cs`:

```csharp
public static class SaveFile
{
  public static string Write(Session session);
  public static Session Read(string json);
}
```

  No file IO here; the console owns paths and files. `Read` throws `DomainException` with a readable
  message for JSON that is malformed, of an unknown version, or missing something required.

- `Snapshots.cs` (or one file per snapshot type if it grows): plain `record` DTOs with primitive
  properties only (`Guid`, `string`, `int`, `decimal`, enums as strings, arrays). One DTO per domain
  type that has state. A root `SessionSnapshot` carries `int Version` (start at `1`), the game and the
  world.

- Mapping in both directions lives next to the DTOs, as plain methods. No reflection over domain
  types, no serializer attributes on domain types, no change to the domain to make it "serializable".

Use `System.Text.Json` (already in the framework; no package). Options: indented, enums as strings,
invariant number formatting (the default).

Rules:

- **Everything that has state is saved**, including concluded wars and their peace, techniques, the
  subsistence basket, each branch's current means, hands, working day, necessary labour and output per
  hour, and each commodity's current value.
- **Order is preserved wherever order can change an outcome**: countries, provinces within a country,
  branches within a province, class presences within a composition, wars, techniques, instruments and
  material within means. Store contents and register contents are written sorted by commodity id so
  the same state always produces the same text.
- **Loading goes through `Load` factories for entities** (no invariant re-checked, as their comments
  say) and through the ordinary factories for value objects.
- If a domain type turns out to lack a way to be rebuilt exactly (for example a value object whose
  only factory rejects a legitimately saved state), add a `Load` factory to it in the domain in the
  house style and say so in the report.

### Console

| Command        | Effect                                                              |
|----------------|---------------------------------------------------------------------|
| `save <path>`  | Write the session to the file, overwriting; confirm with one line   |
| `load <path>`  | Replace the current session with the one in the file; print status  |

A missing file or an unreadable one prints one line and leaves the current session untouched.

## Tests — `surplus.Application.Tests/Saving/SaveFileTests.cs`

- `A_saved_session_is_read_back_exactly_as_it_was_left`
  (the 1836 scenario advanced 45 days; `Write(Read(Write(s)))` equals `Write(s)` as text)
- `A_session_read_back_goes_on_exactly_as_the_original_would_have`
  (advance the original and the copy 400 more days each; their `Write` outputs are equal as text.
  This is also the determinism test for the whole simulation.)
- `A_saved_session_keeps_its_date_and_its_name`
- `A_war_in_progress_survives_the_save` (momentum intact, still open)
- `A_concluded_war_survives_the_save_with_its_peace` (one test per kind of peace)
- `A_reequipped_branch_survives_the_save` (means, output per hour)
- `An_annexed_country_stays_gone_after_the_save`
- `A_session_in_which_no_one_is_played_survives_the_save`
- `The_same_state_is_always_written_the_same_way` (two `Write` calls on one session are equal)
- `What_is_not_a_save_file_cannot_be_read` (garbage, empty string, valid JSON of the wrong shape)
- `A_save_file_of_an_unknown_version_cannot_be_read`

## Smoke test

```
"next 40`nsave $env:TEMP\surplus-smoke.json`nnext 10`nload $env:TEMP\surplus-smoke.json`nstatus`nquit" | dotnet run --project surplus.Console
```

The final `status` must show 1836-02-10 (forty days after the start), not 1836-02-20.

## Out of scope

Autosave, save slots, a save directory, compression, migration between versions (there is only one),
a database. The `Version` field exists so that a later change can refuse old files cleanly, and for
nothing else yet.

## Done when

Build and tests are clean, the smoke test behaves as described, and the two round-trip tests pass
without any tolerance: text equality, not approximate equality.
