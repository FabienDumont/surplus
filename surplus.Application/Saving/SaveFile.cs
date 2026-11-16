using System.Text.Json;
using System.Text.Json.Serialization;
using Surplus.Application.Saving.Snapshots;
using Surplus.Domain.SharedKernel;

namespace Surplus.Application.Saving;

/// <summary>
/// A session written as text and read back. No file is touched here: where a
/// save is kept is the console's business. What is written is the whole state
/// of the game, so that a session read back goes on exactly as the original
/// would have. The player's preferences are not part of it; they belong to
/// the player, not to the game.
/// </summary>
public static class SaveFile
{
  #region Fields

  /// <summary>The only version there is. It exists so a later change can refuse older files cleanly.</summary>
  private const int Version = 1;

  private static readonly JsonSerializerOptions Options = new()
  {
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() },
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true
  };

  #endregion

  #region Methods

  public static string Write(Session session)
  {
    return JsonSerializer.Serialize(Snapshotting.Taken(session, Version), Options);
  }

  /// <summary>Reads a session back, refusing in a sentence anything that is not a save file of this version.</summary>
  public static Session Read(string json)
  {
    SessionSnapshot? snapshot;

    try
    {
      snapshot = JsonSerializer.Deserialize<SessionSnapshot>(json, Options);
    }
    catch (JsonException)
    {
      throw new DomainException("That is not a save file this game can read.");
    }

    if (snapshot is null)
    {
      throw new DomainException("That is not a save file this game can read.");
    }

    if (snapshot.Version != Version)
    {
      throw new DomainException(
        $"That save file is of version {snapshot.Version}, and this game reads only version {Version}.");
    }

    try
    {
      return Snapshotting.Restored(snapshot);
    }
    catch (DomainException refusal)
    {
      throw new DomainException($"That save file holds something this game cannot rebuild: {refusal.Message}");
    }
  }

  #endregion
}
