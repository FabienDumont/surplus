namespace Surplus.Domain.Simulation.Society;

/// <summary>
/// The classes history has actually thrown up. They are not transhistorical:
/// "freeman and slave, patrician and plebeian, lord and serf, guild-master and
/// journeyman" — each mode of production has its own, and a class outliving the
/// mode that bore it is a survival, not a fixture.
/// A few classes genuinely straddle modes: merchant's capital and usurer's
/// capital are, in Marx's phrase, antediluvian forms of capital, older than the
/// capitalist mode and outliving every one they attach themselves to.
/// </summary>
public enum SocialClass
{
  // Primitive communal
  ClanMembers,

  // Slave
  SlaveOwners,
  Slaves,
  Freedmen,
  Plebeians,

  // Feudal
  FeudalLords,
  Vassals,
  Clergy,
  Serfs,
  FreePeasants,
  GuildMasters,
  Journeymen,
  Apprentices,

  // Antediluvian forms of capital, spanning several modes
  Merchants,
  Usurers,

  // Capitalist
  Bourgeoisie,
  Financiers,
  Landowners,
  PettyBourgeoisie,
  Peasantry,
  Proletariat,
  AgriculturalProletariat,
  Lumpenproletariat,

  // Socialist
  Bureaucracy,
  CollectivisedPeasantry,

  // Communist
  AssociatedProducers
}
