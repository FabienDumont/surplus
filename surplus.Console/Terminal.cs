using System.Globalization;
using Surplus.Application;
using Surplus.Application.Reporting;
using Surplus.Application.Settings;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Politics;
using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Console;

/// <summary>
/// The command loop: reads a line, answers it, prompts again. It reads and
/// writes through whatever it is handed, so a pipe or a test can drive it as
/// well as a person at a keyboard.
/// Things are named by the numbers the listings print, never by typing their
/// names, since names have spaces in them. Every order goes through the
/// session, and every refusal the domain makes is printed as it was written:
/// those messages are meant for the player.
/// </summary>
public sealed class Terminal
{
  #region Fields

  private static readonly LaborTime TwelveHours = LaborTime.FromHours(12m);

  private static readonly Dictionary<string, string> Usage = new()
  {
    ["next"] = "next takes a number of days, month or year, e.g. next 30 or next month.",
    ["country"] = "country takes a country's number from status, e.g. country 2.",
    ["enact"] = "enact takes a law, e.g. enact EnclosureActs. Type laws to see them.",
    ["repeal"] = "repeal takes a law in force, e.g. repeal EnclosureActs.",
    ["form"] = "form takes a form of government, e.g. form Republic.",
    ["adopt"] = "adopt takes an ideology, e.g. adopt Liberalism.",
    ["open"] = "open takes a province, a technique, a class, hands and optionally hours, e.g. open 1 2 Proletariat 1000 12.",
    ["close"] = "close takes a province and a commodity, e.g. close 1 2.",
    ["employ"] = "employ takes a province, a commodity and hands, e.g. employ 1 2 1000.",
    ["layoff"] = "layoff takes a province, a commodity and hands, e.g. layoff 1 2 1000.",
    ["reequip"] = "reequip takes a province, a commodity and a technique, e.g. reequip 1 2 5.",
    ["day"] = "day takes a province, a commodity and hours, e.g. day 1 2 10."
  };

  private readonly TextReader _input;
  private readonly TextWriter _output;
  private readonly Session _session;
  private readonly IPreferenceStore _store;
  private Passage? _lastPassage;
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

    if (_session.World.Played is { } played)
    {
      _output.WriteLine($"You govern {played.Name}. Type help to see what you can order.");
    }

    while (true)
    {
      _output.Write($"{(_session.World.Played is null ? "(no country) " : "")}{_session.Game.CurrentDate}> ");

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
      case "report":
        Report();
        break;
      case "status":
        Status();
        break;
      case "provinces":
        ProvincesOf(Played());
        break;
      case "stores":
        StoresOf(Played());
        break;
      case "values":
        Values();
        break;
      case "techniques":
        Techniques();
        break;
      case "laws":
        Laws();
        break;
      case "country":
        Country(arguments);
        break;
      case "units":
        Units(arguments);
        break;
      case "enact":
      case "repeal":
        Legislate(command, arguments);
        break;
      case "form":
        Form(arguments);
        break;
      case "adopt":
        Adopt(arguments);
        break;
      case "open":
        Open(arguments);
        break;
      case "close":
        Close(arguments);
        break;
      case "employ":
        Employ(arguments);
        break;
      case "layoff":
        LayOff(arguments);
        break;
      case "reequip":
        Reequip(arguments);
        break;
      case "day":
        Day(arguments);
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
    if (arguments.Length > 1 || DaysAsked(arguments) is not { } days)
    {
      _output.WriteLine(Usage["next"]);

      return;
    }

    var dayBefore = _lastPassage?.Reports[^1];
    var passage = _session.AdvanceUntil(days, report =>
    {
      var calls = Significance.CallsForAttention(report, dayBefore, _session.World);

      dayBefore = report;

      return calls;
    });
    var digest = Digest.Of(passage, _session.World, _preferences.Units);

    _lastPassage = passage;

    if (digest.Interrupted)
    {
      _output.WriteLine($"Stopped on {digest.To}: something needs your attention.");
    }

    _output.WriteLine(digest.From == digest.To ? $"{digest.To}" : $"{digest.From} to {digest.To}");

    foreach (var line in digest.Lines)
    {
      _output.WriteLine($"  {line}");
    }
  }

  /// <summary>Days to the first of the next month or year, or the number typed; null for anything else.</summary>
  private int? DaysAsked(string[] arguments)
  {
    var today = _session.Game.CurrentDate.Date;

    if (arguments.Length == 0)
    {
      return 1;
    }

    return arguments[0] switch
    {
      "month" => new DateOnly(today.Year, today.Month, 1).AddMonths(1).DayNumber - today.DayNumber,
      "year" => new DateOnly(today.Year + 1, 1, 1).DayNumber - today.DayNumber,
      _ => int.TryParse(arguments[0], NumberStyles.None, CultureInfo.InvariantCulture, out var days) ? days : null
    };
  }

  private void Report()
  {
    var dated = _lastPassage?.Reports
      .SelectMany(report => report.Occurrences, (report, occurrence) => $"{report.Date}  {occurrence}")
      .ToList() ?? [];

    if (dated.Count == 0)
    {
      _output.WriteLine("Nothing was reported.");

      return;
    }

    foreach (var line in dated)
    {
      _output.WriteLine(line);
    }
  }

  private void Status()
  {
    _output.WriteLine(_session.Game.CurrentDate);

    var countries = _session.World.Countries;

    for (var index = 0; index < countries.Count; index++)
    {
      var country = countries[index];
      var days = Standing.DaysOfSubsistence(country, _session.World.Subsistence);

      _output.WriteLine(
        $"[{index + 1}] {country.Name}{(country.IsPlayed ? " (you)" : "")} — {country.Government.Ideology} " +
        $"{country.Government.Form}, {Number(country.Population)} souls, {country.DominantMode}, " +
        (days is { } kept ? $"{Number(Math.Floor(kept))} days of subsistence in store" : "no one to keep"));
    }
  }

  private void ProvincesOf(Country? country)
  {
    if (country is null)
    {
      return;
    }

    for (var index = 0; index < country.Provinces.Count; index++)
    {
      var province = country.Provinces[index];

      _output.WriteLine($"[{index + 1}] {province.Name} — {province.Mode}");
      _output.WriteLine(
        "    Classes: " +
        string.Join(", ", province.Composition.Presences.Select(presence => $"{presence.Class} {Number(presence.Heads)}")));

      foreach (var branch in province.Branches)
      {
        _output.WriteLine(
          $"    [{CommodityNumber(branch.Produces)}] {Commodities()[CommodityNumber(branch.Produces) - 1].Name}: " +
          $"{branch.Form}, {Number(branch.Hands)} {branch.Workforce}, {Shown(branch.OutputPerHour)} an hour, " +
          $"necessary labour {Number(branch.NecessaryLabor.Hours)} h of {Number(branch.WorkingDay.Hours)} h, " +
          RateOfSurplusValue(branch));
      }

      var idle = province.Composition.Presences
        .Where(presence => presence.Profile.IsDirectProducer && province.Idle(presence.Class) > 0)
        .Select(presence => $"{presence.Class} {Number(province.Idle(presence.Class))}")
        .ToList();

      _output.WriteLine($"    Without work: {(idle.Count == 0 ? "no one" : string.Join(", ", idle))}");
    }
  }

  /// <summary>Surplus labour over necessary labour, as a percentage; there is none to speak of where nothing is paid back.</summary>
  private string RateOfSurplusValue(Branch branch)
  {
    var composition = branch.Work(_session.World.Register).Composition;

    return composition.VariableCapital.IsNone
      ? "no necessary labour to set surplus against"
      : $"rate of surplus-value {Number(Math.Round(composition.RateOfSurplusValue() * 100m, 1))}%";
  }

  private void StoresOf(Country? country)
  {
    if (country is null)
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
      _output.WriteLine(
        $"{_session.World.Register.Get(stock.Commodity).Name}: {Shown(stock.Quantity)} " +
        $"({Number(_session.World.Register.ValueOf(stock).Magnitude.Hours)} h)");
    }
  }

  private void Values()
  {
    var commodities = Commodities();

    for (var index = 0; index < commodities.Count; index++)
    {
      var commodity = commodities[index];
      var unit = commodity.UseValue.Unit;
      var perUnit = Measures.Shown(commodity.Value, unit, _preferences.Units);

      _output.WriteLine(
        $"[{index + 1}] {commodity.Name}: {Number(perUnit.Magnitude.Hours)} h per {Measures.Shown(unit, _preferences.Units)}");
    }
  }

  private void Techniques()
  {
    var techniques = _session.World.Techniques;

    for (var index = 0; index < techniques.Count; index++)
    {
      var technique = techniques[index];

      _output.WriteLine(
        $"[{index + 1}] {technique.Name}: {_session.World.Register.Get(technique.Produces).Name}, {technique.Form}, " +
        $"{Shown(technique.OutputPerHour)} an hour; instruments per hand: {Stocks(technique.InstrumentsPerHand)}; " +
        $"material per hand a day: {Stocks(technique.MaterialPerHand)}");
    }
  }

  private void Laws()
  {
    if (Played() is not { } country)
    {
      return;
    }

    var inForce = country.Government.Laws;

    _output.WriteLine($"In force: {(inForce.Count == 0 ? "none" : string.Join(", ", inForce))}");
    _output.WriteLine($"Not in force: {string.Join(", ", Enum.GetValues<Law>().Where(law => !inForce.Contains(law)))}");
  }

  private void Country(string[] arguments)
  {
    var countries = _session.World.Countries;

    if (arguments.Length != 1 || !Index(arguments[0], countries.Count, out var index))
    {
      _output.WriteLine(Usage["country"]);

      return;
    }

    var country = countries[index];

    _output.WriteLine($"{country.Name} — {country.Government.Ideology} {country.Government.Form}");
    ProvincesOf(country);
    StoresOf(country);
  }

  private void Units(string[] arguments)
  {
    if (arguments.Length == 0)
    {
      _output.WriteLine($"Quantities are shown in {Named(_preferences.Units)} units.");

      return;
    }

    if (arguments.Length > 1 || !TryName<UnitSystem>(arguments[0], out var units))
    {
      _output.WriteLine("units takes metric or imperial, e.g. units metric.");

      return;
    }

    _preferences = _preferences.Measuring(units);
    _store.Save(_preferences);

    _output.WriteLine($"Quantities are now shown in {Named(units)} units.");
  }

  private void Legislate(string command, string[] arguments)
  {
    if (arguments.Length != 1 || !TryName<Law>(arguments[0], out var law))
    {
      _output.WriteLine(Usage[command]);

      return;
    }

    if (command == "enact")
    {
      _session.Enact(law);
      _output.WriteLine($"{law} is now law.");
    }
    else
    {
      _session.Repeal(law);
      _output.WriteLine($"{law} is repealed.");
    }
  }

  private void Form(string[] arguments)
  {
    if (arguments.Length != 1 || !TryName<GovernmentForm>(arguments[0], out var form))
    {
      _output.WriteLine(Usage["form"]);

      return;
    }

    _session.TakeForm(form);
    _output.WriteLine($"The state is now a {form}.");
  }

  private void Adopt(string[] arguments)
  {
    if (arguments.Length != 1 || !TryName<Ideology>(arguments[0], out var ideology))
    {
      _output.WriteLine(Usage["adopt"]);

      return;
    }

    _session.Adopt(ideology);
    _output.WriteLine($"The state now professes {ideology}.");
  }

  private void Open(string[] arguments)
  {
    var hours = TwelveHours;

    if (arguments.Length is < 4 or > 5 ||
        !ProvinceAt(arguments[0], out var province) ||
        !Index(arguments[1], _session.World.Techniques.Count, out var technique) ||
        !TryName<SocialClass>(arguments[2], out var workforce) ||
        !Count(arguments[3], out var hands) ||
        (arguments.Length == 5 && !Hours(arguments[4], out hours)))
    {
      _output.WriteLine(Usage["open"]);

      return;
    }

    var chosen = _session.World.Techniques[technique];

    _session.OpenBranch(province.Id, chosen.Name, workforce, hands, hours);
    _output.WriteLine(
      $"{province.Name} now works {_session.World.Register.Get(chosen.Produces).Name} by {chosen.Name}, " +
      $"with {Number(hands)} {workforce} at {Number(hours.Hours)} h a day.");
  }

  private void Close(string[] arguments)
  {
    if (arguments.Length != 2 || !ProvinceAt(arguments[0], out var province) ||
        !CommodityAt(arguments[1], out var commodity))
    {
      _output.WriteLine(Usage["close"]);

      return;
    }

    _session.CloseBranch(province.Id, commodity.Id);
    _output.WriteLine($"{province.Name} no longer works {commodity.Name}.");
  }

  private void Employ(string[] arguments)
  {
    if (arguments.Length != 3 || !ProvinceAt(arguments[0], out var province) ||
        !CommodityAt(arguments[1], out var commodity) || !Count(arguments[2], out var hands))
    {
      _output.WriteLine(Usage["employ"]);

      return;
    }

    _session.Employ(province.Id, commodity.Id, hands);
    _output.WriteLine(
      $"{province.Name}'s {commodity.Name} branch took on {Number(hands)} hands; {HandsNow(province, commodity)}.");
  }

  private void LayOff(string[] arguments)
  {
    if (arguments.Length != 3 || !ProvinceAt(arguments[0], out var province) ||
        !CommodityAt(arguments[1], out var commodity) || !Count(arguments[2], out var hands))
    {
      _output.WriteLine(Usage["layoff"]);

      return;
    }

    _session.LayOff(province.Id, commodity.Id, hands);
    _output.WriteLine(
      $"{province.Name}'s {commodity.Name} branch turned off {Number(hands)} hands; {HandsNow(province, commodity)}.");
  }

  private void Reequip(string[] arguments)
  {
    if (arguments.Length != 3 || !ProvinceAt(arguments[0], out var province) ||
        !CommodityAt(arguments[1], out var commodity) ||
        !Index(arguments[2], _session.World.Techniques.Count, out var technique))
    {
      _output.WriteLine(Usage["reequip"]);

      return;
    }

    var chosen = _session.World.Techniques[technique];

    _session.Reequip(province.Id, commodity.Id, chosen.Name);
    _output.WriteLine($"{province.Name}'s {commodity.Name} branch now works by {chosen.Name}.");
  }

  private void Day(string[] arguments)
  {
    if (arguments.Length != 3 || !ProvinceAt(arguments[0], out var province) ||
        !CommodityAt(arguments[1], out var commodity) || !Hours(arguments[2], out var hours))
    {
      _output.WriteLine(Usage["day"]);

      return;
    }

    _session.FixTheWorkingDay(province.Id, commodity.Id, hours);
    _output.WriteLine($"{province.Name}'s {commodity.Name} branch now works {Number(hours.Hours)} h a day.");
  }

  private static string HandsNow(Province province, Commodity commodity)
  {
    var branch = province.Branches.SingleOrDefault(candidate => candidate.Produces == commodity.Id);

    return branch is null ? "with no one left, it is closed" : $"it now has {Number(branch.Hands)}";
  }

  private void Help()
  {
    _output.WriteLine("Time");
    _output.WriteLine("  next [n|month|year]      let time pass, stopping if something needs you");
    _output.WriteLine("  report                   everything that happened in the last stretch, day by day");
    _output.WriteLine("Looking");
    _output.WriteLine("  status                   the date, and every country, numbered");
    _output.WriteLine("  provinces                your provinces, numbered: classes, branches, labour, who is without work");
    _output.WriteLine("  stores                   what your country has laid up, and what it is worth");
    _output.WriteLine("  values                   every commodity, numbered, and its value");
    _output.WriteLine("  techniques               every known way of working, numbered");
    _output.WriteLine("  laws                     the laws in force and the rest");
    _output.WriteLine("  country <country#>       another country's provinces and stores");
    _output.WriteLine("  units [metric|imperial]  show or set the units quantities are shown in");
    _output.WriteLine("Economy");
    _output.WriteLine("  open <province#> <technique#> <class> <hands> [hours]  open a branch (12 h a day unless told)");
    _output.WriteLine("  close <province#> <commodity#>                         close a branch");
    _output.WriteLine("  employ <province#> <commodity#> <hands>                take on hands");
    _output.WriteLine("  layoff <province#> <commodity#> <hands>                turn hands off");
    _output.WriteLine("  reequip <province#> <commodity#> <technique#>          work a branch by another technique");
    _output.WriteLine("  day <province#> <commodity#> <hours>                   fix the working day");
    _output.WriteLine("State");
    _output.WriteLine("  enact <law> / repeal <law>");
    _output.WriteLine("  form <governmentForm>    e.g. form Republic");
    _output.WriteLine("  adopt <ideology>         e.g. adopt Liberalism");
    _output.WriteLine("help, quit");
  }

  private bool ProvinceAt(string argument, out Province province)
  {
    province = null!;

    if (Played() is not { } country || !Index(argument, country.Provinces.Count, out var index))
    {
      return false;
    }

    province = country.Provinces[index];

    return true;
  }

  private bool CommodityAt(string argument, out Commodity commodity)
  {
    var commodities = Commodities();

    commodity = null!;

    if (!Index(argument, commodities.Count, out var index))
    {
      return false;
    }

    commodity = commodities[index];

    return true;
  }

  /// <summary>A number as the listings print it, from 1, turned into a position from 0.</summary>
  private static bool Index(string argument, int count, out int index)
  {
    index = -1;

    if (!int.TryParse(argument, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ||
        number < 1 || number > count)
    {
      return false;
    }

    index = number - 1;

    return true;
  }

  private static bool Count(string argument, out int count)
  {
    return int.TryParse(argument, NumberStyles.None, CultureInfo.InvariantCulture, out count);
  }

  private static bool Hours(string argument, out LaborTime hours)
  {
    hours = LaborTime.None;

    if (!decimal.TryParse(argument, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount))
    {
      return false;
    }

    hours = LaborTime.FromHours(amount);

    return true;
  }

  /// <summary>An enum value typed by name, in any case; numbers are refused, though the parser would take them.</summary>
  private static bool TryName<T>(string argument, out T value) where T : struct, Enum
  {
    value = default;

    return argument.All(char.IsLetter) && Enum.TryParse(argument, true, out value);
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

  private List<Commodity> Commodities()
  {
    return [.. _session.World.Register.Commodities];
  }

  private int CommodityNumber(CommodityId commodity)
  {
    return Commodities().FindIndex(candidate => candidate.Id == commodity) + 1;
  }

  private string Shown(Quantity quantity)
  {
    var shown = Measures.Shown(quantity, _preferences.Units);

    // Rates per hand run to small fractions; four places would turn 0.00625 t into 0.0063.
    return $"{shown.Amount.ToString("#,0.######", CultureInfo.InvariantCulture)} {shown.Unit}";
  }

  private string Stocks(IReadOnlyList<Stock> stocks)
  {
    return stocks.Count == 0
      ? "none"
      : string.Join(", ", stocks.Select(stock => $"{Shown(stock.Quantity)} {_session.World.Register.Get(stock.Commodity).Name}"));
  }

  private static string Named(UnitSystem units)
  {
    return units.ToString().ToLowerInvariant();
  }

  private static string Number(decimal number)
  {
    return number.ToString("#,0.####", CultureInfo.InvariantCulture);
  }

  #endregion
}
