using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Warfare;

namespace Surplus.Application.Ai;

/// <summary>
/// What a country does when no one is playing it. It is deliberately small:
/// it keeps its own reproduction going and ends the wars it is winning, using
/// only orders a player also has. It does not plan, declare war, legislate or
/// re-equip, and it never chooses at random: the same world always gets the
/// same answer.
/// It never fails for want of something to do. Where it cannot do what a rule
/// asks, it does as much as it can and moves on.
/// </summary>
public static class CountryAi
{
  #region Fields

  /// <summary>
  /// The room a province keeps in its carts beyond what its neighbours want
  /// carried today: a month passes before the AI looks again, and a people
  /// grows in the meantime.
  /// </summary>
  private static readonly decimal CartsToSpare = 1.2m;

  #endregion

  #region Methods

  /// <summary>Lets one AI country do what it does at the turn of a month.</summary>
  public static void Act(World world, Country country)
  {
    KeepFlowsInBalance(world, country);
    KeepCartsEnough(world, country);
    EndWarsItIsWinning(world, country);
  }

  /// <summary>
  /// Rule 1. For every commodity it falls short of in a day, against what its
  /// people have a title to and its branches use up, it sets idle hands to
  /// work in the branches that make it, province by province.
  /// </summary>
  private static void KeepFlowsInBalance(World world, Country country)
  {
    foreach (var commodity in world.Register.Commodities.Where(commodity => commodity.Id != world.Haulage.Carriage))
    {
      var shortfall = country.Provinces.Sum(province => Use(world, country, province, commodity.Id))
                      - country.Provinces.Sum(province => Output(province, commodity.Id));

      Hire(country, country.Provinces, commodity.Id, shortfall);
    }
  }

  /// <summary>
  /// Rule 1 for carriage. Carriage is used up by nothing a branch or a basket
  /// names, only by goods crossing a border, so its use is read off the
  /// neighbours: every load a province's neighbours lack in a day of what it
  /// has to spare is a load its carters must carry. The AI keeps a fifth more
  /// than that, since it looks again only next month.
  /// </summary>
  private static void KeepCartsEnough(World world, Country country)
  {
    if (world.Haulage.Carriage is not { } carriage)
    {
      return;
    }

    foreach (var province in country.Provinces)
    {
      var loads = world.Borders.Where(border => border.Joins(province.Id))
        .Select(border => border.One == province.Id ? border.Other : border.One)
        .Where(country.Governs)
        .Select(neighbour => country.Provinces.Single(candidate => candidate.Id == neighbour))
        .Sum(neighbour => world.Register.Commodities
          .Where(commodity => commodity.Id != carriage)
          .Where(commodity => Output(province, commodity.Id) > Use(world, country, province, commodity.Id))
          .Sum(commodity => Math.Max(0m, Use(world, country, neighbour, commodity.Id) - Output(neighbour, commodity.Id))
                            * world.Haulage.LoadsPer(commodity.Id)));

      Hire(country, [province], carriage, loads * CartsToSpare - Output(province, carriage));
    }
  }

  /// <summary>What a province must find of a commodity in a day: its people's titles and its branches' wants.</summary>
  private static decimal Use(World world, Country country, Province province, CommodityId commodity)
  {
    var baskets = country.BasketsClaimed(province.Id);

    return world.Subsistence.PerHead.Where(stock => stock.Commodity == commodity).Sum(stock => stock.Quantity.Amount * baskets)
           + province.Branches.SelectMany(branch => branch.Means.ConsumedInOnePeriod())
             .Where(stock => stock.Commodity == commodity)
             .Sum(stock => stock.Quantity.Amount);
  }

  private static decimal Output(Province province, CommodityId commodity)
  {
    return province.Branches.Where(branch => branch.Produces == commodity).Sum(branch => PerHand(branch) * branch.Hands);
  }

  /// <summary>
  /// Takes on as many idle hands as a shortfall calls for in the branches that
  /// make a commodity, in the order given, no more than stand idle and no more
  /// than the province's store can equip.
  /// </summary>
  private static void Hire(Country country, IEnumerable<Province> provinces, CommodityId commodity, decimal shortfall)
  {
    foreach (var province in provinces)
    {
      if (shortfall <= 0m)
      {
        return;
      }

      if (province.Branches.FirstOrDefault(branch => branch.Produces == commodity) is not { } branch)
      {
        continue;
      }

      var wanted = (int)Math.Ceiling(shortfall / PerHand(branch));
      var hire = Math.Min(wanted, Math.Min(province.Idle(branch.Workforce), Equippable(province, branch)));

      if (hire > 0)
      {
        country.Employ(province.Id, commodity, hire);
        shortfall -= hire * PerHand(branch);
      }
    }
  }

  /// <summary>What one hand of a branch turns out in a working day.</summary>
  private static decimal PerHand(Branch branch)
  {
    return branch.OutputPerHour.Amount * branch.WorkingDay.Hours;
  }

  /// <summary>
  /// The most hands the province's store holds instruments for, at the
  /// branch's present means per hand. Unlimited when the branch uses none.
  /// </summary>
  private static int Equippable(Province province, Branch branch)
  {
    if (branch.Means.Instruments.Count == 0)
    {
      return int.MaxValue;
    }

    return branch.Means.Instruments
      .Select(instrument => (int)Math.Floor(
        province.Store.AmountOf(instrument.Commodity) / (instrument.Quantity.Amount / branch.Hands)))
      .Min();
  }

  /// <summary>
  /// Rule 2. A war it has won outright ends in annexation; one well enough in
  /// hand ends in the provinces within reach, never the enemy's last. It never
  /// proposes a white peace and never ends a war it is losing.
  /// </summary>
  private static void EndWarsItIsWinning(World world, Country country)
  {
    foreach (var war in world.WarsOf(country.Id).ToList())
    {
      if (war.IsOver || war.Prevailing != country.Id)
      {
        continue;
      }

      var loser = world.Country(war.Aggressor == country.Id ? war.Defender : war.Aggressor);

      if (Math.Abs(war.Momentum) >= 100)
      {
        world.MakePeace(war.Id, country.Id, Peace.Annexation());
      }
      else if (war.ProvincesWithinReach >= 2 && loser.Provinces.Count > 1)
      {
        var taken = Math.Min(war.ProvincesWithinReach, loser.Provinces.Count - 1);

        world.MakePeace(war.Id, country.Id, Peace.Ceding([.. loser.Provinces.Take(taken).Select(province => province.Id)]));
      }
    }
  }

  #endregion
}
