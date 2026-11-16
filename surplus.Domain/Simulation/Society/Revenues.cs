using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Society;

/// <summary>
/// What each form of revenue gives a title to. Kept as one table, like
/// <see cref="SocialClasses" />, because these are facts about the revenue
/// forms and not settings.
/// </summary>
public static class Revenues
{
  #region Fields

  private static readonly Dictionary<IncomeSource, Access> Titles = new()
  {
    // Rent, profit, interest and tithe are shares of the surplus product, and
    // the surplus product belongs to its appropriators before anyone else.
    [IncomeSource.Rent] = Access.First,
    [IncomeSource.Profit] = Access.First,
    [IncomeSource.MerchantProfit] = Access.First,
    [IncomeSource.Interest] = Access.First,
    [IncomeSource.Tithe] = Access.First,

    // The petty producer and the serf eat from the plot whether or not the
    // model has put them in a branch; the slave is kept as property is kept;
    // the common product is shared out.
    [IncomeSource.OwnProduct] = Access.Whole,
    [IncomeSource.Maintenance] = Access.Whole,
    [IncomeSource.Distribution] = Access.Whole,

    // The wage is paid only for labour-power actually bought. A worker nobody
    // buys has nothing to live on, however full the granaries.
    [IncomeSource.Wages] = Access.Employed,

    // Plunder is no title. What the lumpenproletariat lives on, the model does
    // not count.
    [IncomeSource.Plunder] = Access.None
  };

  #endregion

  #region Methods

  /// <summary>What a class's revenue gives it a title to.</summary>
  public static Access Title(this IncomeSource income)
  {
    return Titles.TryGetValue(income, out var access)
      ? access
      : throw new DomainException($"No title is registered for {income}.");
  }

  #endregion
}
