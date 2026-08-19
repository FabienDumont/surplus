using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Countries;
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

    var province = new ProvinceBuilder().WithId(id).WithName("Bretagne").WithComposition(composition).Build();

    Assert.Equal(id, province.Id);
    Assert.Equal("Bretagne", province.Name);
    Assert.Equal(composition, province.Composition);
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

  #endregion
}
