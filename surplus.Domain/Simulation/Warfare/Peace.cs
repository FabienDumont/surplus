using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Warfare;

/// <summary>The terms a war is ended on.</summary>
public sealed record Peace
{
  #region Fields

  private readonly List<ProvinceId> _ceded;

  #endregion

  #region Properties

  public PeaceTerms Terms { get; }

  /// <summary>The provinces changing hands — empty for a white peace or an annexation.</summary>
  public IReadOnlyList<ProvinceId> Ceded => _ceded;

  #endregion

  #region Ctors

  private Peace(PeaceTerms terms, List<ProvinceId> ceded)
  {
    Terms = terms;
    _ceded = ceded;
  }

  #endregion

  #region Methods

  /// <summary>The guns stop and nothing changes hands.</summary>
  public static Peace White()
  {
    return new Peace(PeaceTerms.White, []);
  }

  public static Peace Ceding(params ProvinceId[] provinces)
  {
    if (provinces.Length == 0)
    {
      throw new DomainException("A peace ceding nothing is a white peace.");
    }

    if (provinces.Distinct().Count() != provinces.Length)
    {
      throw new DomainException("The same province cannot be ceded twice.");
    }

    return new Peace(PeaceTerms.Cession, [..provinces]);
  }

  /// <summary>The defeated state is swallowed whole.</summary>
  public static Peace Annexation()
  {
    return new Peace(PeaceTerms.Annexation, []);
  }

  public bool Equals(Peace? other)
  {
    return other is not null
           && Terms == other.Terms
           && _ceded.Count == other._ceded.Count
           && _ceded.All(other._ceded.Contains);
  }

  public override int GetHashCode()
  {
    return _ceded.Aggregate(Terms.GetHashCode(), (hash, province) => hash ^ province.GetHashCode());
  }

  public override string ToString()
  {
    return Terms switch
    {
      PeaceTerms.White => "white peace",
      PeaceTerms.Cession => $"cession of {_ceded.Count} {(_ceded.Count == 1 ? "province" : "provinces")}",
      _ => "annexation"
    };
  }

  #endregion
}
