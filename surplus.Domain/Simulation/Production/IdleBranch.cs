namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// A branch that stood still for a day because the store could not furnish its
/// means. Living labour sets dead labour in motion only if the dead labour is
/// there: hands without yarn are not weavers for the day, whatever they are on
/// the books.
/// </summary>
public sealed record IdleBranch(ProvinceId Province, CommodityId Commodity);
