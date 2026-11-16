using Surplus.Application.Ai;
using Surplus.Application.Reporting;
using Surplus.Application.Scenarios;
using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;
using Surplus.Domain.Simulation.Warfare;

namespace Surplus.Application;

/// <summary>
/// One game being played: the engine's clock and the simulation's world, held
/// together. Neither knows of the other. <see cref="Game" /> advances the
/// calendar without knowing what a day holds, and <see cref="World" /> lives
/// through a day without keeping the calendar, so joining them falls to the
/// application layer.
/// A session is always paused between calls. Time passes only inside
/// <see cref="Advance" /> and <see cref="AdvanceUntil" />, so whatever the
/// player orders between two advances happens on a stopped clock.
/// Every order acts for the country the human plays and for no other. The
/// session adds no rule of its own to any of them: it finds the country, and
/// the domain decides whether the order can be carried out.
/// </summary>
public sealed class Session
{
  #region Properties

  public Game Game { get; }
  public World World { get; }

  #endregion

  #region Ctors

  private Session(Game game, World world)
  {
    Game = game;
    World = world;
  }

  #endregion

  #region Methods

  /// <summary>Starts a new game from a scenario, paused on its start date.</summary>
  public static Session Begin(Scenario scenario)
  {
    return new Session(Game.Start(scenario.Name, scenario.StartDate), scenario.World);
  }

  /// <summary>Takes up a game already under way, as it was saved.</summary>
  public static Session Resume(Game game, World world)
  {
    return new Session(game, world);
  }

  /// <summary>Lets so many days pass and reports each of them.</summary>
  public IReadOnlyList<DayReport> Advance(int days)
  {
    return Pass(days, _ => false);
  }

  /// <summary>
  /// Lets up to so many days pass, and stops after the first one that
  /// interrupts: a player who asked for a year does not want to read of a
  /// famine at home eleven months after it began.
  /// </summary>
  public Passage AdvanceUntil(int days, Func<DayReport, bool> interrupts)
  {
    var before = World.Played?.Store;
    var interrupted = false;

    var reports = Pass(days, report => interrupted = interrupts(report));

    return new Passage(reports, before, interrupted);
  }

  /// <summary>Every day passes through here, whichever way time was asked to pass.</summary>
  private List<DayReport> Pass(int days, Func<DayReport, bool> stopsAfter)
  {
    if (days < 1)
    {
      throw new DomainException("Time is advanced by at least one day.");
    }

    if (Game.Status is GameStatus.Paused)
    {
      Game.Resume();
    }

    List<DayReport> reports = [];

    try
    {
      for (var day = 0; day < days; day++)
      {
        Game.AdvanceOneDay();

        var report = World.Elapse(Game.CurrentDate);

        if (Game.CurrentDate.Date.Day == 1)
        {
          LetTheMachinePlay();
        }

        reports.Add(report);

        if (stopsAfter(report))
        {
          break;
        }
      }
    }
    finally
    {
      Game.Pause();
    }

    return reports;
  }

  /// <summary>
  /// At the turn of a month every country no one plays takes its turn. The
  /// list is read first, because a peace the machine makes can take a country
  /// out of the world while the others are still to act.
  /// </summary>
  private void LetTheMachinePlay()
  {
    foreach (var country in World.Countries.Where(country => !country.IsPlayed).ToList())
    {
      if (World.Countries.Contains(country))
      {
        CountryAi.Act(World, country);
      }
    }
  }

  public void Enact(Law law)
  {
    Played().Enact(law);
  }

  public void Repeal(Law law)
  {
    Played().Repeal(law);
  }

  public void TakeForm(GovernmentForm form)
  {
    Played().TakeForm(form);
  }

  public void Adopt(Ideology ideology)
  {
    Played().Adopt(ideology);
  }

  public void RelieveThePoor()
  {
    Played().RelieveThePoor();
  }

  public void WithdrawRelief()
  {
    Played().WithdrawRelief();
  }

  /// <summary>Opens a branch by a technique of the catalogue; what a hand costs to keep is the world's to say.</summary>
  public void OpenBranch(
    ProvinceId province, string technique, SocialClass workforce, int hands, LaborTime workingDay)
  {
    World.OpenBranch(Played().Id, province, World.Technique(technique), workforce, hands, workingDay);
  }

  public void CloseBranch(ProvinceId province, CommodityId commodity)
  {
    Played().CloseBranch(province, commodity);
  }

  public void Employ(ProvinceId province, CommodityId commodity, int hands)
  {
    Played().Employ(province, commodity, hands);
  }

  public void LayOff(ProvinceId province, CommodityId commodity, int hands)
  {
    Played().LayOff(province, commodity, hands);
  }

  public void Reequip(ProvinceId province, CommodityId commodity, string technique)
  {
    Played().Reequip(province, commodity, World.Technique(technique));
  }

  public void FixTheWorkingDay(ProvinceId province, CommodityId commodity, LaborTime workingDay)
  {
    Played().FixTheWorkingDay(province, commodity, workingDay);
  }

  public War DeclareWarOn(CountryId defender)
  {
    return World.DeclareWar(Played().Id, defender);
  }

  public void MakePeace(WarId war, Peace peace)
  {
    World.MakePeace(war, Played().Id, peace);
  }

  private Country Played()
  {
    return World.Played ?? throw new DomainException("No one is played: the state you sat in is no more.");
  }

  #endregion
}
