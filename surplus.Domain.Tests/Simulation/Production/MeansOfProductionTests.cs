using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Surplus.Testing;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Production;

public class MeansOfProductionTests
{
  #region Fields

  private static readonly UnitOfMeasure Loom = UnitOfMeasure.Of("loom");
  private static readonly UnitOfMeasure Pound = UnitOfMeasure.Of("lb");

  #endregion

  #region Tests

  [Fact]
  public void An_instrument_cannot_wear_backwards_nor_more_than_wholly()
  {
    Assert.Throws<DomainException>(() => MeansOfProduction.SetInMotion(ProductiveForm.Manufacture, -0.1m, [], []));
    Assert.Throws<DomainException>(() => MeansOfProduction.SetInMotion(ProductiveForm.Manufacture, 1.1m, [], []));
  }

  [Fact]
  public void An_instrument_hands_on_no_more_than_it_loses_by_wear()
  {
    var (register, loom, yarn) = Weaving();

    var means = MeansOfProduction.SetInMotion(
      ProductiveForm.Manufacture, 0.02m, [Stock.Of(loom.Id, Quantity.Of(1m, Loom))],
      [Stock.Of(yarn.Id, Quantity.Of(10m, Pound))]
    );

    // The loom holds 100 hours and gives up two of them; the yarn holds ten and
    // gives up all ten, because it is gone.
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(12m)), means.ValueTransferred(register));
  }

  [Fact]
  public void Empty_hands_transfer_nothing_and_advance_nothing()
  {
    var (register, _, _) = Weaving();

    var means = MeansOfProduction.None(ProductiveForm.Handicraft);

    Assert.Equal(Value.None, means.ValueTransferred(register));
    Assert.Equal(Value.None, means.ValueAdvanced(register));
    Assert.Equal(ProductiveForm.Handicraft, means.Form);
  }

  [Fact]
  public void Means_holding_the_same_things_in_the_same_form_are_the_same_means()
  {
    var (_, loom, yarn) = Weaving();

    var means = Equipped(loom, yarn, 0.02m);

    Assert.Equal(means, Equipped(loom, yarn, 0.02m));
    Assert.Equal(means.GetHashCode(), Equipped(loom, yarn, 0.02m).GetHashCode());

    Assert.NotEqual(means, Equipped(loom, yarn, 0.03m));
    Assert.NotEqual(means, MeansOfProduction.None(ProductiveForm.Manufacture));
    Assert.False(means.Equals(null));
  }

  [Fact]
  public void Means_of_production_read_as_their_form_their_wear_and_what_they_hold()
  {
    var (_, loom, yarn) = Weaving();

    Assert.Equal(
      "Manufacture: 1 instrument(s) worn at 2%, 1 material(s)", Equipped(loom, yarn, 0.02m).ToString()
    );
  }

  [Fact]
  public void The_instruments_and_the_material_are_told_apart_and_stay_apart()
  {
    var (_, loom, yarn) = Weaving();

    var means = Equipped(loom, yarn, 0.02m);

    Assert.Equal(Stock.Of(loom.Id, Quantity.Of(1m, Loom)), Assert.Single(means.Instruments));
    Assert.Equal(Stock.Of(yarn.Id, Quantity.Of(10m, Pound)), Assert.Single(means.Subjects));
    Assert.Equal(0.02m, means.Wear);
  }

  [Fact]
  public void What_is_advanced_and_what_is_transferred_part_company_over_the_instrument()
  {
    var (register, loom, yarn) = Weaving();

    var means = MeansOfProduction.SetInMotion(
      ProductiveForm.Manufacture, 0.02m, [Stock.Of(loom.Id, Quantity.Of(1m, Loom))],
      [Stock.Of(yarn.Id, Quantity.Of(10m, Pound))]
    );

    // The whole loom is tied up in the process; only its wear enters the product.
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(110m)), means.ValueAdvanced(register));
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(12m)), means.ValueTransferred(register));
  }

  [Fact]
  public void What_the_material_holds_passes_wholly_into_the_product()
  {
    var (register, _, yarn) = Weaving();

    var means = MeansOfProduction.SetInMotion(
      ProductiveForm.Handicraft, 0m, [], [Stock.Of(yarn.Id, Quantity.Of(10m, Pound))]
    );

    Assert.Equal(means.ValueAdvanced(register), means.ValueTransferred(register));
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(10m)), means.ValueTransferred(register));
  }

  #endregion

  #region Methods

  private static MeansOfProduction Equipped(Commodity loom, Commodity yarn, decimal wear)
  {
    return MeansOfProduction.SetInMotion(
      ProductiveForm.Manufacture, wear, [Stock.Of(loom.Id, Quantity.Of(1m, Loom))],
      [Stock.Of(yarn.Id, Quantity.Of(10m, Pound))]
    );
  }

  /// <summary>A loom worth a hundred hours, and yarn worth an hour the pound.</summary>
  private static (CommodityRegister Register, Commodity Loom, Commodity Yarn) Weaving()
  {
    var loom = new CommodityBuilder().WithName("Loom").WithDepartment(Department.MeansOfProduction)
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("weaving").WithUnit(Loom).Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(100m)).Build();

    var yarn = new CommodityBuilder().WithName("Yarn").WithDepartment(Department.MeansOfProduction)
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("being woven").WithUnit(Pound).Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(1m)).Build();

    return (CommodityRegister.Of(loom, yarn), loom, yarn);
  }

  #endregion
}
