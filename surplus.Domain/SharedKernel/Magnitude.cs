using System.Globalization;

namespace Surplus.Domain.SharedKernel;

/// <summary>
/// How magnitudes are written out.
/// Trailing zeros carry no meaning here: twenty hours of labour are not more
/// definite for having been arrived at by a multiplication, and 120.00 says
/// nothing 120 does not. Nor may the separator follow whoever happens to be
/// running the game — the same simulation must read the same in Paris as in
/// London.
/// </summary>
internal static class Magnitude
{
  #region Methods

  public static string Written(this decimal magnitude)
  {
    return magnitude.ToString("0.####", CultureInfo.InvariantCulture);
  }

  #endregion
}
