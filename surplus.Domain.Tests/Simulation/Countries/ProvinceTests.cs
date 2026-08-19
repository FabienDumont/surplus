using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Testing;

namespace Surplus.Domain.Tests.Simulation.Countries;

public class ProvinceTests
{
  #region Methods

  [Fact]
  public void Establishing_a_province_keeps_its_trimmed_name_and_its_classes()
  {
    var composition = new ClassCompositionBuilder().Build();

    var province = Province.Establish("  Île-de-France  ", composition);

    Assert.Equal("Île-de-France", province.Name);
    Assert.Equal(composition, province.Composition);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void A_province_must_have_a_name(string blank)
  {
    Assert.Throws<DomainException>(() => Province.Establish(blank, ClassComposition.Empty));
  }

  [Fact]
  public void Each_province_has_its_own_identity()
  {
    var first = Province.Establish("Normandie", ClassComposition.Empty);
    var second = Province.Establish("Normandie", ClassComposition.Empty);

    Assert.NotEqual(first.Id, second.Id);
  }

  [Fact]
  public void A_saved_province_is_reloaded_exactly_as_it_was_left()
  {
    var id = ProvinceId.New();
    var composition = new ClassCompositionBuilder().Of(ClassPresence.Of(SocialClass.Proletariat, 700)).Build();
    var branch = new BranchBuilder().WorkedBy(SocialClass.Proletariat, 700).Build();

    var province = new ProvinceBuilder().WithId(id).WithName("Bretagne").WithComposition(composition)
      .Working(branch).Build();

    Assert.Equal(id, province.Id);
    Assert.Equal("Bretagne", province.Name);
    Assert.Equal(composition, province.Composition);
    Assert.Equal(branch, Assert.Single(province.Branches));
  }

  [Fact]
  public void A_province_counts_its_souls_by_class()
  {
    var province = new ProvinceBuilder().Build();

    Assert.Equal(915, province.Population);
    Assert.Equal(900, province.HeadsOf(SocialClass.Serfs));
    Assert.Equal(0, province.HeadsOf(SocialClass.Proletariat));
  }

  [Fact]
  public void A_provinces_mode_is_read_off_its_class_structure()
  {
    // Nine hundred serfs under a lord and a priest are a feudal province,
    // whatever anyone would prefer to call it.
    Assert.Equal(ModeOfProduction.Feudal, new ProvinceBuilder().Build().Mode);
  }

  [Fact]
  public void A_province_where_no_one_lives_owns_nothing_in_common_or_otherwise()
  {
    var province = new ProvinceBuilder().Unpeopled().Build();

    Assert.Equal(0, province.Population);
    Assert.Equal(ModeOfProduction.PrimitiveCommunal, province.Mode);
    Assert.True(province.IsClassless);
  }

  [Fact]
  public void A_province_under_lords_is_not_classless()
  {
    Assert.False(new ProvinceBuilder().Build().IsClassless);
  }

  [Fact]
  public void A_province_growing_and_declining_moves_its_numbers()
  {
    var province = new ProvinceBuilder().Build();

    province.Grow(SocialClass.Merchants, 40);
    province.Decline(SocialClass.Serfs, 100);

    Assert.Equal(40, province.HeadsOf(SocialClass.Merchants));
    Assert.Equal(800, province.HeadsOf(SocialClass.Serfs));
  }

  [Fact]
  public void Enclosure_turns_peasants_into_proletarians_without_changing_the_head_count()
  {
    var province = new ProvinceBuilder()
      .WithClasses(ClassPresence.Of(SocialClass.FreePeasants, 1_000))
      .Build();

    province.Transform(SocialClass.FreePeasants, SocialClass.Proletariat, 600);

    Assert.Equal(400, province.HeadsOf(SocialClass.FreePeasants));
    Assert.Equal(600, province.HeadsOf(SocialClass.Proletariat));
    Assert.Equal(1_000, province.Population);
  }

  [Fact]
  public void A_province_cannot_lose_more_of_a_class_than_stands_in_it()
  {
    var province = new ProvinceBuilder().Build();

    Assert.Throws<DomainException>(() => province.Decline(SocialClass.Serfs, 901));
  }

  [Fact]
  public void A_revolution_in_the_base_is_a_balance_tipping_and_not_a_decree()
  {
    // No one proclaims the province capitalist. It becomes capitalist when
    // enough serfs have stopped being serfs — quantity turning into quality.
    var province = new ProvinceBuilder().Build();

    province.Transform(SocialClass.Serfs, SocialClass.Proletariat, 500);
    Assert.Equal(ModeOfProduction.Feudal, province.Mode);

    province.Transform(SocialClass.Serfs, SocialClass.Proletariat, 300);
    Assert.Equal(ModeOfProduction.Capitalist, province.Mode);
  }

  [Fact]
  public void Classes_outliving_their_mode_show_up_as_survivals()
  {
    var province = new ProvinceBuilder().Build();

    Assert.Empty(province.Survivals);

    province.Transform(SocialClass.Serfs, SocialClass.Proletariat, 800);

    // Lords, serfs and a tithe-drawing Church left standing after the base has
    // moved out from under them: Prussia after 1807, on paper.
    Assert.Contains(SocialClass.Serfs, province.Survivals);
    Assert.Contains(SocialClass.FeudalLords, province.Survivals);
    Assert.Contains(SocialClass.Clergy, province.Survivals);
    Assert.DoesNotContain(SocialClass.Proletariat, province.Survivals);
  }

  [Fact]
  public void A_province_reads_as_its_name_its_mode_and_its_population()
  {
    Assert.Equal("Île-de-France — Feudal (915 souls)", new ProvinceBuilder().Build().ToString());
  }

  [Fact]
  public void A_province_works_all_of_one_kind_in_a_single_branch()
  {
    // Not because two mills cannot stand in one province, but because a branch
    // is already all the mills of its kind there.
    var province = new ProvinceBuilder().Build();
    var wheat = CommodityId.New();

    province.Open(new BranchBuilder().Producing(wheat).WorkedBy(SocialClass.Serfs, 400).Build());

    Assert.Throws<DomainException>(
      () => province.Open(new BranchBuilder().Producing(wheat).WorkedBy(SocialClass.Serfs, 100).Build())
    );
  }

  [Fact]
  public void Nobody_can_be_set_to_work_who_is_not_standing_in_the_province()
  {
    var province = new ProvinceBuilder().Build();

    Assert.Throws<DomainException>(
      () => province.Open(new BranchBuilder().WorkedBy(SocialClass.Serfs, 901).Build())
    );
    Assert.Throws<DomainException>(
      () => province.Open(new BranchBuilder().WorkedBy(SocialClass.Proletariat, 1).Build())
    );
  }

  [Fact]
  public void Those_the_branches_have_no_work_for_stand_idle()
  {
    var province = new ProvinceBuilder().Build();
    var wheat = CommodityId.New();

    province.Open(new BranchBuilder().Producing(wheat).WorkedBy(SocialClass.Serfs, 600).Build());

    Assert.Equal(600, province.Employed(SocialClass.Serfs));
    Assert.Equal(300, province.Idle(SocialClass.Serfs));

    province.Employ(wheat, 300);
    Assert.Equal(0, province.Idle(SocialClass.Serfs));

    // And there is no one left to take on.
    Assert.Throws<DomainException>(() => province.Employ(wheat, 1));
  }

  [Fact]
  public void Those_who_are_no_longer_here_are_no_longer_at_work()
  {
    var province = new ProvinceBuilder().Build();
    var wheat = CommodityId.New();

    province.Open(new BranchBuilder().Producing(wheat).WorkedBy(SocialClass.Serfs, 900).Build());

    // A famine does not ask the demesne whether it can spare them.
    province.Decline(SocialClass.Serfs, 100);

    Assert.Equal(800, province.Employed(SocialClass.Serfs));
  }

  [Fact]
  public void Enclosure_takes_hands_out_of_the_branch_it_emptied()
  {
    var province = new ProvinceBuilder().Build();
    var wheat = CommodityId.New();

    province.Open(new BranchBuilder().Producing(wheat).WorkedBy(SocialClass.Serfs, 900).Build());

    // Those driven off the land are not driven into the same work under another
    // name: they leave the branch, and stand outside it as free labour.
    province.Transform(SocialClass.Serfs, SocialClass.Proletariat, 400);

    Assert.Equal(500, province.Employed(SocialClass.Serfs));
    Assert.Equal(0, province.Employed(SocialClass.Proletariat));
    Assert.Equal(400, province.Idle(SocialClass.Proletariat));
  }

  [Fact]
  public void A_branch_left_without_hands_is_no_branch_of_production()
  {
    var province = new ProvinceBuilder().Build();
    var wheat = CommodityId.New();

    province.Open(new BranchBuilder().Producing(wheat).WorkedBy(SocialClass.Serfs, 900).Build());

    province.Decline(SocialClass.Serfs, 900);

    Assert.Empty(province.Branches);
    Assert.False(province.Produces(wheat));
  }

  [Fact]
  public void Turning_out_the_last_hand_shuts_the_branch()
  {
    var province = new ProvinceBuilder().Build();
    var wheat = CommodityId.New();

    province.Open(new BranchBuilder().Producing(wheat).WorkedBy(SocialClass.Serfs, 900).Build());

    province.LayOff(wheat, 400);
    Assert.Equal(500, province.Employed(SocialClass.Serfs));

    province.LayOff(wheat, 500);
    Assert.Empty(province.Branches);
    Assert.Equal(900, province.HeadsOf(SocialClass.Serfs));
  }

  [Fact]
  public void Shutting_a_branch_leaves_its_hands_where_they_stood()
  {
    var province = new ProvinceBuilder().Build();
    var wheat = CommodityId.New();

    province.Open(new BranchBuilder().Producing(wheat).WorkedBy(SocialClass.Serfs, 600).Build());

    province.Close(wheat);

    Assert.Empty(province.Branches);
    Assert.False(province.Produces(wheat));
    Assert.Equal(900, province.HeadsOf(SocialClass.Serfs));
    Assert.Equal(900, province.Idle(SocialClass.Serfs));
  }

  [Fact]
  public void A_branch_takes_on_and_turns_off_someone_or_no_one()
  {
    var province = new ProvinceBuilder().Build();
    var wheat = CommodityId.New();

    province.Open(new BranchBuilder().Producing(wheat).WorkedBy(SocialClass.Serfs, 600).Build());

    Assert.Throws<DomainException>(() => province.Employ(wheat, 0));
    Assert.Throws<DomainException>(() => province.LayOff(wheat, 0));
    Assert.Throws<DomainException>(() => province.LayOff(wheat, 601));

    Assert.Equal(600, province.Employed(SocialClass.Serfs));
  }

  [Fact]
  public void Only_as_many_hands_are_taken_out_of_the_branches_as_have_gone()
  {
    var province = new ProvinceBuilder().Build();
    var wheat = CommodityId.New();
    var flax = CommodityId.New();

    province.Open(new BranchBuilder().Producing(wheat).WorkedBy(SocialClass.Serfs, 500).Build());
    province.Open(new BranchBuilder().Producing(flax).WorkedBy(SocialClass.Serfs, 400).Build());

    province.Decline(SocialClass.Serfs, 100);

    // The hundred are taken from the first branch, and the second is left alone:
    // the dead are counted once, not once per mill.
    Assert.Equal(800, province.Employed(SocialClass.Serfs));
    Assert.Equal(400, province.Branches.Single(branch => branch.Produces == flax).Hands);
    Assert.Equal(0, province.Idle(SocialClass.Serfs));
  }

  [Fact]
  public void Nothing_can_be_worked_or_shut_that_is_not_produced_here()
  {
    var province = new ProvinceBuilder().Build();

    Assert.Throws<DomainException>(() => province.Close(CommodityId.New()));
    Assert.Throws<DomainException>(() => province.Employ(CommodityId.New(), 1));
    Assert.Throws<DomainException>(() => province.LayOff(CommodityId.New(), 1));
  }

  [Fact]
  public void A_province_works_every_branch_it_holds_for_the_period()
  {
    var wheat = new CommodityBuilder().WithName("Wheat")
      .WithUseValue(new UseValueBuilder().WithSatisfiedWant("nourishment").WithUnit("quarter").Build())
      .WithSociallyNecessaryLaborTime(LaborTime.FromHours(3m)).Build();

    var province = new ProvinceBuilder().Build();

    province.Open(new BranchBuilder().Producing(wheat.Id).WorkedBy(SocialClass.Serfs, 10)
      .WithOutputPerHour(Quantity.Of(0.5m, UnitOfMeasure.Of("quarter"))).Build());

    var yields = province.Work(CommodityRegister.Of(wheat));

    // Ten serfs, twelve hours each, half a quarter to the hour.
    Assert.Equal(Quantity.Of(60m, UnitOfMeasure.Of("quarter")), Assert.Single(yields).Product.Quantity);
  }

  #endregion
}
