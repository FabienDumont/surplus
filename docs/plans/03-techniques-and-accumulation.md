# 03 — Techniques and accumulation

## Goal

Let a country change what it produces, and make that cost what it should. After this plan a branch can
be opened, staffed, thinned out, re-equipped and closed through the country, and every step that
needs instruments takes them from the store. This is accumulation in its plainest form: yesterday's
surplus product of Department I becomes today's instruments.

Two things have to be put right first. A branch's means do not follow its head count today, and its
productivity is a free number anyone can set.

## Read first

- `docs/plans/README.md`, decisions 6, 7 and 8
- `docs/plans/00-scenario-data.md`, the techniques table (for what a technique looks like in use)
- `surplus.Domain/Simulation/Production/Branch.cs`, `MeansOfProduction.cs`, `ProductiveForm.cs`,
  `ProductiveForms.cs`
- `surplus.Domain/Simulation/Countries/Province.cs` — `Open`, `Close`, `Employ`, `LayOff`,
  `DismissThoseWhoAreNoLongerHere`
- `surplus.Domain/Simulation/Commodities/Commodity.cs`, `Department.cs`
- What plan 02 added: `Store`, `Country.Store`, `Country.WorkOneDay`

## Changes

### `MeansOfProduction.cs`

```csharp
/// The same means for more hands or fewer: every instrument and every material in proportion.
public MeansOfProduction Scaled(decimal factor);   // negative factor is a DomainException
```

Form and wear are unchanged.

### `Branch.cs`

`Employ` and `LayOff` (both `internal`) now scale the means with the hands: after either,
`Means` is the old means scaled by `newHands / oldHands`. A branch laid off to no hands is scaled to
nothing (the province then removes it, as today).

If an existing test asserts the old behaviour (means untouched by a change in hands), update the test:
the new rule is the intended one.

### `surplus.Domain/Simulation/Production/Technique.cs`

A `sealed record Technique`: a known way of producing one commodity, stated per hand so that it fits a
branch of any size. Content equality (it holds lists).

```csharp
public string Name { get; }
public CommodityId Produces { get; }
public ProductiveForm Form { get; }
public Quantity OutputPerHour { get; }
public decimal Wear { get; }
public IReadOnlyList<Stock> InstrumentsPerHand { get; }
public IReadOnlyList<Stock> MaterialPerHand { get; }   // per hand, per day

public static Technique Of(
  string name, CommodityId produces, ProductiveForm form, Quantity outputPerHour, decimal wear,
  IEnumerable<Stock> instrumentsPerHand, IEnumerable<Stock> materialPerHand);

/// The means this technique puts in the hands of so many.
public MeansOfProduction EquipFor(int hands);
```

`Of` rejects a blank name and an output of none. `EquipFor` builds through
`MeansOfProduction.SetInMotion` (which already checks wear).

### `World.cs`

- New property `IReadOnlyList<Technique> Techniques` and lookup `Technique Technique(string name)`
  (throws if unknown). `Assemble` and `Load` gain an `IEnumerable<Technique> techniques` parameter.
- `Assemble` additionally rejects:
  - two techniques with the same name;
  - a technique producing a commodity the register does not hold;
  - a technique whose output unit is not the unit of the commodity it produces;
  - a technique whose instruments or material include a commodity that is not registered or whose
    department is not `Department.MeansOfProduction`.
- `WorldBuilder` gets `WithTechniques`.

### `Province.cs`

```csharp
/// Puts new means into the hands of the branch working this commodity.
public void Reequip(CommodityId commodity, MeansOfProduction means, Quantity outputPerHour);
```

Delegates to the existing `Branch.Reequip`.

### `Country.cs` — the commands

Every command first finds the province and throws `DomainException` if the country does not govern
it. Every command that needs instruments follows the same sequence so that a failure changes nothing:
**check the store holds them, perform the change on the province, then draw.**

```csharp
public void OpenBranch(
  ProvinceId province, Technique technique, SocialClass workforce, int hands,
  LaborTime workingDay, LaborTime necessaryLabor);

public void CloseBranch(ProvinceId province, CommodityId commodity);

public void Employ(ProvinceId province, CommodityId commodity, int hands);

public void LayOff(ProvinceId province, CommodityId commodity, int hands);

public void Reequip(ProvinceId province, CommodityId commodity, Technique technique);
```

- **OpenBranch.** Needs `technique.EquipFor(hands).Instruments` from the store. Builds the branch with
  `Branch.Open(technique.Produces, technique.EquipFor(hands), workforce, hands, workingDay,
  necessaryLabor, technique.OutputPerHour)` and hands it to `Province.Open`. Both already enforce
  their own rules (direct producers only, enough idle hands, one branch per commodity).
- **CloseBranch.** `Province.Close`. The instruments are scrapped, not returned.
- **Employ.** Needs the branch's current instruments times `hands / branch.Hands` from the store,
  then `Province.Employ`.
- **LayOff.** `Province.LayOff`. Nothing returns to the store.
- **Reequip.** The technique must produce the branch's commodity. Needs the whole of
  `technique.EquipFor(branch.Hands).Instruments` from the store; the old instruments are scrapped.
  Then `Province.Reequip(commodity, newMeans, technique.OutputPerHour)`.

When the store lacks the instruments the message should name what is missing, e.g. *"There is not
enough Machinery in store to equip 400 hands."* The country has no register to look names up in, so
either pass what is needed for the message or keep the message to quantities and units; do not add a
register dependency to `Country` for the sake of a message.

## Tests

`Simulation/Production/MeansOfProductionTests.cs` (extend)

- `Means_for_twice_the_hands_are_twice_the_means`
- `Scaling_changes_neither_the_form_nor_the_wear`

`Simulation/Production/BranchTests.cs` (extend)

- `Hands_taken_on_bring_their_share_of_the_means_with_them`
- `Hands_turned_off_take_their_share_of_the_means_with_them`

`Simulation/Production/TechniqueTests.cs`

- `A_technique_must_have_a_name`
- `A_technique_that_yields_nothing_is_no_technique`
- `A_technique_equips_each_hand_alike`
- `Two_techniques_stated_alike_are_the_same_technique`

`Simulation/WorldTests.cs` (extend) — one test per new `Assemble` rejection, plus
`A_technique_is_found_by_its_name`.

`Simulation/Countries/CountryTests.cs` (extend)

- `Opening_a_branch_takes_its_instruments_out_of_the_store`
- `A_branch_cannot_be_opened_on_instruments_the_store_does_not_hold` (store and province unchanged)
- `A_branch_cannot_be_opened_in_a_province_the_country_does_not_govern`
- `Taking_on_hands_takes_their_instruments_out_of_the_store`
- `Hands_cannot_be_taken_on_without_instruments_to_give_them` (unchanged afterwards)
- `Hands_with_bare_hands_need_nothing_from_the_store`
- `Laying_off_returns_nothing_to_the_store`
- `Reequipping_takes_the_new_instruments_and_scraps_the_old`
- `A_branch_cannot_be_reequipped_to_make_something_else`
- `Reequipping_raises_what_an_hour_of_labour_yields`
- `Closing_a_branch_scraps_its_instruments`

## Out of scope

Deciding necessary labour (plan 06 derives it; here it is a parameter). Any UI. Any rule about which
class may work under which form: `Branch` deliberately does not check form against mode, see its
comment, and that stays.

## Done when

Build and tests are clean. In a test, a country with machinery in store can re-equip a hand-loom
branch with power looms, and its next day's cloth output is four times what it was.
