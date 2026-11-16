using Surplus.Application.Reporting;
using Surplus.Domain.Simulation.Production;
using Surplus.Testing;

namespace Surplus.Application.Tests.Reporting;

public class MasteryTests
{
  #region Fields

  private static readonly Technique HandLoom = Technique.Of(
    "Hand loom", CommodityId.New(), ProductiveForm.Handicraft, Quantity.Of(0.5m, UnitOfMeasure.Of("m")), 0m, [], []);

  private static readonly Technique PowerLoom = Technique.Of(
      "Power loom", CommodityId.New(), ProductiveForm.MachineIndustry, Quantity.Of(2m, UnitOfMeasure.Of("m")), 0m, [], [])
    .DevelopedFrom(LaborTime.FromHours(20_000_000m), "Hand loom", "Machine shop");

  #endregion

  #region Tests

  [Fact]
  public void A_known_technique_reads_as_known()
  {
    Assert.Equal("known", Mastery.Of(new CountryBuilder().Knowing("Hand loom").Build(), HandLoom));
  }

  [Fact]
  public void An_elementary_technique_a_country_lacks_is_simply_not_known()
  {
    Assert.Equal("not known", Mastery.Of(new CountryBuilder().Build(), HandLoom));
  }

  [Fact]
  public void A_technique_whose_prerequisites_are_missing_names_them()
  {
    Assert.Equal("out of reach until Machine shop is known", Mastery.Of(new CountryBuilder().Knowing("Hand loom").Build(), PowerLoom));
    Assert.Equal("out of reach until Hand loom and Machine shop are known", Mastery.Of(new CountryBuilder().Build(), PowerLoom));
  }

  [Fact]
  public void A_technique_under_way_reads_as_the_share_found_so_far_rounded_down()
  {
    var country = new CountryBuilder()
      .WithKnowledge(Knowledge.Of(["Hand loom", "Machine shop"], [new Development("Power loom", LaborTime.FromHours(8_499_999m))]))
      .Build();

    Assert.Equal("under way, 42%", Mastery.Of(country, PowerLoom));
  }

  #endregion
}
