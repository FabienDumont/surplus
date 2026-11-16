using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Circulation;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Testing;

namespace Surplus.Domain.Tests.Simulation.Circulation;

public class TrafficTests
{
  #region Fields

  private static readonly UnitOfMeasure Load = UnitOfMeasure.Of("load");
  private static readonly UnitOfMeasure Tonne = UnitOfMeasure.Of("t");

  private readonly Commodity _carriage = new CommodityBuilder().WithName("Carriage").WithDepartment(Department.MeansOfProduction)
    .WithUseValue(new UseValueBuilder().WithSatisfiedWant("carrying goods").WithUnit(Load).Build()).Build();

  private readonly Commodity _corn = new CommodityBuilder().WithName("Corn")
    .WithUseValue(new UseValueBuilder().WithSatisfiedWant("nourishment").WithUnit(Tonne).Build()).Build();

  #endregion

  #region Tests

  [Fact]
  public void Goods_cross_a_border_toward_where_they_are_wanted()
  {
    var kent = Farms(1_010m);
    var lancashire = Mills(0m);
    var world = Kingdom(Haulage.None, kent, lancashire);

    Traffic.Circulate(world);

    // Lancashire's ten people keep ten days of a tonne each; Kent could spare five hundred.
    Assert.Equal(100m, Corn(lancashire));
    Assert.Equal(910m, Corn(kent));
  }

  [Fact]
  public void A_province_keeps_ten_days_of_what_it_needs_before_sending_any()
  {
    var kent = Farms(10m);
    var lancashire = Mills(0m);

    Traffic.Circulate(Kingdom(Haulage.None, kent, lancashire));

    Assert.Equal(10m, Corn(kent));
    Assert.Equal(0m, Corn(lancashire));
  }

  [Fact]
  public void A_province_sends_only_what_its_mode_lets_go_of_in_a_day()
  {
    var capitalist = Farms(110m);
    var feudal = Farms(110m, SocialClass.Serfs);
    var britain = Mills(0m);
    var russia = Mills(0m);
    var world = new WorldBuilder().WithRegister(CommodityRegister.Of(_corn, _carriage)).WithSubsistence(Basket())
      .WithCountries(Country("United Kingdom", capitalist, britain), Country("Russia", feudal, russia))
      .WithBorders(new Border(capitalist.Id, britain.Id), new Border(feudal.Id, russia.Id)).Build();

    Traffic.Circulate(world);

    // A hundred tonnes to spare: capital sends half of it, the manor a twentieth.
    Assert.Equal(50m, Corn(britain));
    Assert.Equal(5m, Corn(russia));
  }

  [Fact]
  public void Carriage_limits_what_can_be_sent()
  {
    var kent = Farms(1_010m, carts: 30m);
    var lancashire = Mills(0m);

    Traffic.Circulate(Kingdom(Carts(), kent, lancashire));

    Assert.Equal(30m, Corn(lancashire));
    Assert.Equal(0m, kent.Store.AmountOf(_carriage.Id));
  }

  [Fact]
  public void Carriage_falling_short_is_reported_once_a_day()
  {
    var kent = Farms(1_010m, carts: 30m);

    var occurrences = Traffic.Circulate(Kingdom(Carts(), kent, Mills(0m)));

    Assert.Equal([new CarriageFellShort("United Kingdom", "Kent", "Corn")], occurrences);
  }

  [Fact]
  public void Enough_carriage_reports_nothing()
  {
    var kent = Farms(1_010m, carts: 500m);
    var lancashire = Mills(0m);

    var occurrences = Traffic.Circulate(Kingdom(Carts(), kent, lancashire));

    Assert.Empty(occurrences);
    Assert.Equal(100m, Corn(lancashire));
  }

  [Fact]
  public void No_carriage_at_all_sends_nothing_that_takes_up_room()
  {
    var kent = Farms(1_010m);
    var lancashire = Mills(0m);

    var occurrences = Traffic.Circulate(Kingdom(Carts(), kent, lancashire));

    Assert.Equal(0m, Corn(lancashire));
    Assert.Single(occurrences);
  }

  [Fact]
  public void Carriage_not_used_today_is_gone_tomorrow()
  {
    var kent = Farms(10m, carts: 50m);

    Traffic.Circulate(Kingdom(Carts(), kent, Mills(0m)));

    Assert.Equal(0m, kent.Store.AmountOf(_carriage.Id));
  }

  [Fact]
  public void Nothing_crosses_a_border_between_two_countries()
  {
    var kent = Farms(1_010m);
    var flanders = Mills(0m);
    var world = new WorldBuilder().WithRegister(CommodityRegister.Of(_corn, _carriage)).WithSubsistence(Basket())
      .WithCountries(Country("United Kingdom", kent), Country("Belgium", flanders))
      .WithBorders(new Border(kent.Id, flanders.Id)).Build();

    Traffic.Circulate(world);

    Assert.Equal(0m, Corn(flanders));
  }

  [Fact]
  public void Goods_travel_one_border_a_day_and_go_no_further()
  {
    var kent = Farms(1_010m);
    var middlesex = Mills(0m);
    var lancashire = Mills(0m);
    var world = new WorldBuilder().WithRegister(CommodityRegister.Of(_corn, _carriage)).WithSubsistence(Basket())
      .WithCountries(Country("United Kingdom", kent, middlesex, lancashire))
      .WithBorders(new Border(kent.Id, middlesex.Id), new Border(middlesex.Id, lancashire.Id)).Build();

    Traffic.Circulate(world);

    Assert.Equal(100m, Corn(middlesex));
    Assert.Equal(0m, Corn(lancashire));
  }

  [Fact]
  public void What_a_provinces_branches_use_up_is_kept_back_too()
  {
    var mill = new BranchBuilder().Producing(CommodityId.New()).WithMeans(MeansOfProduction.SetInMotion(
      ProductiveForm.Cooperation, 0m, [], [Stock.Of(_corn.Id, Quantity.Of(5m, Tonne))])).Build();
    var kent = new ProvinceBuilder().WithName("Kent").WithClasses(ClassPresence.Of(SocialClass.Bourgeoisie, 1))
      .Working(mill).Holding(Tonnes(60m)).Build();
    var lancashire = Mills(0m);

    Traffic.Circulate(Kingdom(Haulage.None, kent, lancashire));

    // Ten days of one head and of five tonnes worked up a day: sixty, and nothing to spare.
    Assert.Equal(0m, Corn(lancashire));
  }

  #endregion

  #region Methods

  /// <summary>A capitalist Kent of one bourgeois, so it keeps ten tonnes before it sends any.</summary>
  private Province Farms(decimal corn, SocialClass socialClass = SocialClass.Bourgeoisie, decimal carts = 0m)
  {
    List<Stock> held = [Tonnes(corn)];

    if (carts > 0m)
    {
      held.Add(Stock.Of(_carriage.Id, Quantity.Of(carts, Load)));
    }

    return new ProvinceBuilder().WithName("Kent").WithClasses(ClassPresence.Of(socialClass, 1)).Holding([.. held]).Build();
  }

  /// <summary>A Lancashire of ten proletarians, who keep a hundred tonnes before they send any.</summary>
  private Province Mills(decimal corn)
  {
    return new ProvinceBuilder().WithName("Lancashire").WithClasses(ClassPresence.Of(SocialClass.Proletariat, 10))
      .Holding(Tonnes(corn)).Build();
  }

  private World Kingdom(Haulage haulage, Province one, Province other)
  {
    return new WorldBuilder().WithRegister(CommodityRegister.Of(_corn, _carriage)).WithSubsistence(Basket())
      .WithHaulage(haulage).WithCountries(Country("United Kingdom", one, other))
      .WithBorders(new Border(one.Id, other.Id)).Build();
  }

  private static Country Country(string name, params Province[] provinces)
  {
    return new CountryBuilder().WithName(name).WithProvinces(provinces).Build();
  }

  /// <summary>A tonne of corn a head a day.</summary>
  private Subsistence Basket()
  {
    return Subsistence.Of(Tonnes(1m));
  }

  /// <summary>A load of carriage a tonne of corn.</summary>
  private Haulage Carts()
  {
    return Haulage.Of(_carriage.Id, [new Bulk(_corn.Id, 1m)]);
  }

  private Stock Tonnes(decimal amount)
  {
    return Stock.Of(_corn.Id, Quantity.Of(amount, Tonne));
  }

  private decimal Corn(Province province)
  {
    return province.Store.AmountOf(_corn.Id);
  }

  #endregion
}
