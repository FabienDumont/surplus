using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Domain.Tests.Simulation.Production;

public class TechniqueTests
{
  #region Fields

  private static readonly CommodityId Cloth = CommodityId.New();
  private static readonly CommodityId Fibre = CommodityId.New();
  private static readonly CommodityId Machinery = CommodityId.New();
  private static readonly UnitOfMeasure Machine = UnitOfMeasure.Of("machine");
  private static readonly UnitOfMeasure Pound = UnitOfMeasure.Of("pound");
  private static readonly UnitOfMeasure Yard = UnitOfMeasure.Of("yard");

  #endregion

  #region Tests

  [Fact]
  public void A_technique_must_have_a_name()
  {
    Assert.Throws<DomainException>(
      () => Technique.Of("  ", Cloth, ProductiveForm.Handicraft, Quantity.Of(0.5m, Yard), 0m, [], []));
    Assert.Equal("Power loom", PowerLoom("  Power loom ").Name);
  }

  [Fact]
  public void A_technique_that_yields_nothing_is_no_technique()
  {
    Assert.Throws<DomainException>(
      () => Technique.Of("Idle loom", Cloth, ProductiveForm.Handicraft, Quantity.NoneOf(Yard), 0m, [], []));
  }

  [Fact]
  public void A_technique_equips_each_hand_alike()
  {
    var means = PowerLoom().EquipFor(400);

    Assert.Equal(ProductiveForm.MachineIndustry, means.Form);
    Assert.Equal(0.002m, means.Wear);
    Assert.Equal([Stock.Of(Machinery, Quantity.Of(200m, Machine))], means.Instruments);
    Assert.Equal([Stock.Of(Fibre, Quantity.Of(9_600m, Pound))], means.Subjects);
  }

  [Fact]
  public void Two_techniques_stated_alike_are_the_same_technique()
  {
    Assert.Equal(PowerLoom(), PowerLoom());
    Assert.Equal(PowerLoom().GetHashCode(), PowerLoom().GetHashCode());
    Assert.NotEqual(PowerLoom(), PowerLoom("Self-acting loom"));
    Assert.NotEqual(
      PowerLoom(),
      Technique.Of(
        "Power loom", Cloth, ProductiveForm.MachineIndustry, Quantity.Of(2m, Yard), 0.002m,
        [Stock.Of(Machinery, Quantity.Of(1m, Machine))], [Stock.Of(Fibre, Quantity.Of(24m, Pound))]));
  }

  [Fact]
  public void A_technique_reads_as_its_name_its_form_and_what_an_hour_yields()
  {
    Assert.Equal("Power loom: MachineIndustry, 2 yard an hour", PowerLoom().ToString());
  }

  #endregion

  #region Methods

  private static Technique PowerLoom(string name = "Power loom")
  {
    return Technique.Of(
      name, Cloth, ProductiveForm.MachineIndustry, Quantity.Of(2m, Yard), 0.002m,
      [Stock.Of(Machinery, Quantity.Of(0.5m, Machine))], [Stock.Of(Fibre, Quantity.Of(24m, Pound))]);
  }

  #endregion
}
