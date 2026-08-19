using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Testing;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Production;

public class BranchTests
{
  #region Fields

  private static readonly UnitOfMeasure Loom = UnitOfMeasure.Of("loom");
  private static readonly UnitOfMeasure Pound = UnitOfMeasure.Of("lb");
  private static readonly UnitOfMeasure Yard = UnitOfMeasure.Of("yard");

  #endregion

  #region Tests

  [Fact]
  public void A_branch_cannot_turn_out_what_its_commodity_is_not_measured_in()
  {
    var (register, linen, _, _) = Weaving();

    var branch = new BranchBuilder().Producing(linen.Id).WithOutputPerHour(Quantity.Of(2m, Pound)).Build();

    Assert.Throws<DomainException>(() => branch.Work(register));
  }

  [Fact]
  public void A_branch_is_worked_by_someone_or_it_is_not_worked()
  {
    Assert.Throws<DomainException>(() => Opened(hands: 0));
  }

  [Fact]
  public void A_day_too_short_to_reproduce_the_labourer_cannot_be_worked()
  {
    Assert.Throws<DomainException>(
      () => Opened(workingDay: LaborTime.FromHours(5m), necessaryLabor: LaborTime.FromHours(6m))
    );
    Assert.Throws<DomainException>(() => Opened(workingDay: LaborTime.None));
  }

  [Fact]
  public void A_period_of_work_yields_a_product_and_an_account_of_the_value_in_it()
  {
    var (register, linen, loom, yarn) = Weaving();

    // A thousand hours of loom wearing at two per cent, and a hundred pounds of
    // yarn at an hour the pound: a hundred and twenty hours of dead labour.
    var branch = Branch.Open(
      linen.Id,
      MeansOfProduction.SetInMotion(
        ProductiveForm.Manufacture, 0.02m, [Stock.Of(loom.Id, Quantity.Of(1m, Loom))],
        [Stock.Of(yarn.Id, Quantity.Of(100m, Pound))]
      ),
      SocialClass.Proletariat, 10, LaborTime.FromHours(12m), LaborTime.FromHours(6m), Quantity.Of(2m, Yard)
    );

    var yield = branch.Work(register);

    Assert.Equal(Quantity.Of(240m, Yard), yield.Product.Quantity);
    Assert.Equal("120c + 60v + 60s", yield.Composition.ToString());
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(240m)), yield.Composition.Product);

    // Two hundred and forty hours spread over two hundred and forty yards.
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(1m)), yield.IndividualValue);
    Assert.Equal(1m, yield.Composition.RateOfSurplusValue());
  }

  [Fact]
  public void Cooperation_begins_where_one_pair_of_hands_leaves_off()
  {
    Assert.Throws<DomainException>(() => Opened(form: ProductiveForm.Cooperation, hands: 1));
    Assert.Throws<DomainException>(() => Opened(form: ProductiveForm.MachineIndustry, hands: 1));

    // The isolated weaver at their own loom is a whole branch by themselves.
    Assert.Equal(1, Opened(form: ProductiveForm.Handicraft, hands: 1).Hands);
  }

  [Fact]
  public void Labor_that_yields_nothing_is_not_a_branch_of_production()
  {
    Assert.Throws<DomainException>(() => Opened(outputPerHour: Quantity.NoneOf(Yard)));
  }

  [Fact]
  public void Only_those_who_produce_work_a_branch()
  {
    Assert.Throws<DomainException>(() => Opened(workforce: SocialClass.Landowners));
    Assert.Throws<DomainException>(() => Opened(workforce: SocialClass.Merchants));

    Assert.Equal(SocialClass.Serfs, Opened(workforce: SocialClass.Serfs).Workforce);
  }

  [Fact]
  public void Re_equipping_and_the_rise_in_productivity_it_is_made_for_are_one_act()
  {
    var (register, linen, _, _) = Weaving();

    var branch = Branch.Open(
      linen.Id, MeansOfProduction.None(ProductiveForm.Handicraft), SocialClass.Proletariat, 10,
      LaborTime.FromHours(12m), LaborTime.FromHours(6m), Quantity.Of(2m, Yard)
    );

    branch.Reequip(MeansOfProduction.None(ProductiveForm.MachineIndustry), Quantity.Of(8m, Yard));

    Assert.Equal(ProductiveForm.MachineIndustry, branch.Form);
    Assert.Equal(Quantity.Of(960m, Yard), branch.Work(register).Product.Quantity);
    Assert.Throws<DomainException>(
      () => branch.Reequip(MeansOfProduction.None(ProductiveForm.Handicraft), Quantity.NoneOf(Yard))
    );
  }

  [Fact]
  public void A_branch_reads_as_its_form_its_hands_and_the_day_they_work()
  {
    var branch = new BranchBuilder().InTheFormOf(ProductiveForm.Manufacture)
      .WorkedBy(SocialClass.Journeymen, 100).Build();

    Assert.Equal("Manufacture, 100 Journeymen at 12h of labour", branch.ToString());
  }

  [Fact]
  public void The_petty_producer_keeps_the_surplus_and_no_other_relation_does()
  {
    Assert.False(Opened(workforce: SocialClass.GuildMasters).SurplusIsPumpedOut);
    Assert.False(Opened(workforce: SocialClass.AssociatedProducers).SurplusIsPumpedOut);

    Assert.True(Opened(workforce: SocialClass.Serfs).SurplusIsPumpedOut);
    Assert.True(Opened(workforce: SocialClass.Slaves).SurplusIsPumpedOut);
    Assert.True(Opened(workforce: SocialClass.Proletariat).SurplusIsPumpedOut);
  }

  [Fact]
  public void The_same_labor_yielding_more_makes_each_unit_hold_less_of_it()
  {
    var (register, linen, _, _) = Weaving();

    var handloom = new BranchBuilder().Producing(linen.Id).WorkedBy(SocialClass.Proletariat, 10)
      .WithOutputPerHour(Quantity.Of(2m, Yard)).Build();

    var powerloom = new BranchBuilder().Producing(linen.Id).WorkedBy(SocialClass.Proletariat, 10)
      .WithOutputPerHour(Quantity.Of(8m, Yard)).Build();

    // The same ten hands, the same twelve hours, the same hundred and twenty
    // hours of labour: four times the cloth, and a quarter of the labour in each
    // yard. Nothing has been made more valuable; weaving has been made cheaper.
    Assert.Equal(handloom.LivingLabor, powerloom.LivingLabor);
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(0.5m)), handloom.Work(register).IndividualValue);
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(0.125m)), powerloom.Work(register).IndividualValue);
  }

  [Fact]
  public void The_working_day_is_fixed_but_never_below_what_keeps_the_labourer()
  {
    var branch = Opened();

    branch.FixTheWorkingDay(LaborTime.FromHours(10m));
    Assert.Equal(LaborTime.FromHours(10m), branch.WorkingDay);

    Assert.Throws<DomainException>(() => branch.FixTheWorkingDay(LaborTime.FromHours(5m)));
    Assert.Throws<DomainException>(() => branch.CostsToKeepAHand(LaborTime.FromHours(11m)));

    branch.CostsToKeepAHand(LaborTime.FromHours(8m));
    Assert.Equal(LaborTime.FromHours(8m), branch.NecessaryLabor);
  }

  #endregion

  #region Methods

  private static Branch Opened(
    ProductiveForm form = ProductiveForm.Handicraft, SocialClass workforce = SocialClass.Proletariat,
    int hands = 10, LaborTime? workingDay = null, LaborTime? necessaryLabor = null, Quantity? outputPerHour = null)
  {
    return Branch.Open(
      CommodityId.New(), MeansOfProduction.None(form), workforce, hands,
      workingDay ?? LaborTime.FromHours(12m), necessaryLabor ?? LaborTime.FromHours(6m),
      outputPerHour ?? Quantity.Of(2m, Yard)
    );
  }

  /// <summary>Linen, the loom that weaves it, and the yarn it is woven from.</summary>
  private static (CommodityRegister Register, Commodity Linen, Commodity Loom, Commodity Yarn) Weaving()
  {
    var linen = Commodity(
      "Linen", "clothing material", Yard, Department.MeansOfConsumption, LaborTime.FromHours(1m)
    );
    var loom = Commodity("Loom", "weaving", Loom, Department.MeansOfProduction, LaborTime.FromHours(1_000m));
    var yarn = Commodity("Yarn", "being woven", Pound, Department.MeansOfProduction, LaborTime.FromHours(1m));

    return (CommodityRegister.Of(linen, loom, yarn), linen, loom, yarn);
  }

  private static Commodity Commodity(
    string name, string want, UnitOfMeasure unit, Department department, LaborTime perUnit)
  {
    return new CommodityBuilder().WithName(name).WithDepartment(department)
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant(want).WithUnit(unit).Build())
      .WithSociallyNecessaryLaborTime(perUnit).Build();
  }

  #endregion
}
