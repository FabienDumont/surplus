using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Circulation;

/// <summary>
/// What carrying goods costs: which commodity carriage is, and how much of it
/// each other commodity takes up. Carriage is a product like any other. Carters
/// make it, the world values it, and it is used up in the moving. It cannot be
/// stored, because a cart-day not used today is gone.
/// </summary>
public sealed record Haulage
{
  #region Fields

  /// <summary>A world where goods take up no room and need no carts: nothing limits what crosses a border.</summary>
  public static readonly Haulage None = new(null, []);

  private readonly List<Bulk> _bulk;

  #endregion

  #region Properties

  /// <summary>The commodity carters make, counted in loads; null when nothing needs carrying.</summary>
  public CommodityId? Carriage { get; }

  public IReadOnlyList<Bulk> Bulk => _bulk;

  #endregion

  #region Ctors

  private Haulage(CommodityId? carriage, List<Bulk> bulk)
  {
    Carriage = carriage;
    _bulk = bulk;
  }

  #endregion

  #region Methods

  public static Haulage Of(CommodityId carriage, IEnumerable<Bulk> bulk)
  {
    List<Bulk> listed = [.. bulk];

    if (listed.Any(entry => entry.LoadsPerUnit < 0m))
    {
      throw new DomainException("Nothing takes up less than no room.");
    }

    if (listed.GroupBy(entry => entry.Commodity).Any(group => group.Count() > 1))
    {
      throw new DomainException("A commodity takes up one amount of room, not two.");
    }

    if (listed.Any(entry => entry.Commodity == carriage))
    {
      throw new DomainException("Carriage does not carry itself.");
    }

    return new Haulage(carriage, listed);
  }

  /// <summary>The loads one unit of a commodity takes up; nothing for a commodity nobody listed.</summary>
  public decimal LoadsPer(CommodityId commodity)
  {
    return _bulk.FirstOrDefault(entry => entry.Commodity == commodity)?.LoadsPerUnit ?? 0m;
  }

  public bool Equals(Haulage? other)
  {
    return other is not null && Carriage == other.Carriage && _bulk.SequenceEqual(other._bulk);
  }

  public override int GetHashCode()
  {
    return _bulk.Aggregate(Carriage.GetHashCode(), HashCode.Combine);
  }

  #endregion
}
