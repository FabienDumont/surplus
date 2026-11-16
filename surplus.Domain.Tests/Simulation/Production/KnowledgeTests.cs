using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Domain.Tests.Simulation.Production;

public class KnowledgeTests
{
  #region Tests

  [Fact]
  public void A_country_that_has_learnt_nothing_knows_nothing()
  {
    Assert.Empty(Knowledge.None.Known);
    Assert.Empty(Knowledge.None.UnderWay);
    Assert.False(Knowledge.None.Knows("Hand loom"));
  }

  [Fact]
  public void What_is_known_cannot_also_be_under_way()
  {
    Assert.Throws<DomainException>(
      () => Knowledge.Of(["Hand loom"], [new Development("Hand loom", Hours(5m))]));
  }

  [Fact]
  public void The_same_technique_cannot_be_known_twice_or_be_under_way_twice()
  {
    Assert.Throws<DomainException>(() => Knowledge.Of(["Hand loom", "Hand loom"], []));
    Assert.Throws<DomainException>(
      () => Knowledge.Of([], [new Development("Power loom", Hours(1m)), new Development("Power loom", Hours(2m))]));
  }

  [Fact]
  public void Labour_spent_toward_a_technique_adds_up()
  {
    var knowledge = Knowledge.None.Advanced("Power loom", Hours(5m)).Advanced("Machine shop", Hours(1m))
      .Advanced("Power loom", Hours(7m));

    Assert.Equal(Hours(12m), knowledge.Spent("Power loom"));
    Assert.Equal(["Power loom", "Machine shop"], knowledge.UnderWay.Select(development => development.Technique));
    Assert.Equal(LaborTime.None, knowledge.Spent("Railway"));
  }

  [Fact]
  public void A_technique_learnt_is_no_longer_under_way()
  {
    var knowledge = Knowledge.Of(["Hand loom"], [new Development("Power loom", Hours(5m))]).Learnt("Power loom");

    Assert.Equal(["Hand loom", "Power loom"], knowledge.Known);
    Assert.Empty(knowledge.UnderWay);
  }

  [Fact]
  public void What_is_already_known_cannot_be_advanced()
  {
    Assert.Throws<DomainException>(() => Knowledge.Of(["Hand loom"], []).Advanced("Hand loom", Hours(1m)));
  }

  [Fact]
  public void Learning_what_is_known_changes_nothing()
  {
    var knowledge = Knowledge.Of(["Hand loom"], []);

    Assert.Same(knowledge, knowledge.Learnt("Hand loom"));
  }

  [Fact]
  public void Knowledge_of_the_same_techniques_in_the_same_order_is_the_same_knowledge()
  {
    var knowledge = Knowledge.Of(["Hand loom"], [new Development("Power loom", Hours(5m))]);
    var same = Knowledge.Of(["Hand loom"], [new Development("Power loom", Hours(5m))]);

    Assert.Equal(knowledge, same);
    Assert.Equal(knowledge.GetHashCode(), same.GetHashCode());
    Assert.NotEqual(knowledge, Knowledge.Of(["Hand loom"], []));
    Assert.False(knowledge.Equals(null));
  }

  #endregion

  #region Methods

  private static LaborTime Hours(decimal hours)
  {
    return LaborTime.FromHours(hours);
  }

  #endregion
}
