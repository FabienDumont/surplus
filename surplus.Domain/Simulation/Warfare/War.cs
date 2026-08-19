using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Warfare;

/// <summary>
/// Aggregate root: a war between two states, and the record of how far it has
/// actually gone.
/// It is decided by no general. Engels: "it is not the 'free creations of the
/// mind' of generals of genius which have revolutionised war, but the invention
/// of better weapons and changes in the human material, the soldiers." What is
/// weighed here is therefore what each society can put under arms, which is
/// settled by its class structure and nothing else.
/// Momentum runs from -100 to 100 and is what limits the peace: a state that has
/// barely prevailed may take a province, and only a state that has broken its
/// enemy outright may swallow it whole.
/// </summary>
public sealed class War
{
  #region Fields

  /// <summary>Momentum needed before a single province can be demanded.</summary>
  private const int PerProvince = 25;

  /// <summary>Momentum needed to swallow a state whole.</summary>
  private const int ToAnnex = 100;

  private const int MostGroundInOneTurn = 20;

  #endregion

  #region Properties

  public WarId Id { get; }
  public CountryId Aggressor { get; }
  public CountryId Defender { get; }

  /// <summary>Positive while the aggressor is prevailing, negative while the defender is.</summary>
  public int Momentum { get; private set; }

  public Peace? Peace { get; private set; }
  public bool IsOver => Peace is not null;

  /// <summary>Whoever is currently ahead, if either is.</summary>
  public CountryId? Prevailing => Momentum switch
  {
    > 0 => Aggressor,
    < 0 => Defender,
    _ => null
  };

  /// <summary>How many provinces the fighting has so far put within reach.</summary>
  public int ProvincesWithinReach => Math.Abs(Momentum) / PerProvince;

  #endregion

  #region Ctors

  private War(WarId id, CountryId aggressor, CountryId defender, int momentum, Peace? peace)
  {
    Id = id;
    Aggressor = aggressor;
    Defender = defender;
    Momentum = momentum;
    Peace = peace;
  }

  #endregion

  #region Methods

  public static War Declare(CountryId aggressor, CountryId defender)
  {
    if (aggressor == defender)
    {
      throw new DomainException("A state cannot make war on itself.");
    }

    return new War(WarId.New(), aggressor, defender, 0, null);
  }

  /// <summary>
  /// Reconstitutes a war from a stored snapshot. Unlike <see cref="Declare" />
  /// this asserts no invariant: the state it receives was already valid when it
  /// was saved.
  /// </summary>
  public static War Load(WarId id, CountryId aggressor, CountryId defender, int momentum, Peace? peace)
  {
    return new War(id, aggressor, defender, momentum, peace);
  }

  /// <summary>
  /// One turn of fighting. Ground is given up in proportion to how badly one
  /// side is outweighed, so an even match moves nothing and a hopeless one
  /// collapses in a few turns.
  /// </summary>
  public void Fight(int aggressorStrength, int defenderStrength)
  {
    if (IsOver)
    {
      throw new DomainException("The fighting is over; there is a peace on the table.");
    }

    if (aggressorStrength < 0 || defenderStrength < 0)
    {
      throw new DomainException("A state cannot field fewer than no one.");
    }

    var heaviest = Math.Max(aggressorStrength, defenderStrength);

    if (heaviest == 0)
    {
      return;
    }

    var ground = MostGroundInOneTurn * (aggressorStrength - defenderStrength) / (decimal)heaviest;

    Momentum = Math.Clamp(Momentum + (int)Math.Round(ground), -ToAnnex, ToAnnex);
  }

  /// <summary>
  /// Whether the fighting has earned these terms. A white peace is always
  /// within reach — either side may simply stop — but nothing is taken that
  /// was not won.
  /// </summary>
  public bool Permits(Peace peace)
  {
    return peace.Terms switch
    {
      PeaceTerms.White => true,
      PeaceTerms.Cession => peace.Ceded.Count <= ProvincesWithinReach,
      _ => Math.Abs(Momentum) >= ToAnnex
    };
  }

  public void ConcludeWith(Peace peace)
  {
    if (IsOver)
    {
      throw new DomainException("This war has already been brought to an end.");
    }

    if (!Permits(peace))
    {
      throw new DomainException($"The fighting has not earned {peace}.");
    }

    Peace = peace;
  }

  public override string ToString()
  {
    return IsOver
      ? $"war concluded on {Peace}"
      : $"war in progress (momentum {Momentum:+#;-#;0})";
  }

  #endregion
}
