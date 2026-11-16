# 15 — Regimes and the verdict

## Status

**Design plan, not an implementation contract.** Unlike plans 00–14 it gives no signatures to
build against. It settles what the game is for and what bounds the player's politics. The
implementation plans that follow (see **Follow-up plans**) turn each part into code, in the usual
format. Where this plan gives a number or a rule, take it as the intended shape, to be tuned once
it runs.

## Goal

Answer the question the README leaves open under "Goals and pressure": what is the player trying
to do, and what stops them doing it at once?

Two things are wrong today:

1. **There is no goal.** Nothing in the model says one game went better than another.
2. **Politics is a menu.** `Session.Enact`, `Repeal`, `TakeForm` and `Adopt` work at any moment,
   for any law, form or ideology. Russia can abolish serfdom on 1 January 1836 and become a
   council republic the next morning. The only thing that pushes back is the domain's
   `DomainException` against enacting a law twice.

This plan settles both, and it does so without the game taking a side. **Every ideology must be
playable and every ideology must be able to win.** Marx is the engine of the simulation, not the
referee of the score.

## The principle

Every society, whatever it avows, has to do the same three things each year:

1. feed the people it holds,
2. replace in kind the means of production it used up,
3. dispose of what is left: **the surplus product**.

Ideologies don't disagree about 1 and 2. They disagree about 3: whose the surplus is and what
it's for. The model already says this through `IdeologyProfile`. Each ideology `ServesClass` one
class and `DrivesToward` one mode of production, "whatever it says of itself".

From that come the two halves of this plan:

- **The verdict.** Every regime is judged by its own profile: did the class it serves prosper,
  and did the society move toward the mode it drives toward? On top of that comes one test no
  regime is excused from: did the society reproduce itself?
- **Regime change.** A state stands on classes. A law, a form or an ideology can be put in place
  only when the classes it serves outweigh the classes it strikes at, and *who counts* in that
  weighing is set by the form of government already in place.

## 1. The world as it stands

The player picks **a country**, and nothing else. Its government, laws, classes, branches and
knowledge come from the scenario, as `EighteenThirtySix` already does:

| Country        | Form                   | Ideology       | Classes that matter                                     |
|----------------|------------------------|----------------|---------------------------------------------------------|
| United Kingdom | Constitutional monarchy| Liberalism     | Bourgeoisie, landowners, proletariat, agricultural proletariat |
| United States  | Republic               | Liberalism     | Peasantry, petty bourgeoisie; slave owners and slaves in Carolina |
| Russia         | Absolute monarchy      | Traditionalism | Feudal lords, clergy, serfs, free peasants              |

There is no "custom start" and no choice of ideology at setup. The scenario *is* the opening
position. Earlier start dates (README, "Planned") will bring their own.

## 2. Regime change: conditions, not a menu

### 2.1 Whose voice counts: the form of government

Every class has a **weight** in the state. Under every form, weight is head count multiplied by a
factor. **The form of government decides the factor**, because a form is exactly a rule about
whose voice counts:

| Form                    | Who weighs                                                                 |
|-------------------------|----------------------------------------------------------------------------|
| Absolute monarchy, Theocracy | Appropriators only (`AppropriatesSurplus`), plus clergy under a theocracy |
| Oligarchy               | The appropriators of the dominant mode only                               |
| Constitutional monarchy | Appropriators fully; holders of their own means (`HoldsOwnMeans`) at a fraction; the rest at nothing (a narrow franchise) |
| Republic                | Every armable head (`CanBeArmed`), so slaves don't weigh, as in 1836       |
| Dictatorship, Technocracy, Party state | The class the ruling ideology `ServesClass`, plus whoever holds arms (see 2.4) |
| Council republic, Commune | Direct producers only (`IsDirectProducer`)                              |
| Tribal council          | Every head                                                                 |

These factors are the main dial to tune. The order of the table is fixed, the numbers aren't.

### 2.2 What a change asks for

Every change (enacting or repealing a law, taking a form, adopting an ideology) has **for** and
**against** classes, read off the model and not hand-written per change:

- **Law.** *Against*: every class standing in a relation the law `Forbids` on the appropriating
  side (`OwnsProducers`, `OwnsLand`, `OwnsCapital`). *For*: every class standing in a forbidden
  relation on the producing side, plus whatever `Laws.Frees` would make of them, plus the class
  the ruling ideology serves if that class is not itself struck at.
  - Repeal reverses for and against.
- **Ideology.** *For*: the class the new ideology serves. *Against*: the class the current one
  serves.
- **Form.** *For*: the classes that weigh more under the new form than under the current one.
  *Against*: those that weigh less.

A change is **possible** when the weight *for* exceeds the weight *against*, both counted under
the **current** form. That last clause is the whole design. In Russia under absolutism, serfs
weigh nothing, so serf emancipation needs the feudal lords to be outweighed by a class that
already counts: merchants, a bourgeoisie, or the crown's own servants under a later form. In the
United States the slaves weigh nothing either. Abolition needs the free classes of the whole
republic to outweigh the slave owners, and that grows possible only as New England grows.

### 2.3 Prerequisites

A few changes also need a material base or a prior change. They are kept short and checkable:

- **Economic base.** `NationalisationOfIndustry` needs a bourgeoisie to nationalise.
  `AbolitionOfWageLabour` needs wage labour to be the prevailing relation. Any form or ideology
  whose `DrivesToward` mode is two or more steps beyond the country's `PrevailingMode` is out of
  reach. No leap from feudalism straight to communism, at least not without 2.4.
- **Form ladder.** A council republic or commune follows a revolution (2.4), not a decree. A
  republic or constitutional monarchy can be reached from an absolute monarchy by decree, since
  that happened (Prussia 1848, Piedmont).

### 2.4 Crises open the door

Some events lower the bar for a time, which the console reports as an occurrence:

- a **lost war** (any peace in which the country cedes provinces),
- a **famine** (deaths from want above a share of the population in a month),
- **mass worklessness** (paupers above a share of the population).

During such a **crisis**, which lasts some months, the weight of the classes hit hardest counts
under the next more popular form, as though the street had a voice. That's how Russia's serfs
come to matter in 1905 without a franchise.

When a crisis coincides with a **discontented majority**, meaning the weight against the regime,
counted with the street, exceeds the weight for it, the regime **falls**:

- the ideology becomes the one serving the strongest discontented class whose `DrivesToward`
  mode is within reach (2.3),
- the form becomes the one that class weighs most under,
- **the player keeps the country.** A revolution is not a game over. It hands the player a new
  regime with new aims, and possibly one they didn't want. Changing it back means meeting its
  conditions again.

### 2.5 What the player can still do

The player's levers are everything plans 07–12 already give them: employment, re-equipment,
relief, war and peace. Through those they **change the class structure**, and the class structure
is what opens and closes politics. Industrialise, and the proletariat grows until it weighs.
Enclose, and the peasantry turns into a proletariat. Win a war, and new provinces bring their
classes with them. Politics is reached through the economy, which is the point.

## 3. The verdict

### 3.1 Every year, by the regime then in power

On 1 January each year the world records one **year's verdict** for every country, judged by the
`IdeologyProfile` in force that day. Three marks, each read off the model:

1. **Reproduction (every regime alike).** Did the society reproduce itself? Population kept or
   grown, no `PeopleWentWithout`, no branch idle for want of instruments, no province lost. This
   mark is the same for every ideology.
2. **The class served.** How did the class the ideology `ServesClass` fare? Its heads (grown or
   shrunk) and how often it went without. A liberal regime is judged by its bourgeoisie, a
   traditionalist one by its lords, a socialist one by its proletariat, a Maoist one by its
   peasantry.
3. **The direction of travel.** Did the share of the population in classes native to the
   ideology's `DrivesToward` mode grow? Fascism and conservatism, which drive toward the mode
   they already have, are judged on holding that share, not growing it.

There is deliberately **no single world-wide measure** such as output, accumulation or free time.
Any one of those would crown one ideology. Mark 1 is the common floor, and marks 2 and 3 belong
to each regime.

### 3.2 The end of a game

A scenario has an end date, 1 January 1936 for the 1836 start. The final verdict is the
**chronicle of years**: each regime in turn, how long it held, how it scored by its own lights,
and what it cost. The cost lines are the same for everyone and always shown:

- deaths from want,
- years spent in pauperism, in heads × years,
- provinces lost and taken,
- what each regime's served class gained against what the others lost.

A country is **defeated** only by annexation (plan 08), or by its population falling below a
floor. Everything else is a history that can be read as better or worse, by its own aims and by
the costs every reader sees.

### 3.3 Why this isn't neutral (and needn't be)

The marks are neutral: each regime gets the yardstick it holds itself to. The **cost lines** are
not, because they are what Marx would read the chronicle for. That's deliberate and that's where
the game's voice lives. A liberal Britain can score highly by its own aims while the chronicle
shows a million tonnes of corn in store and its unemployed starving beside it. The player wins on
their terms and sees the bill.

## Follow-up plans

Each is a normal implementation plan, executed in order:

| #  | Plan                 | What exists afterwards                                                              |
|----|----------------------|-------------------------------------------------------------------------------------|
| 16 | Weight of classes    | `Country` reckons each class's weight under its form; the console shows it          |
| 17 | Conditions of change | `Enact`, `Repeal`, `TakeForm`, `Adopt` throw `DomainException` unless 2.2 and 2.3 hold; the console lists what is possible and what blocks the rest |
| 18 | Crisis and revolution| Crises are recorded and reported; a regime can fall (2.4); the AI regimes fall too  |
| 19 | The verdict          | A yearly verdict per country, an end date per scenario, the final chronicle        |
| 20 | AI politics          | AI countries enact what becomes possible for them, so the world changes without the player |

Save version: 16 adds nothing to save; 18 and 19 add state (crises under way, the year's
verdicts) and need a save version bump, per plan 13.

## Decisions to settle before plan 16

- **Factors in 2.1.** One table of numbers, tuned so that in 1836:
  - Britain can repeal nothing that matters to the bourgeoisie,
  - Russia cannot emancipate the serfs,
  - the United States cannot abolish slavery,
  - and each becomes possible somewhere within the first fifty years if the economy is pushed
    that way.

  This will be the 1836 scenario test of plan 17.
- **Ideologies that don't fit 3.1.** Nationalism and fascism serve a class, but they also aim at
  power over other countries. One option is a fourth mark for those ideologies only, read off
  provinces held and `Armable`. The alternative is to leave it to mark 2.
- **Should the player see the conditions before acting?** Recommended: yes. `politics` in the
  console lists each possible change with its weight for and against, so the player plans
  toward a reform rather than discovering it by trial.

## Out of scope

Elections and parties, individual leaders, a legitimacy or stability bar as a separate resource
(weight replaces it), diplomacy, and trade between countries (still the first thing after the
basis, per the README).
