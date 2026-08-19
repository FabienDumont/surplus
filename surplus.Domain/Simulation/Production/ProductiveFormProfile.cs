using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// What a form of the labour process is: whether it rests on combined labour,
/// who holds the instrument, and which modes of production have actually borne
/// it. These are historical facts about the form, not state a game can vary.
/// </summary>
public sealed record ProductiveFormProfile
{
  #region Properties

  public ProductiveForm Form { get; }

  /// <summary>
  /// Whether the form rests on the combined labour of many. The moment several
  /// hands work together their labour ceases to be a sum of individual labours
  /// and becomes a social force — one that costs the buyer of labour-power
  /// nothing, since it is bought by the head and appears only in the mass.
  /// </summary>
  public bool SetsManyHandsInMotion { get; }

  /// <summary>
  /// Whether the labourer wields the instrument or serves it. Up to and
  /// including manufacture the tool is the worker's; in the factory the worker
  /// is the machine's, and the relation is stood on its head.
  /// </summary>
  public bool WorkerWieldsTheInstrument { get; }

  /// <summary>The modes of production that have actually carried this form.</summary>
  public IReadOnlyList<ModeOfProduction> NativeModes { get; }

  /// <summary>
  /// Whether capital has revolutionised the technical basis itself rather than
  /// merely taking over a labour process it found ready-made — real as against
  /// formal subsumption. It is read off who holds the instrument, because that
  /// reversal is exactly what the transformation consists in.
  /// </summary>
  public bool RevolutionisesTheLaborProcess => !WorkerWieldsTheInstrument;

  #endregion

  #region Ctors

  internal ProductiveFormProfile(
    ProductiveForm form, bool setsManyHandsInMotion, bool workerWieldsTheInstrument,
    params ModeOfProduction[] nativeModes)
  {
    Form = form;
    SetsManyHandsInMotion = setsManyHandsInMotion;
    WorkerWieldsTheInstrument = workerWieldsTheInstrument;
    NativeModes = nativeModes;
  }

  #endregion

  #region Methods

  public bool IsNativeTo(ModeOfProduction mode)
  {
    return NativeModes.Contains(mode);
  }

  public override string ToString()
  {
    return $"{Form} ({(WorkerWieldsTheInstrument ? "the tool is the worker's" : "the worker is the machine's")})";
  }

  #endregion
}
