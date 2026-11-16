using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Tests.Simulation.Production;

public class TechnicalProgressTests
{
  #region Tests

  [Fact]
  public void Capital_drives_development_ten_times_as_hard_as_the_lord()
  {
    Assert.Equal(10m, ModeOfProduction.Capitalist.Drive() / ModeOfProduction.Feudal.Drive());
  }

  [Fact]
  public void Every_mode_of_production_drives_development_somewhat()
  {
    Assert.All(Enum.GetValues<ModeOfProduction>(), mode => Assert.True(mode.Drive() > 0m));
    Assert.Throws<DomainException>(() => ((ModeOfProduction)999).Drive());
  }

  [Fact]
  public void The_slave_drives_less_than_the_serf_and_the_commune_least_of_all()
  {
    Assert.True(ModeOfProduction.Slave.Drive() < ModeOfProduction.Feudal.Drive());
    Assert.True(ModeOfProduction.PrimitiveCommunal.Drive() < ModeOfProduction.Slave.Drive());
  }

  #endregion
}
