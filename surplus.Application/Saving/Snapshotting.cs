using Surplus.Application.Saving.Snapshots;
using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Circulation;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Domain.Simulation.Warfare;

namespace Surplus.Application.Saving;

/// <summary>
/// Turns a session into plain snapshots and back. Entities are rebuilt through
/// their <c>Load</c> factories, which assert nothing, and value objects
/// through their ordinary factories. Nothing here reflects over the domain or
/// asks it to be serialisable.
/// Order is kept wherever order decides an outcome: countries, provinces,
/// branches, classes, wars, techniques, the register and the means. Only a
/// store's heaps, whose order decides nothing, are written sorted, so that the
/// same state is always written the same way.
/// </summary>
internal static class Snapshotting
{
  #region Methods

  public static SessionSnapshot Taken(Session session, int version)
  {
    var game = session.Game;
    var world = session.World;

    return new SessionSnapshot(
      version,
      new GameSnapshot(game.Id.Value, game.Name, game.CurrentDate.Date.Year, game.CurrentDate.Date.Month,
        game.CurrentDate.Date.Day, game.Status),
      new WorldSnapshot(
        [.. world.Register.Commodities.Select(Taken)],
        [.. world.Techniques.Select(Taken)],
        [.. world.Subsistence.PerHead.Select(Taken)],
        [.. world.Countries.Select(Taken)],
        [.. world.Borders.Select(border => new BorderSnapshot(border.One.Value, border.Other.Value))],
        new HaulageSnapshot(
          world.Haulage.Carriage?.Value,
          [.. world.Haulage.Bulk.Select(bulk => new BulkSnapshot(bulk.Commodity.Value, bulk.LoadsPerUnit))]),
        [.. world.Wars.Select(Taken)],
        [.. world.Pending.Select(Taken)]));
  }

  public static Session Restored(SessionSnapshot snapshot)
  {
    var game = Game.Load(
      GameId.From(snapshot.Game.Id), snapshot.Game.Name,
      GameDate.Of(snapshot.Game.Year, snapshot.Game.Month, snapshot.Game.Day), snapshot.Game.Status);
    var world = snapshot.World;

    return Session.Resume(
      game,
      World.Load(
        CommodityRegister.Of([.. world.Commodities.Select(Restored)]),
        [.. world.Techniques.Select(Restored)],
        Subsistence.Load(world.Subsistence.Select(Restored)),
        [.. world.Countries.Select(Restored)],
        [.. world.Borders.Select(border => new Border(ProvinceId.From(border.One), ProvinceId.From(border.Other)))],
        world.Haulage.Carriage is { } carriage
          ? Haulage.Of(
            CommodityId.From(carriage),
            world.Haulage.Bulk.Select(bulk => new Bulk(CommodityId.From(bulk.Commodity), bulk.LoadsPerUnit)))
          : Haulage.None,
        [.. world.Wars.Select(Restored)],
        [.. world.Pending.Select(Restored)]));
  }

  private static CommoditySnapshot Taken(Commodity commodity)
  {
    return new CommoditySnapshot(
      commodity.Id.Value, commodity.Name, commodity.UseValue.SatisfiedWant, commodity.UseValue.Unit.Name,
      commodity.Department, commodity.Value.Magnitude.Hours);
  }

  private static Commodity Restored(CommoditySnapshot commodity)
  {
    return Commodity.Load(
      CommodityId.From(commodity.Id), commodity.Name, UseValue.Of(commodity.SatisfiedWant, UnitOfMeasure.Of(commodity.Unit)),
      commodity.Department, Value.CrystallisedFrom(LaborTime.FromHours(commodity.Value)));
  }

  private static StockSnapshot Taken(Stock stock)
  {
    return new StockSnapshot(stock.Commodity.Value, stock.Quantity.Amount, stock.Quantity.Unit.Name);
  }

  private static Stock Restored(StockSnapshot stock)
  {
    return Stock.Of(CommodityId.From(stock.Commodity), Quantity.Of(stock.Amount, UnitOfMeasure.Of(stock.Unit)));
  }

  private static StockSnapshot[] Sorted(Store store)
  {
    return [.. store.Stocks.OrderBy(stock => stock.Commodity.Value).Select(Taken)];
  }

  private static TechniqueSnapshot Taken(Technique technique)
  {
    return new TechniqueSnapshot(
      technique.Name, technique.Produces.Value, technique.Form, technique.OutputPerHour.Amount,
      technique.OutputPerHour.Unit.Name, technique.Wear, [.. technique.InstrumentsPerHand.Select(Taken)],
      [.. technique.MaterialPerHand.Select(Taken)], [.. technique.Requires], technique.Development.Hours);
  }

  private static Technique Restored(TechniqueSnapshot technique)
  {
    var elementary = Technique.Of(
      technique.Name, CommodityId.From(technique.Produces), technique.Form,
      Quantity.Of(technique.OutputPerHour, UnitOfMeasure.Of(technique.OutputUnit)), technique.Wear,
      technique.InstrumentsPerHand.Select(Restored), technique.MaterialPerHand.Select(Restored));

    return technique.Requires.Length == 0
      ? elementary
      : elementary.DevelopedFrom(LaborTime.FromHours(technique.Development), technique.Requires);
  }

  private static CountrySnapshot Taken(Country country)
  {
    var government = country.Government;

    return new CountrySnapshot(
      country.Id.Value, country.Name, country.Control,
      new GovernmentSnapshot(government.Form, government.Ideology, [.. government.Laws], government.RelievesThePoor),
      [.. country.Provinces.Select(Taken)],
      [.. country.Knowledge.Known],
      [.. country.Knowledge.UnderWay.Select(development => new DevelopmentSnapshot(development.Technique, development.Spent.Hours))]);
  }

  private static Country Restored(CountrySnapshot country)
  {
    var government = Government.Of(country.Government.Form, country.Government.Ideology, country.Government.Laws);

    return Country.Load(
      CountryId.From(country.Id), country.Name, country.Control,
      country.Government.RelievesThePoor ? government.Relieving() : government,
      country.Provinces.Select(Restored),
      Knowledge.Of(
        country.Known,
        country.UnderWay.Select(development => new Development(development.Technique, LaborTime.FromHours(development.Spent)))));
  }

  private static ProvinceSnapshot Taken(Province province)
  {
    return new ProvinceSnapshot(
      province.Id.Value, province.Name,
      [.. province.Composition.Presences.Select(presence => new ClassSnapshot(presence.Class, presence.Heads))],
      [.. province.Branches.Select(Taken)],
      Sorted(province.Store));
  }

  private static Province Restored(ProvinceSnapshot province)
  {
    return Province.Load(
      ProvinceId.From(province.Id), province.Name,
      ClassComposition.Of([.. province.Classes.Select(presence => ClassPresence.Of(presence.Class, presence.Heads))]),
      province.Branches.Select(Restored),
      Store.Of([.. province.Store.Select(Restored)]));
  }

  private static BranchSnapshot Taken(Branch branch)
  {
    return new BranchSnapshot(
      branch.Produces.Value, branch.Form, branch.Means.Wear, [.. branch.Means.Instruments.Select(Taken)],
      [.. branch.Means.Subjects.Select(Taken)], branch.Workforce, branch.Hands, branch.WorkingDay.Hours,
      branch.NecessaryLabor.Hours, branch.OutputPerHour.Amount, branch.OutputPerHour.Unit.Name);
  }

  private static Branch Restored(BranchSnapshot branch)
  {
    return Branch.Load(
      CommodityId.From(branch.Produces),
      MeansOfProduction.SetInMotion(
        branch.Form, branch.Wear, branch.Instruments.Select(Restored), branch.Material.Select(Restored)),
      branch.Workforce, branch.Hands, LaborTime.FromHours(branch.WorkingDay), LaborTime.FromHours(branch.NecessaryLabor),
      Quantity.Of(branch.OutputPerHour, UnitOfMeasure.Of(branch.OutputUnit)));
  }

  private static WarSnapshot Taken(War war)
  {
    return new WarSnapshot(
      war.Id.Value, war.Aggressor.Value, war.Defender.Value, war.Momentum,
      war.Peace is { } peace ? new PeaceSnapshot(peace.Terms, [.. peace.Ceded.Select(province => province.Value)]) : null);
  }

  private static War Restored(WarSnapshot war)
  {
    Peace? peace = war.Peace switch
    {
      null => null,
      { Terms: PeaceTerms.White } => Peace.White(),
      { Terms: PeaceTerms.Annexation } => Peace.Annexation(),
      var ceding => Peace.Ceding([.. ceding.Ceded.Select(ProvinceId.From)])
    };

    return War.Load(WarId.From(war.Id), CountryId.From(war.Aggressor), CountryId.From(war.Defender), war.Momentum, peace);
  }

  /// <summary>
  /// Only declarations and treaties are ever left waiting for the next day, so
  /// only they are saved. Anything else waiting is a fault worth knowing about.
  /// </summary>
  private static OccurrenceSnapshot Taken(Occurrence occurrence)
  {
    return occurrence switch
    {
      WarWasDeclared declared => new OccurrenceSnapshot(nameof(WarWasDeclared), [declared.Aggressor, declared.Defender]),
      PeaceWasMade peace => new OccurrenceSnapshot(nameof(PeaceWasMade), [peace.Winner, peace.Loser, peace.Terms]),
      _ => throw new DomainException($"A {occurrence.GetType().Name} waiting for the next day cannot be saved.")
    };
  }

  private static Occurrence Restored(OccurrenceSnapshot occurrence)
  {
    return occurrence switch
    {
      { Kind: nameof(WarWasDeclared), Fields: [var aggressor, var defender] } => new WarWasDeclared(aggressor, defender),
      { Kind: nameof(PeaceWasMade), Fields: [var winner, var loser, var terms] } => new PeaceWasMade(winner, loser, terms),
      _ => throw new DomainException($"The save file holds an occurrence this game does not know: {occurrence.Kind}.")
    };
  }

  #endregion
}
