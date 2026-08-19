namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// The form the labour process takes — the real axis behind "a farm, a workshop,
/// a factory". These are not buildings but stages the productive forces have
/// reached, and the order is the one Marx follows in Capital I, part IV: simple
/// cooperation, then the division of labour on a handicraft basis, then the
/// machine.
/// The form is what makes labour more or less productive, and so what makes one
/// producer's individual labour time fall below the social average. It never
/// makes value: a power-loom cheapens linen precisely by putting less labour in
/// it, not by putting more worth in it.
/// </summary>
public enum ProductiveForm
{
  /// <summary>The isolated producer and the tool in their hand.</summary>
  Handicraft,

  /// <summary>Many hands at the same work, at the same time, in the same place.</summary>
  Cooperation,

  /// <summary>Manufacture: the division of labour, still on a handicraft basis.</summary>
  Manufacture,

  /// <summary>Modern industry: the machine, and the worker as its living appendage.</summary>
  MachineIndustry
}
