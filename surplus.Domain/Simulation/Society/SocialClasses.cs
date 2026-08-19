using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Society;

/// <summary>
/// The register of what each class is and which modes of production bear it.
/// Kept as one table because the definitions are historical facts about the
/// classes, not state any particular game can vary.
/// </summary>
public static class SocialClasses
{
  #region Fields

  private static readonly Dictionary<SocialClass, ClassProfile> Register = new ClassProfile[]
  {
    // Primitive communal: no antagonism, because no one owns the conditions of production.
    new(SocialClass.ClanMembers, ProductionRelation.CommonOwnership, IncomeSource.OwnProduct,
      ModeOfProduction.PrimitiveCommunal),

    // Slave: the producer is a chattel, and the whole product is the owner's.
    new(SocialClass.SlaveOwners, ProductionRelation.OwnsProducers, IncomeSource.Profit, ModeOfProduction.Slave),
    new(SocialClass.Slaves, ProductionRelation.IsOwned, IncomeSource.Maintenance, ModeOfProduction.Slave),
    new(SocialClass.Freedmen, ProductionRelation.SellsLaborPower, IncomeSource.Wages, ModeOfProduction.Slave),
    new(SocialClass.Plebeians, ProductionRelation.HoldsOwnMeans, IncomeSource.OwnProduct, ModeOfProduction.Slave),

    // Feudal: surplus taken openly, by coercion, in labour or in kind.
    new(SocialClass.FeudalLords, ProductionRelation.OwnsLand, IncomeSource.Rent, ModeOfProduction.Feudal),
    new(SocialClass.Vassals, ProductionRelation.OwnsLand, IncomeSource.Rent, ModeOfProduction.Feudal),
    new(SocialClass.Clergy, ProductionRelation.OwnsLand, IncomeSource.Tithe, ModeOfProduction.Feudal),
    new(SocialClass.Serfs, ProductionRelation.BoundToTheLand, IncomeSource.OwnProduct, ModeOfProduction.Feudal),
    new(SocialClass.FreePeasants, ProductionRelation.HoldsOwnMeans, IncomeSource.OwnProduct, ModeOfProduction.Feudal),
    new(SocialClass.GuildMasters, ProductionRelation.HoldsOwnMeans, IncomeSource.OwnProduct, ModeOfProduction.Feudal),
    new(SocialClass.Journeymen, ProductionRelation.SellsLaborPower, IncomeSource.Wages, ModeOfProduction.Feudal),
    new(SocialClass.Apprentices, ProductionRelation.SellsLaborPower, IncomeSource.Maintenance,
      ModeOfProduction.Feudal),

    // Antediluvian forms of capital: older than the capitalist mode, and they outlive every host.
    new(SocialClass.Merchants, ProductionRelation.OwnsCapital, IncomeSource.MerchantProfit,
      ModeOfProduction.Slave, ModeOfProduction.Feudal, ModeOfProduction.Capitalist),
    new(SocialClass.Usurers, ProductionRelation.OwnsCapital, IncomeSource.Interest,
      ModeOfProduction.Slave, ModeOfProduction.Feudal),

    // Capitalist: the three great classes of Capital III, ch. 52, and the strata around them.
    new(SocialClass.Bourgeoisie, ProductionRelation.OwnsCapital, IncomeSource.Profit, ModeOfProduction.Capitalist),
    new(SocialClass.Financiers, ProductionRelation.OwnsCapital, IncomeSource.Interest, ModeOfProduction.Capitalist),
    new(SocialClass.Landowners, ProductionRelation.OwnsLand, IncomeSource.Rent, ModeOfProduction.Capitalist),
    new(SocialClass.PettyBourgeoisie, ProductionRelation.HoldsOwnMeans, IncomeSource.OwnProduct,
      ModeOfProduction.Capitalist),
    new(SocialClass.Peasantry, ProductionRelation.HoldsOwnMeans, IncomeSource.OwnProduct,
      ModeOfProduction.Feudal, ModeOfProduction.Capitalist),
    new(SocialClass.Proletariat, ProductionRelation.SellsLaborPower, IncomeSource.Wages,
      ModeOfProduction.Capitalist, ModeOfProduction.Socialist),
    new(SocialClass.AgriculturalProletariat, ProductionRelation.SellsLaborPower, IncomeSource.Wages,
      ModeOfProduction.Capitalist),
    new(SocialClass.Lumpenproletariat, ProductionRelation.OutsideProduction, IncomeSource.Plunder,
      ModeOfProduction.Capitalist),

    // Socialist: the producers hold power, but the state — and so a stratum administering it — survives.
    new(SocialClass.Bureaucracy, ProductionRelation.AdministersCommonProperty, IncomeSource.Distribution,
      ModeOfProduction.Socialist),
    new(SocialClass.CollectivisedPeasantry, ProductionRelation.CommonOwnership, IncomeSource.Distribution,
      ModeOfProduction.Socialist),

    // Communist: not a class at all, strictly, since there is no other class for it to stand against.
    new(SocialClass.AssociatedProducers, ProductionRelation.CommonOwnership, IncomeSource.Distribution,
      ModeOfProduction.Communist)
  }.ToDictionary(profile => profile.Class);

  #endregion

  #region Methods

  /// <summary>What this class is, economically.</summary>
  public static ClassProfile Profile(this SocialClass socialClass)
  {
    return Register.TryGetValue(socialClass, out var profile)
      ? profile
      : throw new DomainException($"No profile is registered for {socialClass}.");
  }

  /// <summary>The classes a mode of production throws up of its own accord.</summary>
  public static IReadOnlyList<SocialClass> Classes(this ModeOfProduction mode)
  {
    return
    [
      ..Register.Values.Where(profile => profile.IsNativeTo(mode)).Select(profile => profile.Class)
    ];
  }

  #endregion
}
