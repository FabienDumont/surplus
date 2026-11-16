using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Domain.Simulation.Circulation;

/// <summary>
/// A day of goods crossing borders inside each country. Moving a product is a
/// labour process that continues production inside circulation (Capital II,
/// ch. 6 §3), so it is carried by carts that carters made, and goes no faster
/// than the province's mode of production lets go of its surplus.
/// Goods move one border a day, toward where they are lacking, and only once a
/// province has kept enough for itself. Nothing crosses a border between two
/// countries: trade between states is not modelled.
/// </summary>
public static class Traffic
{
  #region Fields

  /// <summary>A province keeps ten days of what it needs before it sends anything away.</summary>
  private static readonly decimal DaysKept = 10m;

  #endregion

  #region Methods

  public static IReadOnlyList<Occurrence> Circulate(World world)
  {
    List<CarriageFellShort> shortfalls = [];
    List<Commodity> commodities = [.. world.Register.Commodities.Where(commodity => commodity.Id != world.Haulage.Carriage)];

    foreach (var country in world.Countries)
    {
      foreach (var border in world.Borders.Where(border => country.Governs(border.One) && country.Governs(border.Other)))
      {
        var one = country.Provinces.Single(province => province.Id == border.One);
        var other = country.Provinces.Single(province => province.Id == border.Other);

        foreach (var commodity in commodities)
        {
          Send(world, country, one, other, commodity, shortfalls);
          Send(world, country, other, one, commodity, shortfalls);
        }
      }
    }

    Perish(world);

    return [.. shortfalls.Distinct()];
  }

  /// <summary>
  /// Sends what one province holds above its own needs toward what the other
  /// lacks of its own, so far as the shipping province's mode and its carts
  /// allow.
  /// </summary>
  private static void Send(
    World world, Country country, Province from, Province to, Commodity commodity, List<CarriageFellShort> shortfalls)
  {
    var surplus = from.Store.AmountOf(commodity.Id) - Target(world, from, commodity.Id);
    var want = Target(world, to, commodity.Id) - to.Store.AmountOf(commodity.Id);

    if (surplus <= 0m || want <= 0m)
    {
      return;
    }

    var amount = Math.Min(surplus * from.Mode.Integration(), want);
    var loads = world.Haulage.LoadsPer(commodity.Id);

    if (loads > 0m && world.Haulage.Carriage is { } carriage)
    {
      var carts = from.Store.AmountOf(carriage);

      if (carts / loads < amount)
      {
        amount = carts / loads;
        shortfalls.Add(new CarriageFellShort(country.Name, from.Name, commodity.Name));
      }

      if (amount <= 0m)
      {
        return;
      }

      from.Draw([Stock.Of(carriage, Quantity.Of(Math.Min(amount * loads, carts), CarriageUnit(world, carriage)))]);
    }

    var goods = Stock.Of(commodity.Id, Quantity.Of(amount, commodity.UseValue.Unit));

    from.Draw([goods]);
    to.LayUp([goods]);
  }

  /// <summary>
  /// What a province keeps before it sends anything away: ten days of its
  /// people's basket and of what its branches use up.
  /// </summary>
  private static decimal Target(World world, Province province, CommodityId commodity)
  {
    var eaten = world.Subsistence.PerHead
      .Where(stock => stock.Commodity == commodity)
      .Sum(stock => stock.Quantity.Amount * province.Population);
    var used = province.Branches
      .SelectMany(branch => branch.Means.ConsumedInOnePeriod())
      .Where(stock => stock.Commodity == commodity)
      .Sum(stock => stock.Quantity.Amount);

    return DaysKept * (eaten + used);
  }

  /// <summary>Carriage cannot be stored: a cart-day not used today is gone.</summary>
  private static void Perish(World world)
  {
    if (world.Haulage.Carriage is not { } carriage)
    {
      return;
    }

    foreach (var province in world.Countries.SelectMany(country => country.Provinces))
    {
      var left = province.Store.AmountOf(carriage);

      if (left > 0m)
      {
        province.Draw([Stock.Of(carriage, Quantity.Of(left, CarriageUnit(world, carriage)))]);
      }
    }
  }

  private static UnitOfMeasure CarriageUnit(World world, CommodityId carriage)
  {
    return world.Register.Get(carriage).UseValue.Unit;
  }

  #endregion
}
