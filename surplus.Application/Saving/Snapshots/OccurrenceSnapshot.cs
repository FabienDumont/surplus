namespace Surplus.Application.Saving.Snapshots;

/// <summary>
/// An occurrence waiting for the next day's report: which kind it is, and its
/// fields in the order its record declares them.
/// </summary>
public sealed record OccurrenceSnapshot(
  string Kind, string[] Fields);
