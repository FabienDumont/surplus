namespace Surplus.Domain.Simulation.Society;

/// <summary>
/// What a class is, economically: how it stands to the conditions of production,
/// what revenue form it lives on, and which modes of production it is native to.
/// Whether a class produces or appropriates surplus-labour is read off its
/// relation rather than stored, because it follows from it and nothing else.
/// </summary>
public sealed record ClassProfile
{
  #region Properties

  public SocialClass Class { get; }
  public ProductionRelation Relation { get; }
  public IncomeSource Income { get; }

  /// <summary>The modes of production that throw this class up of their own accord.</summary>
  public IReadOnlyList<ModeOfProduction> NativeModes { get; }

  /// <summary>Whether this class performs the labour whose surplus others live on.</summary>
  public bool IsDirectProducer =>
    Relation is ProductionRelation.IsOwned
      or ProductionRelation.BoundToTheLand
      or ProductionRelation.HoldsOwnMeans
      or ProductionRelation.SellsLaborPower
      or ProductionRelation.CommonOwnership;

  /// <summary>
  /// Whether this class can be put under arms. One relation forbids it: you
  /// cannot arm those you own, because owning them is exactly what being armed
  /// would end. Rome armed its slaves only after Cannae, and the Confederacy
  /// debated it only in 1865, when the debate itself was an admission.
  /// Every other class can be armed — which is not to say safely. Arming the
  /// class with the most reason to turn is how a state trains its gravediggers.
  /// </summary>
  public bool CanBeArmed => Relation is not ProductionRelation.IsOwned;

  /// <summary>
  /// Whether this class lives on labour it did not perform. The bureaucracy is
  /// deliberately excluded: whether its privileges make it a class or merely a
  /// caste is a question Marxists have never settled, and the simulation should
  /// not settle it by accident.
  /// </summary>
  public bool AppropriatesSurplus =>
    Relation is ProductionRelation.OwnsProducers
      or ProductionRelation.OwnsLand
      or ProductionRelation.OwnsCapital;

  #endregion

  #region Ctors

  internal ClassProfile(
    SocialClass socialClass, ProductionRelation relation, IncomeSource income, params ModeOfProduction[] nativeModes)
  {
    Class = socialClass;
    Relation = relation;
    Income = income;
    NativeModes = nativeModes;
  }

  #endregion

  #region Methods

  public bool IsNativeTo(ModeOfProduction mode)
  {
    return NativeModes.Contains(mode);
  }

  public override string ToString()
  {
    return $"{Class} ({Relation}, lives on {Income})";
  }

  #endregion
}
