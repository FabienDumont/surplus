using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Circulation;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Tests.Simulation.Circulation;

public class MarketIntegrationTests
{
  #region Tests

  [Fact]
  public void Capital_sends_ten_times_what_the_manor_does()
  {
    Assert.Equal(10m, ModeOfProduction.Capitalist.Integration() / ModeOfProduction.Feudal.Integration());
  }

  [Fact]
  public void The_slave_south_trades_more_than_the_manor_and_less_than_capital()
  {
    var slave = ModeOfProduction.Slave.Integration();

    Assert.True(slave > ModeOfProduction.Feudal.Integration());
    Assert.True(slave < ModeOfProduction.Capitalist.Integration());
  }

  [Fact]
  public void Every_mode_lets_go_of_something_and_never_more_than_all()
  {
    Assert.All(Enum.GetValues<ModeOfProduction>(), mode => Assert.InRange(mode.Integration(), 0.0001m, 1m));
    Assert.Throws<DomainException>(() => ((ModeOfProduction)999).Integration());
  }

  #endregion
}
