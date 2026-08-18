using MarxAttack.Domain.SharedKernel;
using MarxAttack.Domain.SharedKernel.StrongIds;

namespace MarxAttack.Domain.Tests.SharedKernel;

public class StrongIdTests
{
    private abstract class TestTag : IStrongIdTag;

    [Fact]
    public void New_generates_a_non_empty_unique_id()
    {
        var first = Id<TestTag>.New();
        var second = Id<TestTag>.New();

        Assert.NotEqual(Guid.Empty, first.Value);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void From_keeps_the_given_value()
    {
        var value = Guid.CreateVersion7();

        Assert.Equal(value, Id<TestTag>.From(value).Value);
    }

    [Fact]
    public void From_rejects_an_empty_guid()
    {
        Assert.Throws<DomainException>(() => Id<TestTag>.From(Guid.Empty));
    }

    [Fact]
    public void Ids_with_the_same_value_and_tag_are_equal()
    {
        var value = Guid.CreateVersion7();

        Assert.Equal(Id<TestTag>.From(value), Id<TestTag>.From(value));
    }

    [Fact]
    public void Ids_of_different_tags_are_different_types()
    {
        // The whole point of the tag: GameId and CommodityId cannot be mixed up.
        Assert.NotEqual(typeof(GameId), typeof(CommodityId));
    }
}
