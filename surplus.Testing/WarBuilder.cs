using Surplus.Domain.Simulation.Warfare;

namespace Surplus.Testing;

/// <summary>
/// Builds a <see cref="War" /> at whatever point in its course a test needs,
/// rather than fighting it turn by turn to get there.
/// </summary>
public sealed class WarBuilder
{
  #region Fields

  private CountryId _aggressor = CountryId.New();
  private CountryId _defender = CountryId.New();
  private WarId _id = WarId.New();
  private int _momentum;
  private Peace? _peace;

  #endregion

  #region Methods

  public WarBuilder WithId(WarId id)
  {
    _id = id;

    return this;
  }

  public WarBuilder WithAggressor(CountryId aggressor)
  {
    _aggressor = aggressor;

    return this;
  }

  public WarBuilder WithDefender(CountryId defender)
  {
    _defender = defender;

    return this;
  }

  public WarBuilder WithMomentum(int momentum)
  {
    _momentum = momentum;

    return this;
  }

  public WarBuilder WithPeace(Peace peace)
  {
    _peace = peace;

    return this;
  }

  /// <summary>Shorthand for a war the aggressor has broken the defender in.</summary>
  public WarBuilder Overwhelming() => WithMomentum(100);

  public War Build() => War.Load(_id, _aggressor, _defender, _momentum, _peace);

  #endregion
}
