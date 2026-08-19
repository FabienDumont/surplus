using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Politics;

/// <summary>
/// What each law forbids, and what those it frees become.
/// A statute never simply deletes a class. Britain abolished slavery in 1833
/// and put the freed under "apprenticeship" until 1838; the Thirteenth
/// Amendment abolished it "except as a punishment for crime", and convict
/// leasing walked through the gap; Russia emancipated the serfs in 1861 and
/// bound them to redemption payments for half a century. In every case the
/// producers were freed of one relation and delivered into another, and which
/// one depended on whether those who had appropriated their labour still held
/// the land when the law came into force.
/// </summary>
public static class Laws
{
  #region Fields

  private static readonly Dictionary<Law, IReadOnlyList<ProductionRelation>> Forbidden = new()
  {
    // Abolition strikes at both sides of the relation: the chattel is freed,
    // and the planter, keeping his acres, becomes a landlord.
    [Law.AbolitionOfSlavery] = [ProductionRelation.IsOwned, ProductionRelation.OwnsProducers],
    [Law.SerfEmancipation] = [ProductionRelation.BoundToTheLand],

    // Enclosure is a law too, and the most candid of them: it extinguishes
    // customary right and turns those who held by it into men with nothing to sell.
    [Law.EnclosureActs] = [ProductionRelation.HoldsOwnMeans],
    [Law.LandReform] = [ProductionRelation.OwnsLand],
    [Law.NationalisationOfIndustry] = [ProductionRelation.OwnsCapital],
    [Law.AbolitionOfWageLabour] = [ProductionRelation.SellsLaborPower]
  };

  /// <summary>
  /// What a class freed from an outlawed relation becomes when the old
  /// appropriators keep their hold on the conditions of production.
  /// </summary>
  private static readonly Dictionary<ProductionRelation, SocialClass> UnderTheOldMasters = new()
  {
    [ProductionRelation.IsOwned] = SocialClass.Freedmen,
    [ProductionRelation.BoundToTheLand] = SocialClass.AgriculturalProletariat,
    [ProductionRelation.HoldsOwnMeans] = SocialClass.Proletariat,
    [ProductionRelation.SellsLaborPower] = SocialClass.AssociatedProducers,
    [ProductionRelation.OwnsProducers] = SocialClass.Landowners,
    [ProductionRelation.OwnsLand] = SocialClass.PettyBourgeoisie,
    [ProductionRelation.OwnsCapital] = SocialClass.PettyBourgeoisie,
    [ProductionRelation.OutsideProduction] = SocialClass.Proletariat,
    [ProductionRelation.AdministersCommonProperty] = SocialClass.AssociatedProducers,
    [ProductionRelation.CommonOwnership] = SocialClass.PettyBourgeoisie
  };

  /// <summary>
  /// What it becomes instead when no class is left standing over it — when the
  /// land was taken from the appropriators along with their title to the labour.
  /// </summary>
  private static readonly Dictionary<ProductionRelation, SocialClass> WithTheLandTaken = new()
  {
    [ProductionRelation.IsOwned] = SocialClass.Plebeians,
    [ProductionRelation.BoundToTheLand] = SocialClass.Peasantry,
    [ProductionRelation.HoldsOwnMeans] = SocialClass.PettyBourgeoisie,
    [ProductionRelation.SellsLaborPower] = SocialClass.AssociatedProducers,
    [ProductionRelation.OwnsProducers] = SocialClass.PettyBourgeoisie,
    [ProductionRelation.OwnsLand] = SocialClass.PettyBourgeoisie,
    [ProductionRelation.OwnsCapital] = SocialClass.Proletariat,
    [ProductionRelation.OutsideProduction] = SocialClass.AssociatedProducers,
    [ProductionRelation.AdministersCommonProperty] = SocialClass.AssociatedProducers,
    [ProductionRelation.CommonOwnership] = SocialClass.PettyBourgeoisie
  };

  #endregion

  #region Methods

  /// <summary>The relations this law puts outside the law.</summary>
  public static IReadOnlyList<ProductionRelation> Forbids(this Law law)
  {
    return Forbidden.TryGetValue(law, out var relations)
      ? relations
      : throw new DomainException($"No profile is registered for {law}.");
  }

  public static bool Forbids(this Law law, ProductionRelation relation)
  {
    return law.Forbids().Contains(relation);
  }

  /// <summary>
  /// What those standing in an outlawed relation become. The same statute
  /// yields opposite results depending on the class structure it lands in:
  /// abolish slavery where the planters keep the soil and you get a rural
  /// proletariat; abolish it where they have been expropriated and you get
  /// smallholders.
  /// </summary>
  public static SocialClass Frees(ProductionRelation relation, bool appropriatorsSurvive)
  {
    var table = appropriatorsSurvive ? UnderTheOldMasters : WithTheLandTaken;

    return table.TryGetValue(relation, out var successor)
      ? successor
      : throw new DomainException($"Nothing is known of what {relation} resolves into.");
  }

  #endregion
}
