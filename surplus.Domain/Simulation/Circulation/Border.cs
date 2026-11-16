namespace Surplus.Domain.Simulation.Circulation;

/// <summary>
/// Two provinces that touch. A border has no direction: goods cross it either
/// way, so a border from Kent to Lancashire is the border from Lancashire to
/// Kent.
/// </summary>
public sealed record Border(ProvinceId One, ProvinceId Other)
{
  #region Methods

  public bool Joins(ProvinceId province)
  {
    return One == province || Other == province;
  }

  public bool Equals(Border? other)
  {
    return other is not null &&
           ((One == other.One && Other == other.Other) || (One == other.Other && Other == other.One));
  }

  public override int GetHashCode()
  {
    return One.GetHashCode() ^ Other.GetHashCode();
  }

  #endregion
}
