namespace Surplus.Domain.Simulation.Society;

/// <summary>The revenue form through which a class lives.</summary>
public enum IncomeSource
{
  /// <summary>Kept alive by another, as the ox is kept: no income at all.</summary>
  Maintenance,

  /// <summary>Consumes what it produces, less whatever is taken from it.</summary>
  OwnProduct,

  /// <summary>The price of labour-power, which makes unpaid labour look paid.</summary>
  Wages,

  /// <summary>The tribute the earth commands for its owner.</summary>
  Rent,

  /// <summary>Surplus-value appropriated in production.</summary>
  Profit,

  /// <summary>Profit on alienation — bought cheap, sold dear.</summary>
  MerchantProfit,

  /// <summary>The share of surplus-value falling to money lent out as capital.</summary>
  Interest,

  /// <summary>The Church's levy on the produce of the land.</summary>
  Tithe,

  /// <summary>Neither produced nor appropriated in production: taken.</summary>
  Plunder,

  /// <summary>A share of the common product, distributed rather than exchanged.</summary>
  Distribution
}
