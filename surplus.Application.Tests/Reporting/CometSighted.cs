using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Application.Tests.Reporting;

/// <summary>An occurrence of a kind no plan has classified yet.</summary>
internal sealed record CometSighted : Occurrence
{
  #region Methods

  public override string ToString()
  {
    return "A comet was seen over the Channel.";
  }

  #endregion
}
