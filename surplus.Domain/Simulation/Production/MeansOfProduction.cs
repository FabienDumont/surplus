using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// The objective factors of the labour process: the subject of labour that is
/// worked on, and the instruments it is worked with.
/// Both are themselves products of earlier labour — dead labour, which living
/// labour finds lying there and sets in motion. Neither adds an hour to what it
/// already holds: a machine creates no more value than the ox that draws the
/// plough. It gives up what is in it and nothing besides, which is why every
/// hour of new value in <see cref="ValueComposition" /> comes from the living
/// factor alone.
/// The two give it up differently, and that difference is the whole of fixed and
/// circulating capital. The subject of labour passes bodily into the product and
/// surrenders its value at one stroke; the instrument keeps its shape, serves
/// process after process, and parts with its value only as fast as it wears out.
/// The instrument enters the labour process whole and the process of
/// valorisation piecemeal — which is why it is possible to lay out more than one
/// consumes, and why <see cref="ValueAdvanced" /> and
/// <see cref="ValueTransferred" /> are two different figures.
/// </summary>
public sealed record MeansOfProduction
{
  #region Fields

  private readonly List<Stock> _instruments;
  private readonly List<Stock> _subjects;

  #endregion

  #region Properties

  /// <summary>The form the labour process takes with these means.</summary>
  public ProductiveForm Form { get; }

  /// <summary>
  /// Instruments of labour: looms, ploughs, buildings, the soil itself. They
  /// serve repeatedly and are not used up in one period.
  /// </summary>
  public IReadOnlyList<Stock> Instruments => _instruments;

  /// <summary>
  /// Subjects of labour: the yarn, the ore, the seed. Raw material, which is
  /// consumed entirely in the period and reappears in the product's body.
  /// </summary>
  public IReadOnlyList<Stock> Subjects => _subjects;

  /// <summary>
  /// The share of the instruments used up in one period of production. It is
  /// their wear, and it is the exact measure of what they hand on: an instrument
  /// never gives the product more value than it loses in serving it.
  /// </summary>
  public decimal Wear { get; }

  #endregion

  #region Ctors

  private MeansOfProduction(ProductiveForm form, List<Stock> instruments, List<Stock> subjects, decimal wear)
  {
    Form = form;
    _instruments = instruments;
    _subjects = subjects;
    Wear = wear;
  }

  #endregion

  #region Methods

  public static MeansOfProduction SetInMotion(
    ProductiveForm form, decimal wear, IEnumerable<Stock> instruments, IEnumerable<Stock> subjects)
  {
    if (wear is < 0m or > 1m)
    {
      throw new DomainException(
        "An instrument wears forwards, and by no more than the whole of itself in one period."
      );
    }

    return new MeansOfProduction(form, [..instruments], [..subjects], wear);
  }

  /// <summary>Means of production bare of any instrument — labour on nature with empty hands.</summary>
  public static MeansOfProduction None(ProductiveForm form)
  {
    return new MeansOfProduction(form, [], [], 0m);
  }

  /// <summary>
  /// The whole value lying in these means: the instruments entire, however
  /// little of them this period consumes, plus the material laid out. This is
  /// what is tied up, and what a rate of profit is reckoned against when the
  /// question is what the capital has to earn on.
  /// </summary>
  public Value ValueAdvanced(CommodityRegister register)
  {
    return register.ValueOf(_instruments) + register.ValueOf(_subjects);
  }

  /// <summary>
  /// The value that actually reappears in this period's product: the whole of
  /// the material, which is gone, and only the worn share of the instruments,
  /// which are not. This, and not what is advanced, is the constant capital of
  /// the period — value that has merely changed its place, being preserved by
  /// living labour rather than created by it.
  /// </summary>
  public Value ValueTransferred(CommodityRegister register)
  {
    return register.ValueOf(_subjects) + register.ValueOf(_instruments) * Wear;
  }

  public bool Equals(MeansOfProduction? other)
  {
    return other is not null
           && Form == other.Form
           && Wear == other.Wear
           && _instruments.SequenceEqual(other._instruments)
           && _subjects.SequenceEqual(other._subjects);
  }

  public override int GetHashCode()
  {
    return _instruments.Concat(_subjects).Aggregate(HashCode.Combine(Form, Wear), HashCode.Combine);
  }

  public override string ToString()
  {
    return $"{Form}: {_instruments.Count} instrument(s) worn at {(Wear * 100m).Written()}%, " +
           $"{_subjects.Count} material(s)";
  }

  #endregion
}
