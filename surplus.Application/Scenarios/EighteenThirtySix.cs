using Surplus.Domain.GameEngine;
using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Application.Scenarios;

/// <summary>
/// The one starting scenario: three states in 1836, each resting on a
/// different combination of modes of production. A capitalist Britain, a United
/// States split between a capitalist North and a slave South, and a feudal
/// Russia.
/// The figures are tuned so that every country feeds and clothes itself with a
/// small surplus. They are not history and are not meant to be.
/// Everything is built through the domain's own factories, so that a scenario
/// that breaks a rule of the domain fails to build rather than starting a game
/// the domain would never have allowed.
/// </summary>
public static class EighteenThirtySix
{
  #region Fields

  private static readonly UnitOfMeasure Kilogram = UnitOfMeasure.Of("kg");
  private static readonly UnitOfMeasure Machine = UnitOfMeasure.Of("machine");
  private static readonly UnitOfMeasure Metre = UnitOfMeasure.Of("m");
  private static readonly UnitOfMeasure Tonne = UnitOfMeasure.Of("t");

  private static readonly LaborTime WorkingDay = LaborTime.FromHours(12m);

  /// <summary>Every branch's starting figure, until the subsistence of the hands is reckoned daily.</summary>
  private static readonly LaborTime NecessaryLabor = LaborTime.FromHours(4.5m);

  #endregion

  #region Methods

  public static Scenario Build()
  {
    var register = Commodities();
    var techniques = Techniques(register);

    var world = World.Assemble(
      register, techniques, Basket(register),
      [UnitedKingdom(register, techniques), UnitedStates(register, techniques), Russia(register, techniques)]
    );

    return new Scenario("1836", GameDate.Of(1836, 1, 1), world);
  }

  /// <summary>One head's day: 25 kg of corn and half a metre of cloth.</summary>
  private static Subsistence Basket(CommodityRegister register)
  {
    return Subsistence.Of(
      Stock.Of(Named(register, "Corn").Id, Quantity.Of(0.025m, Tonne)),
      Stock.Of(Named(register, "Cloth").Id, Quantity.Of(0.5m, Metre)));
  }

  private static CommodityRegister Commodities()
  {
    return CommodityRegister.Of(
      Commodity.Produce(
        "Corn", UseValue.Of("nourishment", Tonne), Department.MeansOfConsumption, LaborTime.FromHours(120m)),
      Commodity.Produce(
        "Cloth", UseValue.Of("clothing", Metre), Department.MeansOfConsumption, LaborTime.FromHours(2m)),
      Commodity.Produce(
        "Fibre", UseValue.Of("spinning and weaving", Kilogram), Department.MeansOfProduction, LaborTime.FromHours(2m)),
      Commodity.Produce(
        "Iron", UseValue.Of("metal to be worked", Tonne), Department.MeansOfProduction, LaborTime.FromHours(100m)),
      Commodity.Produce(
        "Machinery", UseValue.Of("motive and working machinery", Machine), Department.MeansOfProduction,
        LaborTime.FromHours(2_100m))
    );
  }

  /// <summary>Every quantity per hand, per day; output per hour times twelve is output per hand per day.</summary>
  private static IReadOnlyList<Technique> Techniques(CommodityRegister register)
  {
    var corn = Named(register, "Corn").Id;
    var cloth = Named(register, "Cloth").Id;
    var fibre = Named(register, "Fibre").Id;
    var iron = Named(register, "Iron").Id;
    var machinery = Named(register, "Machinery").Id;

    return
    [
      Technique.Of("Peasant tillage", corn, ProductiveForm.Handicraft, Quantity.Of(0.00625m, Tonne), 0m, [], []),
      Technique.Of("Large farming", corn, ProductiveForm.Cooperation, Quantity.Of(0.01m, Tonne), 0m, [], []),
      Technique.Of("Fibre growing", fibre, ProductiveForm.Cooperation, Quantity.Of(0.5m, Kilogram), 0m, [], []),
      Technique.Of(
        "Hand loom", cloth, ProductiveForm.Handicraft, Quantity.Of(0.5m, Metre), 0m, [],
        [Stock.Of(fibre, Quantity.Of(3m, Kilogram))]),
      Technique.Of(
        "Power loom", cloth, ProductiveForm.MachineIndustry, Quantity.Of(2m, Metre), 0.002m,
        [Stock.Of(machinery, Quantity.Of(0.5m, Machine))], [Stock.Of(fibre, Quantity.Of(12m, Kilogram))]),
      Technique.Of("Iron working", iron, ProductiveForm.Cooperation, Quantity.Of(0.01m, Tonne), 0m, [], []),
      Technique.Of(
        "Machine shop", machinery, ProductiveForm.Manufacture, Quantity.Of(0.0005m, Machine), 0m, [],
        [Stock.Of(iron, Quantity.Of(0.006m, Tonne))])
    ];
  }

  private static Country UnitedKingdom(CommodityRegister register, IReadOnlyList<Technique> techniques)
  {
    var lancashire = Province.Establish(
      "Lancashire",
      ClassComposition.Of(
        ClassPresence.Of(SocialClass.Proletariat, 7_500),
        ClassPresence.Of(SocialClass.Bourgeoisie, 2_000),
        ClassPresence.Of(SocialClass.PettyBourgeoisie, 8_000)
      )
    );
    Work(lancashire, Named(techniques, "Power loom"), SocialClass.Proletariat, 5_000);
    Work(lancashire, Named(techniques, "Iron working"), SocialClass.Proletariat, 150);
    Work(lancashire, Named(techniques, "Machine shop"), SocialClass.Proletariat, 2_000);

    var kent = Province.Establish(
      "Kent",
      ClassComposition.Of(
        ClassPresence.Of(SocialClass.AgriculturalProletariat, 58_500),
        ClassPresence.Of(SocialClass.Landowners, 2_000),
        ClassPresence.Of(SocialClass.Peasantry, 18_000)
      )
    );
    Work(kent, Named(techniques, "Large farming"), SocialClass.AgriculturalProletariat, 45_000);
    Work(kent, Named(techniques, "Fibre growing"), SocialClass.AgriculturalProletariat, 10_500);

    var country = Country.Found(
      "United Kingdom", CountryControl.Player,
      Government.Of(GovernmentForm.ConstitutionalMonarchy, Ideology.Liberalism).Relieving(), lancashire
    );
    country.Annex(kent);

    LayUp(country, Named(register, "Corn"), 150_000m);
    LayUp(country, Named(register, "Cloth"), 3_000_000m);
    LayUp(country, Named(register, "Fibre"), 1_800_000m);
    LayUp(country, Named(register, "Iron"), 360m);
    LayUp(country, Named(register, "Machinery"), 150m);

    return country;
  }

  private static Country UnitedStates(CommodityRegister register, IReadOnlyList<Technique> techniques)
  {
    var newEngland = Province.Establish(
      "New England",
      ClassComposition.Of(
        ClassPresence.Of(SocialClass.Peasantry, 60_000),
        ClassPresence.Of(SocialClass.PettyBourgeoisie, 20_000),
        ClassPresence.Of(SocialClass.Proletariat, 1_300),
        ClassPresence.Of(SocialClass.Bourgeoisie, 1_000),
        ClassPresence.Of(SocialClass.Merchants, 4_000)
      )
    );
    Work(newEngland, Named(techniques, "Peasant tillage"), SocialClass.Peasantry, 32_000);
    Work(newEngland, Named(techniques, "Hand loom"), SocialClass.PettyBourgeoisie, 13_000);
    Work(newEngland, Named(techniques, "Iron working"), SocialClass.Proletariat, 200);
    Work(newEngland, Named(techniques, "Machine shop"), SocialClass.Proletariat, 1_000);

    var carolina = Province.Establish(
      "Carolina",
      ClassComposition.Of(
        ClassPresence.Of(SocialClass.Slaves, 40_000),
        ClassPresence.Of(SocialClass.SlaveOwners, 1_000),
        ClassPresence.Of(SocialClass.PettyBourgeoisie, 9_000)
      )
    );
    Work(carolina, Named(techniques, "Large farming"), SocialClass.Slaves, 12_000);
    Work(carolina, Named(techniques, "Fibre growing"), SocialClass.Slaves, 10_000);

    var country = Country.Found(
      "United States", CountryControl.Ai, Government.Of(GovernmentForm.Republic, Ideology.Liberalism), newEngland
    );
    country.Annex(carolina);

    LayUp(country, Named(register, "Corn"), 112_500m);
    LayUp(country, Named(register, "Cloth"), 2_250_000m);
    LayUp(country, Named(register, "Fibre"), 1_170_000m);
    LayUp(country, Named(register, "Iron"), 180m);

    return country;
  }

  private static Country Russia(CommodityRegister register, IReadOnlyList<Technique> techniques)
  {
    var muscovy = Province.Establish(
      "Muscovy",
      ClassComposition.Of(
        ClassPresence.Of(SocialClass.Serfs, 100_000),
        ClassPresence.Of(SocialClass.FeudalLords, 3_000),
        ClassPresence.Of(SocialClass.Clergy, 2_000),
        ClassPresence.Of(SocialClass.FreePeasants, 10_000),
        ClassPresence.Of(SocialClass.Merchants, 5_000)
      )
    );
    Work(muscovy, Named(techniques, "Peasant tillage"), SocialClass.Serfs, 42_000);
    Work(muscovy, Named(techniques, "Fibre growing"), SocialClass.Serfs, 8_000);
    Work(muscovy, Named(techniques, "Hand loom"), SocialClass.Serfs, 15_500);

    var ukraine = Province.Establish(
      "Ukraine",
      ClassComposition.Of(
        ClassPresence.Of(SocialClass.Serfs, 52_000),
        ClassPresence.Of(SocialClass.FeudalLords, 2_000),
        ClassPresence.Of(SocialClass.FreePeasants, 6_000)
      )
    );
    Work(ukraine, Named(techniques, "Peasant tillage"), SocialClass.Serfs, 20_000);

    var country = Country.Found(
      "Russia", CountryControl.Ai, Government.Of(GovernmentForm.AbsoluteMonarchy, Ideology.Traditionalism), muscovy
    );
    country.Annex(ukraine);

    LayUp(country, Named(register, "Corn"), 135_000m);
    LayUp(country, Named(register, "Cloth"), 2_700_000m);
    LayUp(country, Named(register, "Fibre"), 1_395_000m);

    return country;
  }

  /// <summary>
  /// Opens a branch with its instruments already standing. They were made
  /// before the game began, so they are not drawn from the starting store.
  /// </summary>
  private static void Work(Province province, Technique technique, SocialClass workforce, int hands)
  {
    province.Open(
      Branch.Open(
        technique.Produces, technique.EquipFor(hands), workforce, hands, WorkingDay, NecessaryLabor,
        technique.OutputPerHour
      )
    );
  }

  private static void LayUp(Country country, Commodity commodity, decimal amount)
  {
    country.LayUp(Stock.Of(commodity.Id, Quantity.Of(amount, commodity.UseValue.Unit)));
  }

  private static Commodity Named(CommodityRegister register, string name)
  {
    return register.Commodities.Single(commodity => commodity.Name == name);
  }

  private static Technique Named(IReadOnlyList<Technique> techniques, string name)
  {
    return techniques.Single(technique => technique.Name == name);
  }

  #endregion
}
