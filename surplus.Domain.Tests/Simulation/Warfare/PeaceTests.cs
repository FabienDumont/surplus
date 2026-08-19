using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Warfare;

namespace Surplus.Domain.Tests.Simulation.Warfare;

public class PeaceTests
{
  #region Methods

  [Fact]
  public void A_white_peace_stops_the_guns_and_moves_nothing()
  {
    var peace = Peace.White();

    Assert.Equal(PeaceTerms.White, peace.Terms);
    Assert.Empty(peace.Ceded);
  }

  [Fact]
  public void A_cession_names_the_provinces_changing_hands()
  {
    var alsace = ProvinceId.New();
    var lorraine = ProvinceId.New();

    var peace = Peace.Ceding(alsace, lorraine);

    Assert.Equal(PeaceTerms.Cession, peace.Terms);
    Assert.Equal([alsace, lorraine], peace.Ceded);
  }

  [Fact]
  public void A_peace_ceding_nothing_is_a_white_peace_and_not_a_cession()
  {
    Assert.Throws<DomainException>(() => Peace.Ceding());
  }

  [Fact]
  public void The_same_province_cannot_be_ceded_twice()
  {
    var alsace = ProvinceId.New();

    Assert.Throws<DomainException>(() => Peace.Ceding(alsace, alsace));
  }

  [Fact]
  public void An_annexation_names_no_provinces_because_it_takes_them_all()
  {
    var peace = Peace.Annexation();

    Assert.Equal(PeaceTerms.Annexation, peace.Terms);
    Assert.Empty(peace.Ceded);
  }

  [Fact]
  public void Peaces_on_the_same_terms_over_the_same_provinces_are_equal()
  {
    var alsace = ProvinceId.New();
    var lorraine = ProvinceId.New();

    var oneOrder = Peace.Ceding(alsace, lorraine);
    var theOther = Peace.Ceding(lorraine, alsace);

    Assert.Equal(oneOrder, theOther);
    Assert.Equal(oneOrder.GetHashCode(), theOther.GetHashCode());
    Assert.Equal(Peace.White(), Peace.White());
    Assert.NotEqual(oneOrder, Peace.Ceding(alsace));
    Assert.NotEqual(Peace.White(), Peace.Annexation());
    Assert.False(oneOrder.Equals(null));
  }

  [Theory]
  [InlineData(0, "white peace")]
  [InlineData(1, "cession of 1 province")]
  [InlineData(3, "cession of 3 provinces")]
  public void A_peace_reads_as_what_it_costs(int provinces, string expected)
  {
    var peace = provinces == 0
      ? Peace.White()
      : Peace.Ceding(Enumerable.Range(0, provinces).Select(_ => ProvinceId.New()).ToArray());

    Assert.Equal(expected, peace.ToString());
  }

  [Fact]
  public void An_annexation_reads_as_what_it_is()
  {
    Assert.Equal("annexation", Peace.Annexation().ToString());
  }

  #endregion
}
