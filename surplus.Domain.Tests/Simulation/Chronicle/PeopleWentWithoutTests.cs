using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Domain.Tests.Simulation.Chronicle;

public class PeopleWentWithoutTests
{
  #region Tests

  [Fact]
  public void A_famine_reads_as_the_share_provided_and_the_dead()
  {
    Assert.Equal(
      "Russia could provide only 92% of its people's subsistence; 1,440 died.",
      new PeopleWentWithout("Russia", 0.92m, 1_440).ToString());
  }

  [Fact]
  public void A_people_a_hair_short_is_not_told_it_had_everything()
  {
    Assert.Equal(
      "Russia could provide only 99% of its people's subsistence; no one died of it today.",
      new PeopleWentWithout("Russia", 0.9999m, 0).ToString());
  }

  #endregion
}
