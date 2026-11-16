using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// A known way of producing one commodity: the form of the labour process, the
/// productivity it gives an hour of labour, and the means it puts in each pair
/// of hands.
/// It is stated per hand because the technical composition of capital is a
/// ratio and not a size. A power-loom shed of forty hands and one of four
/// hundred work by the same technique, and differ only in how many hands it is
/// applied to.
/// Productivity is a property of the technique and of nothing else. No one sets
/// how much a branch yields an hour: they choose among the ways of working
/// already known, and the yield comes with the means. Raising it means
/// installing different instruments, which have to have been made first.
/// </summary>
public sealed record Technique
{
  #region Fields

  private readonly List<Stock> _instrumentsPerHand;
  private readonly List<Stock> _materialPerHand;
  private readonly List<string> _requires;

  #endregion

  #region Properties

  public string Name { get; }
  public CommodityId Produces { get; }
  public ProductiveForm Form { get; }

  /// <summary>What one hour of one hand's labour yields under this technique.</summary>
  public Quantity OutputPerHour { get; }

  /// <summary>The share of the instruments used up in one day.</summary>
  public decimal Wear { get; }

  /// <summary>The instruments each hand is set to work at.</summary>
  public IReadOnlyList<Stock> InstrumentsPerHand => _instrumentsPerHand;

  /// <summary>The material each hand works up in one day.</summary>
  public IReadOnlyList<Stock> MaterialPerHand => _materialPerHand;

  /// <summary>
  /// The techniques this one grows out of, by name, in the order given. Empty
  /// for an elementary technique.
  /// </summary>
  public IReadOnlyList<string> Requires => _requires;

  /// <summary>
  /// The labour that must be spent in the trades it grows out of before this
  /// way of working is found. None for an elementary technique.
  /// </summary>
  public LaborTime Development { get; }

  /// <summary>
  /// Whether this technique grows out of no other. A country either knows it
  /// from the start or never finds it out by working.
  /// </summary>
  public bool IsElementary => _requires.Count == 0;

  #endregion

  #region Ctors

  private Technique(
    string name, CommodityId produces, ProductiveForm form, Quantity outputPerHour, decimal wear,
    List<Stock> instrumentsPerHand, List<Stock> materialPerHand, List<string> requires, LaborTime development)
  {
    Name = name;
    Produces = produces;
    Form = form;
    OutputPerHour = outputPerHour;
    Wear = wear;
    _instrumentsPerHand = instrumentsPerHand;
    _materialPerHand = materialPerHand;
    _requires = requires;
    Development = development;
  }

  #endregion

  #region Methods

  public static Technique Of(
    string name, CommodityId produces, ProductiveForm form, Quantity outputPerHour, decimal wear,
    IEnumerable<Stock> instrumentsPerHand, IEnumerable<Stock> materialPerHand)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new DomainException("A technique must have a name.");
    }

    if (outputPerHour.IsNone)
    {
      throw new DomainException("A way of working that yields nothing is no technique of production.");
    }

    return new Technique(
      name.Trim(), produces, form, outputPerHour, wear, [.. instrumentsPerHand], [.. materialPerHand], [],
      LaborTime.None
    );
  }

  /// <summary>
  /// The same technique, found by working in the trades of those it is
  /// developed from, once so much labour has gone into them.
  /// </summary>
  public Technique DevelopedFrom(LaborTime development, params string[] prerequisites)
  {
    if (prerequisites.Length == 0)
    {
      throw new DomainException($"{Name} must grow out of some technique, or it is elementary.");
    }

    if (development.IsNone)
    {
      throw new DomainException($"{Name} cannot be found for no labour at all.");
    }

    if (prerequisites.Distinct().Count() != prerequisites.Length)
    {
      throw new DomainException($"{Name} cannot grow out of the same technique twice.");
    }

    if (prerequisites.Contains(Name))
    {
      throw new DomainException($"{Name} cannot grow out of itself.");
    }

    return new Technique(
      Name, Produces, Form, OutputPerHour, Wear, [.. _instrumentsPerHand], [.. _materialPerHand], [.. prerequisites],
      development);
  }

  /// <summary>
  /// Whether a class can work by this technique: whether some mode of
  /// production both throws the class up and has borne the form of labour the
  /// technique calls for. A form is borne only by certain relations. Serfs are
  /// bound to the land and owe their labour by coercion, while machine industry
  /// presupposes labour-power bought by the day, so it cannot stand on serfdom.
  /// This is the fetter the productive forces meet in the old relations: the
  /// class has to change before the machine can be installed.
  /// </summary>
  public bool CanBeWorkedBy(SocialClass workforce)
  {
    var form = Form.Profile();

    return workforce.Profile().NativeModes.Any(form.IsNativeTo);
  }

  /// <summary>The means this technique puts in the hands of so many.</summary>
  public MeansOfProduction EquipFor(int hands)
  {
    return MeansOfProduction.SetInMotion(
      Form, Wear,
      _instrumentsPerHand.Select(instrument => Stock.Of(instrument.Commodity, instrument.Quantity * hands)),
      _materialPerHand.Select(material => Stock.Of(material.Commodity, material.Quantity * hands))
    );
  }

  public bool Equals(Technique? other)
  {
    return other is not null
           && Name == other.Name
           && Produces == other.Produces
           && Form == other.Form
           && OutputPerHour == other.OutputPerHour
           && Wear == other.Wear
           && _instrumentsPerHand.SequenceEqual(other._instrumentsPerHand)
           && _materialPerHand.SequenceEqual(other._materialPerHand)
           && _requires.SequenceEqual(other._requires)
           && Development == other.Development;
  }

  public override int GetHashCode()
  {
    return _requires.Aggregate(
      _instrumentsPerHand.Concat(_materialPerHand)
        .Aggregate(HashCode.Combine(Name, Produces, Form, OutputPerHour, Wear, Development), HashCode.Combine),
      HashCode.Combine);
  }

  public override string ToString()
  {
    return $"{Name}: {Form}, {OutputPerHour} an hour";
  }

  #endregion
}
