# 10 — Circulation

## Goal

Put the goods where they are. Until now a country keeps one store: corn grown in Kent is in
Lancashire the moment it is threshed, at no cost to anyone (README decision 3). After this plan:

- every province keeps its own store, works out of it and eats out of it;
- goods cross the border between two provinces of the same country, toward where they are lacking;
- carrying them is labour.

Three things in Marx shape the rules:

1. **Transport is production.** Moving a product is a labour process that continues production
   inside circulation, and the labour spent on it adds value (Capital II, ch. 6 §3). So carriage is
   a commodity here. Carters make it, and the world revalues it like anything else. A province can
   send away only as much as its carriage can move.
2. **How freely goods move depends on the mode of production.** A feudal province lives largely on
   what it grows. Little leaves the manor, and what does goes as rent in kind. Capital is what welds
   provinces into one market, sending every good wherever it is wanted. The slave South belongs on
   the market side: Marx notes that once slave production is drawn into a world market it is
   overworked for export (Capital I, ch. 10 §2). So a province ships goods at the pace its mode
   allows.
3. **Carriage cannot be stored.** A cart-day not used today is gone. Carriage is made fresh each
   day and whatever is left over at the end of the day perishes.

Nothing crosses a border between two countries. Trade between states is later work.

## As built

Where the build departs from the text below:

- **The day's movement is `Traffic.Circulate`.** A static class called `Circulation` would share
  its name with its own namespace.
- **Bulk is a `Bulk(CommodityId, decimal)` record**, not a tuple.
- **`Haulage.None`** is a haulage with no carriage at all, where nothing limits what crosses a
  border. It is the builder's default, and `Assemble` accepts it.
- **`Rations` keeps one `StoreShare`:** the worst-supplied province's. A shortage is reported for
  the country with the share of the province where it was worst.
- **The digest names a played province only below five days of its basket, not ten.** Circulation
  keeps every province near ten days, and each eats one before the next delivery. At ten, both
  British provinces were named every month whatever happened.
- **Goods do not pass through a province that has enough.** A province sends only toward a
  neighbour below its own target. Goods therefore reach a province two borders away only if the one
  between is short too, and this text's "reached in two days" is wrong. The 1836 scenario has only
  pairs of provinces. It is recorded as a known simplification in the README.
- **`Country.Load` lost its store parameter.** `CountryBuilder.Holding` lays its heaps up in the
  country's first province.

## Read first

- `docs/plans/README.md`, decisions 3, 5 and 9
- `Country.cs`: `Store`, `LayUp`, `WorkOneDay`, `OpenBranch`, `Employ`, `Reequip`, `Provision` (as
  plan 09 left it)
- `Province.cs`
- `World.cs`: `Assemble`, `Load`, `Elapse`, `MakePeace` (plan 08's annexation lays up the loser's
  store in the winner's)
- `surplus.Application/Reporting/Digest.cs` and `Standing.cs`
- `surplus.Testing`: `CountryBuilder`, `ProvinceBuilder`, `WorldBuilder`

## Changes

### Stores move to provinces

- `Province` gains `Store Store { get; private set; }` and `LayUp(Stock)`. `Province.Load` gains the
  store, and `ProvinceBuilder` gets `Holding(...)`.
- `Country.Store` becomes a read-only sum of its provinces' stores, for reading only. `Country.LayUp`
  is removed. Scenario and tests lay up province by province.
- `Country.WorkOneDay`: each branch draws its material and wear from **its own province's** store,
  and lays its product up there. A branch whose province lacks its material stands idle, even if
  another province holds plenty.
- `OpenBranch`, `Employ` and `Reequip` take instruments from the province's own store. The message
  for a shortfall names the province: *"There is not enough in store in Lancashire to equip 40
  hands: 10 machine lacking."*
- `Provision` (plan 09) provisions each province from its own store. Claims and shares are worked out
  province by province, so `StoreShare` becomes a per-province figure. `Rations` keeps a list of
  `(ProvinceId, decimal StoreShare)`, and `StoreFellShort` is true if any province fell short.
- Plan 08's annexation no longer moves a store. Provinces carry their stores with them, so a cession
  now hands the victor the ceded provinces' stores too. Remove the transfer step from
  `World.MakePeace` and update its test to say that stores travel with provinces.

### `Simulation/Circulation/`

**`Border.cs`**: `sealed record Border(ProvinceId One, ProvinceId Other)`. A border has no direction:
`Border(a, b)` equals `Border(b, a)`, so override equality to say so.

**`Haulage.cs`**: what carrying costs.

```csharp
public sealed record Haulage
{
  public CommodityId Carriage { get; }                 // the commodity carters make, counted in loads
  public decimal LoadsPer(CommodityId commodity);      // loads one unit of a commodity takes up; 0 if unlisted

  public static Haulage Of(CommodityId carriage, IEnumerable<(CommodityId Commodity, decimal LoadsPerUnit)> bulk);
}
```

`Of` refuses a negative bulk, a commodity listed twice, and a bulk given for carriage itself.

**`MarketIntegration.cs`**: a static register in the style of `ProductiveForms`:
what share of its surplus a province of each mode sends across a border in a day.

```csharp
public static decimal Integration(this ModeOfProduction mode);
```

| Mode              | Share | Comment to write                                                        |
|-------------------|-------|-------------------------------------------------------------------------|
| PrimitiveCommunal | 0.01  | Exchange happens at the edges of the community, not inside it           |
| Slave             | 0.3   | Drawn into the world market, the plantation works for export            |
| Feudal            | 0.05  | The manor lives on its own produce; what leaves it leaves as rent       |
| Capitalist        | 0.5   | Production for sale; every good goes where it is wanted                 |
| Socialist         | 0.5   | Equal to capital pending a model of planning; say so                    |
| Communist         | 0.5   | Likewise                                                                 |

These are tuned figures, and the comment says so.

**`Circulation.cs`**: the day's movement, a static method called by `World.Elapse`.

```csharp
public static IReadOnlyList<Occurrence> Circulate(World world);
```

- **What a province needs** of a commodity in a day:
  - its basket for every head;
  - plus the material and wear its branches consume (`ConsumedInOnePeriod`).

  Its **target** is ten days of that. Make ten a named constant: *a province keeps ten days of what
  it needs before it sends anything away*.
- **The order:** for each country in world order, for each border in world order whose two provinces
  that country both governs, for each commodity in register order except carriage, consider sending
  first from `One` to `Other`, then from `Other` to `One`.
- **For one direction, from A to B:**
  - `surplus` = what A holds above A's target, if positive;
  - `want` = what B lacks of B's target, if positive.
  - If both are positive, the amount moved is the smallest of:
    - `surplus × A.Mode.Integration()`;
    - `want`;
    - A's carriage held ÷ the commodity's loads per unit (no limit when that is 0).
  - Draw the goods and the carriage from A's store, and lay the goods up in B's.
- **When carriage was what stopped a move**, report `CarriageFellShort(Country, Province,
  Commodity)` once per province and commodity per day: *"Kent had not the carts to send Corn where
  it was wanted (United Kingdom)."*
- **After all movement**, every province's remaining carriage perishes: draw it all.

Goods move one border a day. A province two borders away is reached in two days, and that is
deliberate.

### `World.cs`

- New properties: `IReadOnlyList<Border> Borders` and `Haulage Haulage`. `Assemble` and `Load` gain
  them, and `WorldBuilder` gets `WithBorders` and `WithHaulage`. The builder defaults to no border
  and to a haulage on a carriage commodity it registers itself.
- `Assemble` refuses:
  - a border whose provinces are not both in the world;
  - a border from a province to itself;
  - the same border twice, in either direction;
  - a haulage whose carriage or bulk names an unregistered commodity.

  Borders between provinces of different countries are allowed. Nothing crosses them yet, but war
  and trade will need them.
- `Elapse` now runs in this order:
  1. work;
  2. revalue;
  3. necessary labour;
  4. **circulate**;
  5. provision and deaths;
  6. development (plan 11 adds it);
  7. on the first of the month: fighting, pauperism, growth.

  Circulation comes before provision because what is eaten today is what reached the province
  today.

### Scenario

- Add a commodity **Carriage** (unit `load`, MeansOfProduction, "carrying goods", initial value 10 h
  a load).
- Add an elementary technique, **Carting**: Carriage, Cooperation, 0.1 load an hour, no means.
- Haulage per unit: Corn 1 load a tonne; Iron 1 load a tonne; Fibre 0.001 a kg; Cloth 0.0005 a
  metre; Machinery 0.5 a machine.
- Borders:
  - Lancashire–Kent, New England–Carolina and Muscovy–Ukraine;
  - Kent–New England is out: there is no sea in this model, so do not add one.
- **Split each country's store between its provinces.** Each province starts with 30 days of its own
  daily need of every commodity, at the start. The rest of that commodity goes to the province that
  produces it, or the first such province. Machinery and Iron go wholly to the province that works
  them.
- **Add carting branches.** Each province that will send goods across its border gets one, staffed
  from its own province:
  - from the reserve army where there is one;
  - otherwise from the class that works its other branches (serfs in Muscovy, slaves in Carolina).

  Size each so that the province can send its expected daily flow with about 20% to spare. Work the
  flows out from the daily production and need of each province.
- **Write the result into `00-scenario-data.md`**:
  - the per-province stores, the carting branches and the borders;
  - the expected daily flows across each border;
  - the changes in plan 09's reserve army that the carters take.

  The carters change employment, and so the reserve army and the necessary flows, but not the values
  of the other commodities.
- **The targets the retuned scenario must meet**, tested below:
  - **A year left alone:** no branch stands idle, no store falls short, and no carriage falls short
    in the United Kingdom and the United States.
  - **Feudal and capitalist, side by side.** Russia's Ukraine, which grows only corn, may run
    short of cloth: Muscovy sends only 5% of its surplus a day. Record whether it does, and leave it
    so if it does. That difference between the feudal and the capitalist provinces is the point.

### Application and console

- `Digest` and `Standing` keep reading the country's total store: the player still governs a
  country. Add one line per province of the played country whose own store holds less than ten days
  of its basket: `Lancashire: 4 days of subsistence in store.`
- `Significance`: `CarriageFellShort` is Urgent at home and Notable abroad. Its default concern
  (equality of the record) already collapses it day after day.
- Console:
  - `provinces` shows each province's store, and its borders by name.
  - `stores` shows the country's total, then each province.
  - `open`, `employ` and `reequip` already name a province, so their usage is unchanged.

## Tests

`Simulation/Circulation/BorderTests.cs`

- `A_border_runs_both_ways`

`Simulation/Circulation/HaulageTests.cs`

- `Each_commodity_takes_up_its_own_room`
- `A_commodity_nobody_listed_takes_up_none`
- `Bulk_cannot_be_negative_or_given_twice`
- `Carriage_does_not_carry_itself`

`Simulation/Circulation/MarketIntegrationTests.cs`

- `Capital_sends_ten_times_what_the_manor_does`
- `The_slave_south_trades_more_than_the_manor_and_less_than_capital`

`Simulation/Circulation/CirculationTests.cs`

- `Goods_cross_a_border_toward_where_they_are_wanted`
- `A_province_keeps_ten_days_of_what_it_needs_before_sending_any`
- `A_province_sends_only_what_its_mode_lets_go_of_in_a_day`
- `Goods_go_no_further_than_the_want_on_the_other_side`
- `Carriage_limits_what_can_be_sent`
- `Carriage_falling_short_is_reported`
- `Carriage_not_used_today_is_gone_tomorrow`
- `Nothing_crosses_a_border_between_two_countries`
- `Goods_move_one_border_a_day`

`Simulation/Countries/CountryTests.cs` and `ProvinceTests.cs` (update and extend)

- `A_branch_works_out_of_its_own_provinces_store`
- `A_branch_stands_idle_for_want_of_material_its_neighbour_holds`
- `A_branch_is_equipped_out_of_its_own_provinces_store`
- `A_country_holds_what_its_provinces_hold`
- `Each_province_eats_out_of_its_own_store`

`Simulation/WorldTests.cs` (extend)

- `A_border_must_join_two_provinces_of_the_world`
- `A_border_cannot_be_drawn_twice`
- `A_ceded_province_takes_its_store_with_it`
- `Circulation_comes_before_anyone_eats`

`surplus.Application.Tests/Scenarios/EighteenThirtySixTests.cs` (extend)

- `The_scenario_draws_its_borders`
- `Each_province_starts_with_a_month_of_what_it_needs`
- `A_year_left_alone_keeps_britain_and_america_supplied`
- `Russias_provinces_live_mostly_on_their_own` (Muscovy's sends to Ukraine stay below 5% of its
  surplus each day)

## Out of scope

- Trade between countries, money and prices that differ by place. Value stays world-wide, and the
  labour of carriage is counted in carriage's own value but not added to the goods it carried: local
  value needs a market. Add this to the README's known simplifications.
- Roads, railways and canals as things to build. Carting is the only technique of transport; plan 11
  can later add a railway that grows out of the machine shop.
- Distance beyond counting borders.
- The AI choosing what to send. Movement follows the rule, never a choice.

## Done when

- Build and tests are clean.
- A year of the scenario left alone supplies every province of Britain and the United States across
  their borders.
- Closing Kent's carting branch leaves Lancashire hungry within the month, and the clock stops when
  it does.
