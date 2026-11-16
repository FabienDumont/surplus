using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;

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

  [Fact]
  public void An_elementary_technique_grows_out_of_no_other()
  {
    var loom = PowerLoom();

    Assert.True(loom.IsElementary);
    Assert.Empty(loom.Requires);
    Assert.Equal(LaborTime.None, loom.Development);
  }

  [Fact]
  public void A_developed_technique_grows_out_of_those_it_names_for_the_labour_it_costs()
  {
    var loom = PowerLoom().DevelopedFrom(LaborTime.FromHours(20_000_000m), "Hand loom", "Machine shop");

    Assert.False(loom.IsElementary);
    Assert.Equal(["Hand loom", "Machine shop"], loom.Requires);
    Assert.Equal(LaborTime.FromHours(20_000_000m), loom.Development);
    Assert.Equal(PowerLoom().OutputPerHour, loom.OutputPerHour);
  }

  [Fact]
  public void A_technique_developed_from_nothing_is_no_development()
  {
    Assert.Throws<DomainException>(() => PowerLoom().DevelopedFrom(LaborTime.FromHours(1m)));
  }

  [Fact]
  public void A_technique_that_costs_no_labour_to_find_is_no_development()
  {
    Assert.Throws<DomainException>(() => PowerLoom().DevelopedFrom(LaborTime.None, "Hand loom"));
  }

  [Fact]
  public void A_technique_cannot_grow_out_of_the_same_one_twice()
  {
    Assert.Throws<DomainException>(() => PowerLoom().DevelopedFrom(LaborTime.FromHours(1m), "Hand loom", "Hand loom"));
  }

  [Fact]
  public void A_technique_cannot_grow_out_of_itself()
  {
    Assert.Throws<DomainException>(() => PowerLoom().DevelopedFrom(LaborTime.FromHours(1m), "Power loom"));
  }

  [Fact]
  public void Techniques_developed_differently_are_different_techniques()
  {
    var fromHandLoom = PowerLoom().DevelopedFrom(LaborTime.FromHours(1m), "Hand loom");

    Assert.NotEqual(PowerLoom(), fromHandLoom);
    Assert.NotEqual(fromHandLoom, PowerLoom().DevelopedFrom(LaborTime.FromHours(2m), "Hand loom"));
    Assert.NotEqual(fromHandLoom, PowerLoom().DevelopedFrom(LaborTime.FromHours(1m), "Jenny"));
    Assert.Equal(fromHandLoom, PowerLoom().DevelopedFrom(LaborTime.FromHours(1m), "Hand loom"));
    Assert.Equal(fromHandLoom.GetHashCode(), PowerLoom().DevelopedFrom(LaborTime.FromHours(1m), "Hand loom").GetHashCode());
  }

  [Fact]
  public void A_proletarian_can_work_a_power_loom()
  {
    Assert.True(PowerLoom().CanBeWorkedBy(SocialClass.Proletariat));
  }

  [Fact]
  public void A_serf_cannot_work_a_power_loom()
  {
    Assert.False(PowerLoom().CanBeWorkedBy(SocialClass.Serfs));
  }

  [Fact]
  public void A_slave_cannot_work_a_power_loom()
  {
    Assert.False(PowerLoom().CanBeWorkedBy(SocialClass.Slaves));
  }

  [Fact]
  public void A_serf_can_work_by_hand_and_in_cooperation()
  {
    var handLoom = Technique.Of("Hand loom", Cloth, ProductiveForm.Handicraft, Quantity.Of(0.5m, Yard), 0m, [], []);
    var farming = Technique.Of("Large farming", Fibre, ProductiveForm.Cooperation, Quantity.Of(1m, Pound), 0m, [], []);

    Assert.True(handLoom.CanBeWorkedBy(SocialClass.Serfs));
    Assert.True(farming.CanBeWorkedBy(SocialClass.Serfs));
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
