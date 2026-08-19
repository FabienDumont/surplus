namespace Surplus.Domain.Simulation.Commodities;

/// <summary>
/// The place a commodity takes in social reproduction — the great division of
/// the total social product in Capital II, ch. 20: what re-enters production,
/// and what is consumed and never comes back.
/// The line is one of destination, not of nature: the same coal warms a parlour
/// and fires a furnace, the same corn feeds a labourer and seeds a field. It is
/// which consumption the thing enters that decides, which is why this is
/// declared of a commodity rather than read off its body.
/// </summary>
public enum Department
{
  /// <summary>
  /// Department I: iron, coal, machines, seed. Consumed productively — it
  /// disappears into a new product and its value reappears there.
  /// </summary>
  MeansOfProduction,

  /// <summary>
  /// Department II: bread, cloth, dwellings. Consumed individually — the labourer
  /// consumes it to reproduce their labour-power, the capitalist to live.
  /// </summary>
  MeansOfConsumption
}
