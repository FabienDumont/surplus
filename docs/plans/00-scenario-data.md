# 00 — Scenario data: "1836"

Reference numbers for the one starting scenario. No code is written from this file alone; plans 04, 05,
06, 09, 10 and 11 point here. The figures are tuned so that every country feeds and clothes itself with a small
surplus. They are not history, and nobody should correct them toward history.

Start date: **1836-01-01**. Game name: **"1836"**. Every working day is **12 hours**. Every branch
starts with necessary labour of **4.5 hours** (plan 06 replaces this daily with a derived figure).

## Commodities

| Name      | Unit    | Department          | Satisfies the want for          | Initial value (h per unit) |
|-----------|---------|---------------------|---------------------------------|----------------------------|
| Corn      | t       | MeansOfConsumption  | nourishment                     | 120                        |
| Cloth     | m       | MeansOfConsumption  | clothing                        | 2                          |
| Fibre     | kg      | MeansOfProduction   | spinning and weaving            | 2                          |
| Iron      | t       | MeansOfProduction   | metal to be worked              | 100                        |
| Machinery | machine | MeansOfProduction   | motive and working machinery    | 2100                       |

"Fibre" stands for wool, flax and cotton without telling them apart.

Every quantity is metric: tonnes (t), metres (m) and kilograms (kg). The figures were first written in
bushels, yards, pounds and long tons and moved to metric by round factors (a bushel of corn as 25 kg, a
yard as a metre, a pound as half a kilogram, a long ton as a tonne), every quantity of a commodity scaled
alike, so every figure in hours is unchanged. Players who want imperial units get them as a display
preference; the game counts in these.

## Techniques (introduced in plan 03)

All quantities are **per hand, per day**. Output per hour times 12 gives output per hand per day.
The catalogue is in this order. Every technique comes after the ones it develops from (plan 11).

| Name            | Produces  | Form            | Output per hour | Instruments per hand | Daily wear | Material per hand per day |
|-----------------|-----------|-----------------|-----------------|----------------------|------------|---------------------------|
| Peasant tillage | Corn      | Handicraft      | 0.00625 t       | none                 | 0          | none                      |
| Large farming   | Corn      | Cooperation     | 0.01 t          | none                 | 0          | none                      |
| Fibre growing   | Fibre     | Cooperation     | 0.5 kg          | none                 | 0          | none                      |
| Hand loom       | Cloth     | Handicraft      | 0.5 m           | none                 | 0          | 3 kg of Fibre             |
| Iron working    | Iron      | Cooperation     | 0.01 t          | none                 | 0          | none                      |
| Machine shop    | Machinery | Manufacture     | 0.0005 machine  | none                 | 0          | 0.006 t of Iron           |
| Power loom      | Cloth     | MachineIndustry | 2 m             | 0.5 Machinery        | 0.002      | 12 kg of Fibre            |

**How each technique is found (plan 11).** Elementary techniques develop from nothing. A country
either knows them from the start or never finds them.

| Name          | Develops from             | Development (h) |
|---------------|---------------------------|-----------------|
| Large farming | Peasant tillage           | 50,000,000      |
| Machine shop  | Iron working              | 1,000,000       |
| Power loom    | Hand loom, Machine shop   | 20,000,000      |

Peasant tillage, Fibre growing, Hand loom and Iron working are elementary.

## Subsistence (introduced in plan 06)

Per head, per day: **0.025 t (25 kg) of Corn** and **0.5 m of Cloth**.

## Countries

### United Kingdom — played by the human

Government: `ConstitutionalMonarchy`, `Liberalism`, no laws, relieves the poor (plan 09).

**Lancashire** — Proletariat 7,500 · Bourgeoisie 2,000 · PettyBourgeoisie 8,000

| Branch    | Technique    | Workforce   | Hands |
|-----------|--------------|-------------|-------|
| Cloth     | Power loom   | Proletariat | 5,000 |
| Iron      | Iron working | Proletariat | 150   |
| Machinery | Machine shop | Proletariat | 2,000 |

**Kent** — AgriculturalProletariat 58,500 · Landowners 2,000 · Peasantry 18,000

| Branch | Technique     | Workforce               | Hands  |
|--------|---------------|-------------------------|--------|
| Corn   | Large farming | AgriculturalProletariat | 45,000 |
| Fibre  | Fibre growing | AgriculturalProletariat | 10,500 |

Store: Corn 150,000 t · Cloth 3,000,000 m · Fibre 1,800,000 kg · Iron 360 t · Machinery 150

### United States — AI

Government: `Republic`, `Liberalism`, no laws.

**New England** — Peasantry 60,000 · PettyBourgeoisie 20,000 · Proletariat 1,300 · Bourgeoisie 1,000 ·
Merchants 4,000

| Branch    | Technique       | Workforce        | Hands  |
|-----------|-----------------|------------------|--------|
| Corn      | Peasant tillage | Peasantry        | 32,000 |
| Cloth     | Hand loom       | PettyBourgeoisie | 13,000 |
| Iron      | Iron working    | Proletariat      | 200    |
| Machinery | Machine shop    | Proletariat      | 1,000  |

**Carolina** — Slaves 40,000 · SlaveOwners 1,000 · PettyBourgeoisie 9,000

| Branch | Technique     | Workforce | Hands  |
|--------|---------------|-----------|--------|
| Corn   | Large farming | Slaves    | 12,000 |
| Fibre  | Fibre growing | Slaves    | 10,000 |

Store: Corn 112,500 t · Cloth 2,250,000 m · Fibre 1,170,000 kg · Iron 180 t

### Russia — AI

Government: `AbsoluteMonarchy`, `Traditionalism`, no laws.

**Muscovy** — Serfs 100,000 · FeudalLords 3,000 · Clergy 2,000 · FreePeasants 10,000 · Merchants 5,000

| Branch | Technique       | Workforce | Hands  |
|--------|-----------------|-----------|--------|
| Corn   | Peasant tillage | Serfs     | 42,000 |
| Fibre  | Fibre growing   | Serfs     | 8,000  |
| Cloth  | Hand loom       | Serfs     | 15,500 |

**Ukraine** — Serfs 52,000 · FeudalLords 2,000 · FreePeasants 6,000

| Branch | Technique       | Workforce | Hands  |
|--------|-----------------|-----------|--------|
| Corn   | Peasant tillage | Serfs     | 20,000 |

Store: Corn 135,000 t · Cloth 2,700,000 m · Fibre 1,395,000 kg

### What each country knows (plan 11)

| Country        | Knows                                                                                  |
|----------------|----------------------------------------------------------------------------------------|
| United Kingdom | all seven                                                                              |
| United States  | all seven. New England still weaves by hand: knowing a technique is not using it        |
| Russia         | Peasant tillage, Fibre growing, Hand loom, Iron working                                |

## Figures the tests can check

These follow arithmetically from the tables. Use them as expected values, with a tolerance of four
decimal places on hours.

**Structure at the start**

| Country        | Population | Armable | Dominant mode |
|----------------|------------|---------|---------------|
| United Kingdom | 96,000     | 96,000  | Capitalist    |
| United States  | 136,300    | 96,300  | Capitalist    |
| Russia         | 180,000    | 180,000 | Feudal        |

Province modes: Lancashire, Kent, New England are Capitalist; Carolina is Slave; Muscovy and Ukraine
are Feudal.

**Daily flows (produced / consumed inside the country)**

| Country        | Corn (t)          | Cloth (m)         | Fibre (kg)        | Iron (t)| Machinery |
|----------------|-------------------|-------------------|-------------------|---------|-----------|
| United Kingdom | 5,400 / 2,358.125 | 120,000 / 47,162.5 | 63,000 / 60,000   | 18 / 12 | 12 / 5    |
| United States  | 3,840 / 3,405     | 78,000 / 68,100   | 60,000 / 39,000   | 24 / 6  | 6 / 0     |
| Russia         | 4,650 / 4,500     | 93,000 / 90,000   | 48,000 / 46,500   | —       | —         |

Corn and Cloth consumption is the subsistence of those with a title to it (plans 06 and 09): every head
but the unemployed. Britain relieves its 3,350 unemployed with half a basket each, so it feeds 94,325
baskets a day. America's 100 unemployed proletarians get nothing, so it feeds 136,200. Russia has no
wage-earners. Fibre and Iron consumption is
material. Machinery consumption is the replacement of worn instruments (2,500 machines at 0.002).

**Values after the first day of production (plan 05), stable on every later day while nothing else
changes**

| Commodity | Value (h per unit)                    |
|-----------|---------------------------------------|
| Corn      | 1,812,000 / 13,890 = 130.4536         |
| Cloth     | 703,500 / 291,000 = 2.4175            |
| Fibre     | 2                                     |
| Iron      | 100                                   |
| Machinery | 2100                                  |

**Necessary labour after the first day (plan 06):** 0.025 × 130.4536 + 0.5 × 2.4175 = **4.4701 hours**.

**The reserve army (plan 09):**

| Province    | Class                   | Hands  | Heads  | Unemployed |
|-------------|-------------------------|--------|--------|------------|
| Lancashire  | Proletariat             | 7,150  | 7,500  | 350        |
| Kent        | AgriculturalProletariat | 55,500 | 58,500 | 3,000      |
| New England | Proletariat             | 1,200  | 1,300  | 100        |

Until plan 09 these classes counted 60,000, 110,000 and 15,000 heads. Only heads changed, so
production, values, necessary labour and development are as they were.

**A year left alone (plans 02 to 09):** advancing 365 days with no player action and no AI, no branch
stands idle and no store falls short in any country.

- Britain and America report those with no wage every day.
- Britain's reserve army falls from 3,350 to under 1,000 in the year, by death and by sinking into
  pauperism, and its population with it.
- America and Russia grow.

**Development (plan 11):**
- Russia's 62,000 serfs on corn put 744,000 h a day toward large farming. At the feudal drive of 0.1
  that counts as 74,400 h, so Russia finds it on the 673rd day (50,000,000 ÷ 74,400 = 672.04).
- Nothing else is within Russia's reach:
  - It has no iron branch, so nothing brings it closer to the machine shop.
  - The power loom needs the machine shop first.
- A year left alone therefore finds nothing out.
