using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Domain.Tests.Simulation.Chronicle;

public class CarriageFellShortTests
{
  #region Tests

  [Fact]
  public void A_want_of_carts_reads_as_what_could_not_be_sent_from_where()
  {
    Assert.Equal(
      "Kent had not the carts to send Corn where it was wanted (United Kingdom).",
      new CarriageFellShort("United Kingdom", "Kent", "Corn").ToString());
  }

  #endregion
}
