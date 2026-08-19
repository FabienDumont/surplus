using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Society;

/// <summary>A class, and how many stand in it.</summary>
public sealed record ClassPresence
{
  #region Properties

  public SocialClass Class { get; }
  public int Heads { get; }
  public ClassProfile Profile => Class.Profile();

  #endregion

  #region Ctors

  private ClassPresence(SocialClass socialClass, int heads)
  {
    Class = socialClass;
    Heads = heads;
  }

  #endregion

  #region Methods

  public static ClassPresence Of(SocialClass socialClass, int heads)
  {
    if (heads <= 0)
    {
      throw new DomainException("A class present in a province must have someone standing in it.");
    }

    return new ClassPresence(socialClass, heads);
  }

  public override string ToString()
  {
    return $"{Heads} {Class}";
  }

  #endregion
}
