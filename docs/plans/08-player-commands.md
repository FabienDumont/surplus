# 08 — Player commands

## Goal

Let the human act. Everything a player can do already exists in the domain after plans 03 and 07; this
plan exposes it through `Session` and the console. No new simulation rule is introduced here. If
implementing a command seems to need one, the command is being given too much to do.

## Read first

- `docs/plans/README.md`
- `surplus.Application/Session.cs`, the console loop from plan 04
- `Country.cs`: `Enact`, `Repeal`, `TakeForm`, `Adopt`, `OpenBranch`, `CloseBranch`, `Employ`,
  `LayOff`, `Reequip`
- `Province.cs` and `Branch.cs`: `FixTheWorkingDay`
- `World.cs`: `OpenBranch`, `DeclareWar`, `MakePeace`, `WarsOf`, `Technique`, `Played`

## Changes

### `Province.cs`

```csharp
public void FixTheWorkingDay(CommodityId commodity, LaborTime workingDay);
```

Delegates to the branch (which enforces its own rule). If the new day is shorter than current
necessary labour the branch refuses; before delegating, lower necessary labour to the new day when it
would exceed it, so that a Factory Act cannot be blocked by yesterday's bookkeeping. `World.Elapse`
resets necessary labour the next day anyway.

Add the matching `Country.FixTheWorkingDay(ProvinceId, CommodityId, LaborTime)`.

### `Session.cs`

One method per command, each acting for the played country and each a one- or two-line delegation.
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

public War DeclareWarOn(CountryId defender);
public void MakePeace(WarId war, Peace peace);
```

### Console

Things are named by the numbers the listings print, never by typing names (names contain spaces).
Update the listings so every country, province, commodity, technique and open war is printed with a
stable number in brackets: countries and wars in world order, provinces in the played country's
order, commodities and techniques in register and catalogue order. Enum arguments (`Law`,
`GovernmentForm`, `Ideology`, `SocialClass`) are typed by name, case-insensitively.

| Command                                              | Effect                                             |
|------------------------------------------------------|----------------------------------------------------|
| `techniques`                                         | List the catalogue with numbers: what each produces, form, output per hour, instruments and material per hand |
| `laws`                                               | Laws in force, and those that could be enacted     |
| `enact <law>` / `repeal <law>`                       |                                                    |
| `form <governmentForm>` / `adopt <ideology>`         |                                                    |
| `open <province#> <technique#> <class> <hands> [hours]` | Open a branch; working day defaults to 12       |
| `close <province#> <commodity#>`                     |                                                    |
| `employ <province#> <commodity#> <hands>`            |                                                    |
| `layoff <province#> <commodity#> <hands>`            |                                                    |
| `reequip <province#> <commodity#> <technique#>`      |                                                    |
| `day <province#> <commodity#> <hours>`               | Fix the working day                                |
| `wars`                                               | Open wars with numbers, momentum, provinces within reach |
| `war <country#>`                                     | Declare war                                        |
| `peace <war#> white`                                 |                                                    |
| `peace <war#> cede <province name>[, <province name>...]` | Enemy provinces by name, comma-separated      |
| `peace <war#> annex`                                 |                                                    |
| `country <country#>`                                 | Show another country's provinces and stores (read-only) |

Ceded provinces are the one place names are typed, because the enemy's provinces have no number in the
player's own listing; match case-insensitively against the loser's provinces.

Rules:

- After a successful command print one line confirming what changed, in a sentence.
- A `DomainException` prints its message; the domain's messages are written to be read by a player.
- Wrong arity or an unparsable number prints the command's usage line.
- `help` lists everything, grouped: looking, economy, state, war.
- When `Played` is null, `next` and the read-only commands keep working and the prompt is prefixed
  with `(no country)`.

## Tests

`surplus.Application.Tests/SessionTests.cs` (extend) — one test per command confirming it reaches the
played country, kept short since the rules themselves are tested in the domain:

- `The_player_enacts_a_law_in_their_own_country`
- `The_player_opens_a_branch_out_of_their_own_stores`
- `The_player_takes_on_and_turns_off_hands`
- `The_player_reequips_a_branch_from_the_catalogue`
- `An_unknown_technique_cannot_be_used`
- `The_player_fixes_the_working_day`
- `The_player_declares_war_and_makes_peace`
- `A_player_whose_state_is_no_more_can_command_nothing`

`Simulation/Countries/ProvinceTests.cs` (extend)

- `A_shorter_day_is_fixed_even_when_it_falls_below_yesterdays_necessary_labour`

## Smoke test

Pipe this in and check it exits 0 with no usage errors:

```
status
techniques
provinces
employ 2 1 1000
next 31
enact EnclosureActs
provinces
war 2
next 62
wars
peace 1 white
quit
```

Adjust the numbers to whatever the listings actually print; the point is one command from each group.

## Out of scope

New rules, confirmations and undo, command history, colours, a TUI. `GiveToPlayer` / `GiveToAi`
(switching seats) stays unexposed.

## Done when

Build and tests are clean and the smoke script runs through.
