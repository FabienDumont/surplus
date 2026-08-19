using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// The natural unit in which a use-value is counted or measured
/// (yards of linen, single coats, quarters of wheat).
/// Each use-value carries its own unit; units of different use-values
/// are not interchangeable.
/// </summary>
public sealed record UnitOfMeasure
{
  #region Properties

  public string Name { get; }

  #endregion

  #region Ctors

  private UnitOfMeasure(string name)
  {
    Name = name;
  }

  #endregion

  #region Methods

  public static UnitOfMeasure Of(string name)
  {
    return string.IsNullOrWhiteSpace(name)
      ? throw new DomainException("A unit of measure must have a name.")
      : new UnitOfMeasure(name.Trim());
  }

  public override string ToString()
  {
    return Name;
  }

  #endregion
}
