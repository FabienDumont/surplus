using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Testing;

/// <summary>
/// Builds a <see cref="Branch" /> directly in whatever state a test needs,
/// bypassing the checks <see cref="Branch.Open" /> makes when it is set going.
/// Defaults to a handicraft weaving branch: a hundred journeymen at their own
/// looms, twelve hours a day, half of it worked for themselves.
/// </summary>
public sealed class BranchBuilder
{
  #region Fields

  private int _hands = 100;
  private MeansOfProduction _means = MeansOfProduction.None(ProductiveForm.Handicraft);
  private LaborTime _necessaryLabor = LaborTime.FromHours(6m);
  private Quantity _outputPerHour = Quantity.Of(2m, UnitOfMeasure.Of("yard"));
  private CommodityId _produces = CommodityId.New();
  private SocialClass _workforce = SocialClass.Journeymen;
  private LaborTime _workingDay = LaborTime.FromHours(12m);

  #endregion

  #region Methods

  public BranchBuilder Producing(CommodityId produces)
  {
    _produces = produces;

    return this;
  }

  public BranchBuilder WithMeans(MeansOfProduction means)
  {
    _means = means;

    return this;
  }

  /// <summary>Shorthand for means bare of any instrument, in the given form.</summary>
  public BranchBuilder InTheFormOf(ProductiveForm form)
  {
    return WithMeans(MeansOfProduction.None(form));
  }

  public BranchBuilder WorkedBy(SocialClass workforce, int hands)
  {
    _workforce = workforce;
    _hands = hands;

    return this;
  }

  public BranchBuilder WithOutputPerHour(Quantity outputPerHour)
  {
    _outputPerHour = outputPerHour;

    return this;
  }

  public Branch Build()
  {
    return Branch.Load(_produces, _means, _workforce, _hands, _workingDay, _necessaryLabor, _outputPerHour);
  }

  #endregion
}
