using Surplus.Domain.GameEngine;
using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Domain.Tests.Simulation.Chronicle;

public class DayReportTests
{
  #region Tests

  [Fact]
  public void Two_reports_of_the_same_day_telling_the_same_things_are_the_same_report()
  {
    var today = GameDate.Of(1836, 1, 1);

    Assert.Equal(new DayReport(today, [new Harvest("Corn")]), new DayReport(today, [new Harvest("Corn")]));
    Assert.Equal(
      new DayReport(today, [new Harvest("Corn")]).GetHashCode(),
      new DayReport(today, [new Harvest("Corn")]).GetHashCode());
    Assert.NotEqual(new DayReport(today, [new Harvest("Corn")]), new DayReport(today, [new Harvest("Cloth")]));
    Assert.NotEqual(new DayReport(today, []), new DayReport(today.NextDay(), []));
  }

  [Fact]
  public void A_report_can_be_read_for_one_kind_of_occurrence_in_the_order_it_happened()
  {
    var report = new DayReport(
      GameDate.Of(1836, 1, 1),
      [new Harvest("Corn"), new Famine("Silesia"), new Harvest("Fibre")]);

    Assert.Equal([new Harvest("Corn"), new Harvest("Fibre")], report.Of<Harvest>());
    Assert.Equal([new Famine("Silesia")], report.Of<Famine>());
  }

  #endregion

  #region Nested types

  private sealed record Harvest(string Crop) : Occurrence;

  private sealed record Famine(string Province) : Occurrence;

  #endregion
}
