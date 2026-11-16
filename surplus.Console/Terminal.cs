using System.Globalization;
using Surplus.Application;
using Surplus.Application.Settings;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Countries;

namespace Surplus.Console;

/// <summary>
/// The command loop: reads a line, answers it, prompts again. It reads and
/// writes through whatever it is handed, so a pipe or a test can drive it as
/// well as a person at a keyboard. For now it can only watch: let time pass and
/// look at the world.
/// </summary>
public sealed class Terminal
{
  #region Fields

  private readonly TextReader _input;
  private readonly TextWriter _output;
  private readonly Session _session;
  private readonly IPreferenceStore _store;
  private UserPreferences _preferences;

  #endregion

  #region Ctors

  public Terminal(Session session, IPreferenceStore store, TextReader input, TextWriter output)
  {
    _session = session;
    _store = store;
    _preferences = store.Load();
    _input = input;
    _output = output;
  }

  #endregion

  #region Methods

  public void Run()
  {
    Status();

    while (true)
    {
      _output.Write($"{_session.Game.CurrentDate}> ");

      var line = _input.ReadLine();

      if (line is null)
      {
        _output.WriteLine();

        return;
      }

      var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

      if (words.Length == 0)
      {
        continue;
      }

      if (words[0] == "quit")
      {
        return;
      }

      try
      {
        Answer(words[0], words[1..]);
      }
      catch (DomainException refusal)
      {
        _output.WriteLine(refusal.Message);
      }
    }
  }

  private void Answer(string command, string[] arguments)
  {
    switch (command)
    {
      case "next":
        Next(arguments);
        break;
      case "status":
        Status();
        break;
      case "provinces":
        Provinces();
        break;
      case "stores":
        Stores();
        break;
      case "values":
        Values();
        break;
      case "units":
        Units(arguments);
        break;
      case "help":
        Help();
        break;
      default:
        _output.WriteLine($"Unknown command '{command}'. Type help for the list.");
        break;
    }
  }

  private void Next(string[] arguments)
  {
    var days = 1;

    if (arguments.Length > 1 ||
        (arguments.Length == 1 && !int.TryParse(arguments[0], NumberStyles.None, CultureInfo.InvariantCulture, out days)))
    {
      _output.WriteLine("next takes a number of days, e.g. next 30.");

      return;
    }

    foreach (var occurrence in _session.Advance(days).SelectMany(report => report.Occurrences))
    {
      _output.WriteLine(occurrence);
    }

    _output.WriteLine(_session.Game.CurrentDate);
  }

  private void Status()
  {
    _output.WriteLine(_session.Game.CurrentDate);

    foreach (var country in _session.World.Countries)
    {
      _output.WriteLine(
        $"{country.Name} — {country.Control}, {country.Government.Ideology} {country.Government.Form}, " +
        $"{Number(country.Population)} souls, {country.DominantMode}");
    }
  }

  private void Provinces()
  {
    if (Played() is not { } country)
    {
      return;
    }

    foreach (var province in country.Provinces)
    {
      _output.WriteLine($"{province.Name} — {province.Mode}");
      _output.WriteLine(
        "  Classes: " +
        string.Join(", ", province.Composition.Presences.Select(presence => $"{presence.Class} {Number(presence.Heads)}")));

      foreach (var branch in province.Branches)
      {
        _output.WriteLine(
          $"  {_session.World.Register.Get(branch.Produces).Name}: {branch.Form}, " +
          $"{Number(branch.Hands)} {branch.Workforce}");
      }

      foreach (var socialClass in province.Branches.Select(branch => branch.Workforce).Distinct())
      {
        _output.WriteLine($"  Idle {socialClass}: {Number(province.Idle(socialClass))}");
      }
    }
  }

  private void Stores()
  {
    if (Played() is not { } country)
    {
      return;
    }

    if (country.Store.Stocks.Count == 0)
    {
      _output.WriteLine("Nothing is laid up.");

      return;
    }

    foreach (var stock in country.Store.Stocks)
    {
      var shown = Measures.Shown(stock.Quantity, _preferences.Units);

      _output.WriteLine(
        $"{_session.World.Register.Get(stock.Commodity).Name}: {Number(shown.Amount)} {shown.Unit} " +
        $"({Number(_session.World.Register.ValueOf(stock).Magnitude.Hours)} h)");
    }
  }

  private void Values()
  {
    foreach (var commodity in _session.World.Register.Commodities)
    {
      var unit = commodity.UseValue.Unit;
      var perUnit = Measures.Shown(commodity.Value, unit, _preferences.Units);

      _output.WriteLine(
        $"{commodity.Name}: {Number(perUnit.Magnitude.Hours)} h per {Measures.Shown(unit, _preferences.Units)}");
    }
  }

  private void Units(string[] arguments)
  {
    if (arguments.Length == 0)
    {
      _output.WriteLine($"Quantities are shown in {Named(_preferences.Units)} units.");

      return;
    }

    if (arguments.Length > 1 || !arguments[0].All(char.IsLetter) ||
        !Enum.TryParse<UnitSystem>(arguments[0], true, out var units))
    {
      _output.WriteLine("units takes metric or imperial, e.g. units metric.");

      return;
    }

    _preferences = _preferences.Measuring(units);
    _store.Save(_preferences);

    _output.WriteLine($"Quantities are now shown in {Named(units)} units.");
  }

  private static string Named(UnitSystem units)
  {
    return units.ToString().ToLowerInvariant();
  }

  private void Help()
  {
    _output.WriteLine("next [n]    let n days pass (default 1)");
    _output.WriteLine("status      the date, and every country");
    _output.WriteLine("provinces   your provinces: classes, branches, who stands idle");
    _output.WriteLine("stores      what your country has laid up, and what it is worth");
    _output.WriteLine("values      every commodity and its value");
    _output.WriteLine("units [u]   show or set the units quantities are shown in: metric or imperial");
    _output.WriteLine("help        this list");
    _output.WriteLine("quit        leave");
  }

  private Country? Played()
  {
    var country = _session.World.Played;

    if (country is null)
    {
      _output.WriteLine("You no longer have a country.");
    }

    return country;
  }

  private static string Number(decimal number)
  {
    return number.ToString("#,0.####", CultureInfo.InvariantCulture);
  }

  #endregion
}
