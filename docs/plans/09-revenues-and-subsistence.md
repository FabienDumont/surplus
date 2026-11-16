# 09 — Revenues and access to subsistence

## Goal

Make who eats depend on how they live. Since plan 06 everyone in a country takes the same basket out
of one store, employed or not, so the reserve army is a number and not a condition. Under capital
that is exactly backwards. A worker without a wage has no title to anything, however full the
granaries are. That is what makes the reserve army a lever on those still in work, and what makes
unemployment a crisis for the state that has to answer it.

After this plan each class reaches its subsistence through its own revenue, the `IncomeSource` every
class already carries:

| Revenue                                          | Title to the basket                                   |
|--------------------------------------------------|-------------------------------------------------------|
| Rent, Profit, MerchantProfit, Interest, Tithe    | Every head, served before anyone else                 |
| OwnProduct, Maintenance, Distribution            | Every head                                            |
| Wages                                            | One basket for each hand a branch employs, and no more |
| Plunder                                          | None                                                  |

- A state that enacts **poor relief** gives the unemployed of the wage-earning classes, and the
  Lumpenproletariat, half a basket each. Relief is set at half after the principle of "less
  eligibility" that the 1834 Poor Law wrote down: relief must be worse than the worst wage.
- Each month 1% of the unemployed of each wage-earning class sink into the Lumpenproletariat. That
  is pauperism, the "dead weight" of the reserve army (Capital I, ch. 25 §4). No branch can employ
  them, because they are not direct producers.

The basket itself is unchanged and still the same for everyone (README decision 9). What changes is
who has a claim on it.

## Read first

- `docs/plans/README.md`, decision 9 and the known simplifications
- `surplus.Domain/Simulation/Society/IncomeSource.cs`, `ClassProfile.cs`, `SocialClasses.cs`
- `surplus.Domain/Simulation/Politics/Law.cs`, `Laws.cs`
- `Country.cs`: `Provision`, `LoseTheUnprovided`, `Multiply` as plan 06 left them
- `World.Elapse` as left by plan 08
- `surplus.Application/Reporting/Significance.cs`, `Digest.cs`, `Standing.cs` (plan 07)

## Changes

### `Politics/Law.cs` and `Laws.cs`

Add `Law.PoorRelief`. It forbids no relation: register it in `Laws` with an empty list, so `Enforce`
does nothing to the class structure. Its doc comment carries the history:

- the parish relief of the Old Poor Law;
- the workhouse of 1834;
- less eligibility.

### `Society/Access.cs` and `Society/Revenues.cs`

```csharp
public enum Access { First, Whole, Employed, None }

public static class Revenues
{
  /// What a class's revenue gives it a title to.
  public static Access Access(this IncomeSource income);
}
```

The mapping is the table above. The comments say why:

- The appropriators are served first because the surplus product is theirs before it is anyone
  else's.
- The petty producer and the serf eat from the plot whether or not the model has put them in a
  branch.
- The slave is kept as property is kept.
- The wage is paid only for labour-power actually bought.

### `Countries/Ration.cs` and `Countries/Rations.cs`

```csharp
/// What one class in one province had a title to today, and what it got.
public sealed record Ration(
  ProvinceId Province, SocialClass Class, int Heads, int Workless, decimal Claimed, decimal Provided);

/// A country's day of provisioning.
public sealed record Rations(IReadOnlyList<Ration> Each, decimal StoreShare)
{
  public int Workless { get; }        // sum of Workless: those with no title of their own, relieved or not
  public bool StoreFellShort { get; } // StoreShare < 1
}
```

- `Workless` is the heads of a class with no title of their own: the unemployed of a wage-earning
  class, every head of a class that lives by plunder, and zero for everyone else.
- `Claimed` and `Provided` are counted in baskets, so half a basket of relief is 0.5.
- `StoreShare` is the share of the claims after the first that the store could meet.
- Content equality on `Each`.

### `Country.cs`

Replace plan 06's three methods:

```csharp
public Rations Provision(Subsistence subsistence);
public Losses LoseTheUnprovided(Rations rations);
public int Multiply(Rations rations);
public int Pauperise();
```

**Provision.**

1. For every province and every class present, work out the claim:
   - `First` and `Whole` claim one basket for every head.
   - `Employed` claims one basket for each hand employed in the province's branches by that class.
     Under poor relief it also claims half a basket for each head not employed.
   - `None` claims nothing, or half a basket a head under poor relief.
2. Serve the `First` claims. Their share is the smallest, over the basket's goods, of held ÷ needed,
   capped at 1. Draw that share.
3. Serve every other claim, in proportion, from what is left, in the same way. That share is
   `StoreShare`. A country with no other claim has a `StoreShare` of 1.
4. Keep plan 06's guard: never draw a hair more than the store holds.

**LoseTheUnprovided.** For every ration of a class that does not appropriate surplus:

- deaths of shortage are `floor((Claimed − Provided) × 0.01)`;
- deaths of worklessness are `floor((Heads − Claimed) × 0.01)`.

Apply both with `Province.Decline`. The dead fall first on those without work, because
`DismissThoseWhoAreNoLongerHere` lays off only when a class has fewer heads than hands. Return
`Losses(int OfShortage, int OfWorklessness)`, a record in `Countries/Losses.cs`.

**Multiply.** On the first of the month, every class grows by `floor(Provided × 0.001)`. Only those
fed multiply. Plan 06's rule that a country short on the day does not grow at all is replaced by this
one.

**Pauperise.** For every province and every class whose revenue is `Wages`, the number who sink is
`floor(Idle(class) × 0.01)`. They are moved with `Province.Transform(class, Lumpenproletariat, n)`.
Return the total. Named constant, with a comment on Capital I, ch. 25 §4.

### `World.Elapse`

Step 4 becomes:

1. Every country provisions its people.
2. The unprovided die.
3. Report:
   - `PeopleWentWithout(Country, StoreShare, OfShortage)` when the store fell short;
   - `TheWorklessWentWithout(Country, Workless, OfWorklessness)` when anyone had no claim of their
     own, even if relieved.

On the first of the month, after fighting:

1. Every country pauperises, reporting `SankIntoPauperism(Country, Heads)` when above zero.
2. Every country multiplies from today's rations, reporting `PopulationGrew` as before.

New occurrences:

- `Chronicle/TheWorklessWentWithout.cs`: `(string Country, int Heads, int Deaths)`, e.g.
  *"In United Kingdom 3,350 had no wage to live on; 16 died."* (Under relief, 1,675 baskets go
  short: 1 death in Lancashire and 15 in Kent.) Write *"no one died of it today"* when
  `Deaths` is zero, as for `PeopleWentWithout`.
- `Chronicle/SankIntoPauperism.cs`: `(string Country, int Heads)`, e.g. *"In United Kingdom 34 sank
  into pauperism."*

### `surplus.Application/Reporting`

- `Significance`:
  - `TheWorklessWentWithout` is Notable at home and abroad, and its concern is the country. It is
    chronic under capital and must not stop the clock every month.
  - `SankIntoPauperism` is Notable.
- `Digest` collapses both, per country:
  - `United Kingdom: those with no wage went short on 31 days; 520 died.`
  - `United Kingdom: 34 sank into pauperism.`
- `Standing.WithoutWork` counts only the classes whose revenue is `Wages`: the reserve army. A
  peasant outside every branch still lives off the plot. The console's `provinces` listing follows
  it, and also shows the Lumpenproletariat among the classes, where the class structure already
  lists it.

### Scenario

Until now the 1836 figures counted 107,000 idle wage-earners in Britain alone. Under this plan that
would mean a thousand deaths a day. Retune the wage-earning classes so that each holds a reserve army
of about 5%. **Change heads only, never hands**, so that production, values, necessary labour and
plan 11's development figures stay exactly as they are:

| Province    | Class                   | Hands | Heads now | Heads after | Reserve |
|-------------|-------------------------|-------|-----------|-------------|---------|
| Lancashire  | Proletariat             | 7,150 | 60,000    | 7,500       | 350     |
| Kent        | AgriculturalProletariat | 55,500 | 110,000  | 58,500      | 3,000   |
| New England | Proletariat             | 1,200 | 15,000    | 1,300       | 100     |

The United Kingdom starts with `PoorRelief` enacted; the United States and Russia do not.

Update `00-scenario-data.md`, "Structure at the start" and the daily flows:

| Country        | Population | Armable | Corn consumed (t) | Cloth consumed (m) |
|----------------|------------|---------|-------------------|--------------------|
| United Kingdom | 96,000     | 96,000  | 2,400             | 48,000             |
| United States  | 136,300    | 96,300  | 3,407.5           | 68,150             |
| Russia         | 180,000    | 180,000 | 4,500             | 90,000             |

Dominant modes and province modes are unchanged. Check this with the existing tests rather than by
hand.

## Tests

`Simulation/Society/RevenuesTests.cs`

- `The_appropriators_are_served_before_anyone_else`
- `Those_who_live_off_their_own_product_or_are_kept_have_a_title_whether_employed_or_not`
- `A_wage_is_a_title_only_for_labour_power_actually_bought`
- `Plunder_is_no_title_at_all`

`Simulation/Countries/CountryTests.cs` (rewrite plan 06's provisioning tests to the new API, and add
these)

- `An_employed_worker_eats_and_an_unemployed_one_does_not`
- `Full_granaries_do_not_feed_those_with_no_wage`
- `Poor_relief_gives_the_unemployed_half_a_basket`
- `Poor_relief_reaches_the_lumpenproletariat`
- `When_the_store_falls_short_the_appropriators_eat_first`
- `What_is_left_after_the_appropriators_is_shared_in_proportion`
- `The_workless_die_of_it_a_few_each_day`
- `The_unemployed_die_before_anyone_is_struck_off_a_branch`
- `Only_those_who_were_fed_multiply`
- `Each_month_a_share_of_the_unemployed_sink_into_pauperism`
- `Pauperism_takes_no_one_who_is_employed`
- `The_lumpenproletariat_cannot_be_set_to_work` (`OpenBranch` refuses them; this rule already exists)

`Simulation/WorldTests.cs` (extend)

- `Worklessness_is_reported_apart_from_shortage`
- `A_full_store_reports_no_shortage_even_when_the_workless_go_hungry`
- `Pauperism_is_reported_on_the_first_of_the_month`

`surplus.Application.Tests`

- `Worklessness_is_notable_and_does_not_stop_the_clock` (`SignificanceTests`)
- `Worklessness_and_pauperism_are_summed_per_country` (`DigestTests`)
- `Only_wage_earners_without_a_branch_are_without_work` (`StandingTests`)

`Scenarios/EighteenThirtySixTests.cs` (update and extend)

- Update the structure and flow tests to the new figures.
- `Britain_relieves_its_poor_from_the_start`
- `A_year_left_alone_has_no_shortage_but_has_its_reserve_army`
  (no `PeopleWentWithout`; `TheWorklessWentWithout` every day for the United Kingdom and the United
  States; none for Russia, whose serfs all eat from the plot)

## Out of scope

- Money and wages paid in money.
- Wages that rise and fall with the reserve army.
- Families and dependants: a basket is per head, and a wage feeds one head.
- Charity, crime and emigration.
- The Lumpenproletariat's politics.
- Any trade.

## Done when

Build and tests are clean, the scenario has a reserve army that eats only by relief, and a player who
repeals poor relief sees the workless start to die and sink.
