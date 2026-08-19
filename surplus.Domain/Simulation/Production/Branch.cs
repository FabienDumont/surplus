using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// A branch of production in a province: what is made there, under what form of
/// the labour process, with what means, and by whom.
/// It is not an establishment. It is every establishment of one kind in one
/// province taken together, which is why a province holds at most one branch per
/// commodity, and why a branch needs no identity beyond what it produces and
/// where it stands. Individual mills are left unmodelled on purpose: value is
/// settled by a social average, and adding up mills would make it look as though
/// each mill fixed its own.
/// What a branch does have of its own is its productivity, and so the labour one
/// unit costs it. That individual value is a real magnitude and it is not what
/// the unit is worth — the distance between the two is where surplus profit
/// lives, and the reason anyone ever puts a machine in a workshop.
/// The form of the labour process is deliberately not checked against the
/// province's mode of production, and must not be. The machine appears in the
/// pores of the old society, and its appearing there is precisely what breaks
/// the old society up: a rule forbidding it until the mode had changed would
/// have the whole movement the wrong way round.
/// </summary>
public sealed class Branch
{
  #region Properties

  /// <summary>The commodity this branch turns out. It is also the branch's name and address.</summary>
  public CommodityId Produces { get; }

  public MeansOfProduction Means { get; private set; }

  /// <summary>The class whose labour is set in motion here.</summary>
  public SocialClass Workforce { get; }

  public int Hands { get; private set; }

  /// <summary>The working day of one hand over one period.</summary>
  public LaborTime WorkingDay { get; private set; }

  /// <summary>
  /// The part of one hand's day that reproduces the hand: the labour contained
  /// in the subsistence they must have to come back tomorrow. Held here for now,
  /// though it is no free variable — a later stage will read it off the value of
  /// the articles of consumption they actually take.
  /// </summary>
  public LaborTime NecessaryLabor { get; private set; }

  /// <summary>
  /// The productive power of labour here: how much comes out per hour worked.
  /// A technical fact about this branch — its form, its skill, its soil — and
  /// the only thing that makes one producer cheaper than another.
  /// </summary>
  public Quantity OutputPerHour { get; private set; }

  public ProductiveForm Form => Means.Form;

  /// <summary>The whole living labour the branch sets going in one period.</summary>
  public LaborTime LivingLabor => WorkingDay * Hands;

  /// <summary>
  /// Whether the surplus labour performed here is taken from those who perform
  /// it. Surplus labour is done in every society past bare subsistence; what
  /// tells the epochs apart is the form it is taken in — and whether it is taken
  /// at all. The petty producer and the associated producers keep theirs, and no
  /// other relation does.
  /// </summary>
  public bool SurplusIsPumpedOut =>
    Workforce.Profile().Relation is not (ProductionRelation.HoldsOwnMeans or ProductionRelation.CommonOwnership);

  #endregion

  #region Ctors

  private Branch(
    CommodityId produces, MeansOfProduction means, SocialClass workforce, int hands, LaborTime workingDay,
    LaborTime necessaryLabor, Quantity outputPerHour)
  {
    Produces = produces;
    Means = means;
    Workforce = workforce;
    Hands = hands;
    WorkingDay = workingDay;
    NecessaryLabor = necessaryLabor;
    OutputPerHour = outputPerHour;
  }

  #endregion

  #region Methods

  public static Branch Open(
    CommodityId produces, MeansOfProduction means, SocialClass workforce, int hands, LaborTime workingDay,
    LaborTime necessaryLabor, Quantity outputPerHour)
  {
    if (hands <= 0)
    {
      throw new DomainException("A branch is worked by someone, or it is not worked.");
    }

    if (!workforce.Profile().IsDirectProducer)
    {
      throw new DomainException($"{workforce} live on the product, they do not make it.");
    }

    RejectTooFewHandsFor(means.Form, hands);

    if (outputPerHour.IsNone)
    {
      throw new DomainException("Labour that yields nothing is not a branch of production.");
    }

    RejectImpossibleDay(workingDay, necessaryLabor);

    return new Branch(produces, means, workforce, hands, workingDay, necessaryLabor, outputPerHour);
  }

  /// <summary>
  /// Reconstitutes a branch from a stored snapshot. Unlike <see cref="Open" />
  /// this asserts no invariant: the state it receives was already valid when it
  /// was saved.
  /// </summary>
  public static Branch Load(
    CommodityId produces, MeansOfProduction means, SocialClass workforce, int hands, LaborTime workingDay,
    LaborTime necessaryLabor, Quantity outputPerHour)
  {
    return new Branch(produces, means, workforce, hands, workingDay, necessaryLabor, outputPerHour);
  }

  /// <summary>
  /// Works one period. The material laid out is consumed, the instruments wear
  /// by their share, the living labour is spent — and what comes out holds the
  /// value the means handed on plus the value the labour added, never a grain
  /// more.
  /// </summary>
  public Yield Work(CommodityRegister register)
  {
    var commodity = register.Get(Produces);

    if (commodity.UseValue.Unit != OutputPerHour.Unit)
    {
      throw new DomainException(
        $"{commodity.Name} is measured in {commodity.UseValue.Unit}, and this branch turns out {OutputPerHour.Unit}."
      );
    }

    var composition = ValueComposition.FromWorkingDay(
      Means.ValueTransferred(register), LivingLabor, NecessaryLabor * Hands
    );

    return Yield.Of(Stock.Of(Produces, OutputPerHour * LivingLabor.Hours), composition);
  }

  /// <summary>
  /// Puts new means into the branch's hands. Re-equipping and the rise in
  /// productivity that follows it are one act and not two: nobody installs a
  /// power-loom in order to weave at the old rate, and making an hour of labour
  /// yield more than it did is the whole object of the outlay.
  /// </summary>
  public void Reequip(MeansOfProduction means, Quantity outputPerHour)
  {
    if (outputPerHour.IsNone)
    {
      throw new DomainException("Labour that yields nothing is not a branch of production.");
    }

    RejectTooFewHandsFor(means.Form, Hands);

    Means = means;
    OutputPerHour = outputPerHour;
  }

  /// <summary>
  /// Fixes the length of the working day — the stake of a struggle, not a
  /// setting. Every hour added beyond necessary labour is surplus labour, and
  /// every hour a Factory Act strikes off comes out of the same place.
  /// </summary>
  public void FixTheWorkingDay(LaborTime workingDay)
  {
    RejectImpossibleDay(workingDay, NecessaryLabor);

    WorkingDay = workingDay;
  }

  /// <summary>
  /// Revises what keeping a hand alive costs: the value of labour-power, which
  /// rises and falls with the labour contained in their customary subsistence —
  /// and with how much they have managed to make customary.
  /// </summary>
  public void CostsToKeepAHand(LaborTime necessaryLabor)
  {
    RejectImpossibleDay(WorkingDay, necessaryLabor);

    NecessaryLabor = necessaryLabor;
  }

  internal void Employ(int hands)
  {
    if (hands <= 0)
    {
      throw new DomainException("A branch takes on someone, or no one.");
    }

    Hands += hands;
  }

  internal void LayOff(int hands)
  {
    if (hands <= 0)
    {
      throw new DomainException("A branch turns off someone, or no one.");
    }

    if (hands > Hands)
    {
      throw new DomainException($"There are not {hands} hands at work here to turn off.");
    }

    Hands -= hands;
  }

  private static void RejectTooFewHandsFor(ProductiveForm form, int hands)
  {
    if (form.Profile().SetsManyHandsInMotion && hands < 2)
    {
      throw new DomainException($"{form} begins where one pair of hands leaves off.");
    }
  }

  private static void RejectImpossibleDay(LaborTime workingDay, LaborTime necessaryLabor)
  {
    if (workingDay.IsNone)
    {
      throw new DomainException("A branch nobody works an hour in is not a branch of production.");
    }

    if (necessaryLabor.CompareTo(workingDay) > 0)
    {
      throw new DomainException("A day too short to reproduce the labourer consumes them instead of employing them.");
    }
  }

  public override string ToString()
  {
    return $"{Form}, {Hands} {Workforce} at {WorkingDay}";
  }

  #endregion
}
