using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.GameEngine;

/// <summary>
/// A date on the in-game calendar. The simulation clock only ever moves
/// forward, one day at a time.
/// </summary>
public readonly record struct GameDate(DateOnly Date) : IComparable<GameDate>
{
    public static GameDate Of(int year, int month, int day)
    {
        try
        {
            return new GameDate(new DateOnly(year, month, day));
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new DomainException($"{year:0000}-{month:00}-{day:00} is not a valid calendar date.");
        }
    }

    public GameDate NextDay() => new(Date.AddDays(1));

    public int CompareTo(GameDate other) => Date.CompareTo(other.Date);

    public override string ToString() => Date.ToString("yyyy-MM-dd");
}
