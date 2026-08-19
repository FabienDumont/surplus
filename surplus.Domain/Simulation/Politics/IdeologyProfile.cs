using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Politics;

/// <summary>
/// What an ideology is, materially: the class whose interest it advances, the
/// mode of production it works toward, its reading of the bureaucracy, and how
/// forcefully it can drive a class structure to change.
/// </summary>
public sealed record IdeologyProfile
{
  #region Properties

  public Ideology Ideology { get; }

  /// <summary>The class whose interest this ideology advances, whatever it says of itself.</summary>
  public SocialClass ServesClass { get; }

  /// <summary>The mode of production it works toward — which for fascism is the one it already has.</summary>
  public ModeOfProduction DrivesToward { get; }

  /// <summary>Its position on the bureaucracy of a workers' state, held as a position and not as a fact.</summary>
  public BureaucracyDoctrine BureaucracyDoctrine { get; }

  /// <summary>Whether it holds history to be driven by the struggle of classes.</summary>
  public bool RecognisesClassStruggle { get; }

  /// <summary>
  /// How forcefully this ideology can drive one class into another, from 1 to 5.
  /// The tendencies that act consciously and by campaign on the laws of motion
  /// move a class structure faster than those content to let a market do it.
  /// </summary>
  public int MobilisingPower { get; }

  #endregion

  #region Ctors

  internal IdeologyProfile(
    Ideology ideology,
    SocialClass servesClass,
    ModeOfProduction drivesToward,
    BureaucracyDoctrine bureaucracyDoctrine,
    bool recognisesClassStruggle,
    int mobilisingPower)
  {
    Ideology = ideology;
    ServesClass = servesClass;
    DrivesToward = drivesToward;
    BureaucracyDoctrine = bureaucracyDoctrine;
    RecognisesClassStruggle = recognisesClassStruggle;
    MobilisingPower = mobilisingPower;
  }

  #endregion

  #region Methods

  public override string ToString()
  {
    return $"{Ideology} (serves {ServesClass}, drives toward {DrivesToward})";
  }

  #endregion
}
