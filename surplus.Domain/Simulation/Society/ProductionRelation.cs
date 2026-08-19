namespace Surplus.Domain.Simulation.Society;

/// <summary>
/// A class's relation to the conditions of production. This, and not occupation
/// or income bracket, is what defines a class: two people may both swing a
/// hammer, and belong to different classes because one owns the workshop.
/// </summary>
public enum ProductionRelation
{
  /// <summary>The producer is themselves property.</summary>
  IsOwned,

  /// <summary>Holds a plot, but is tied to it and owes labour by coercion.</summary>
  BoundToTheLand,

  /// <summary>Works conditions of production they own — the petty producer.</summary>
  HoldsOwnMeans,

  /// <summary>Owns no conditions of production, and so must sell labour-power.</summary>
  SellsLaborPower,

  /// <summary>Owns the direct producers.</summary>
  OwnsProducers,

  /// <summary>Owns the earth without working it.</summary>
  OwnsLand,

  /// <summary>Owns conditions of production and buys labour-power to set them in motion.</summary>
  OwnsCapital,

  /// <summary>Expelled from production altogether.</summary>
  OutsideProduction,

  /// <summary>Administers socialised property without owning it.</summary>
  AdministersCommonProperty,

  /// <summary>Holds the conditions of production in common with everyone else.</summary>
  CommonOwnership
}
