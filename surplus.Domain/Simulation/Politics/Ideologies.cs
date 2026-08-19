using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Politics;

/// <summary>
/// The register of what each ideology is, materially. The bureaucracy doctrines
/// here are deliberately irreconcilable: the simulation records what each
/// tendency holds without ruling on which is correct.
/// </summary>
public static class Ideologies
{
  #region Fields

  private static readonly Dictionary<Ideology, IdeologyProfile> Register = new IdeologyProfile[]
  {
    // The old order defending itself.
    new(Ideology.Traditionalism, SocialClass.FeudalLords, ModeOfProduction.Feudal,
      BureaucracyDoctrine.NotAtIssue, false, 1),
    new(Ideology.Clericalism, SocialClass.Clergy, ModeOfProduction.Feudal,
      BureaucracyDoctrine.NotAtIssue, false, 1),

    // Capital, in its confident and its frightened forms alike.
    new(Ideology.Conservatism, SocialClass.Landowners, ModeOfProduction.Capitalist,
      BureaucracyDoctrine.NotAtIssue, false, 2),
    new(Ideology.Liberalism, SocialClass.Bourgeoisie, ModeOfProduction.Capitalist,
      BureaucracyDoctrine.NotAtIssue, false, 2),
    new(Ideology.Nationalism, SocialClass.PettyBourgeoisie, ModeOfProduction.Capitalist,
      BureaucracyDoctrine.NotAtIssue, false, 3),

    // Fascism drives toward no new mode: it is capital's rule by other means,
    // which is why it can smash the workers' movement without touching the base.
    new(Ideology.Fascism, SocialClass.Financiers, ModeOfProduction.Capitalist,
      BureaucracyDoctrine.NotAtIssue, false, 4),

    // Socialism before Marx: it appealed to reason rather than to a class.
    new(Ideology.UtopianSocialism, SocialClass.Proletariat, ModeOfProduction.Socialist,
      BureaucracyDoctrine.NotAtIssue, false, 1),

    // Reform, which administers the base it means to outgrow.
    new(Ideology.SocialDemocracy, SocialClass.Proletariat, ModeOfProduction.Capitalist,
      BureaucracyDoctrine.NotAtIssue, true, 2),

    new(Ideology.ClassicalMarxism, SocialClass.Proletariat, ModeOfProduction.Communist,
      BureaucracyDoctrine.NotAtIssue, true, 4),

    // The tendencies that split over what the apparatus of a workers' state is.
    new(Ideology.MarxismLeninism, SocialClass.Proletariat, ModeOfProduction.Communist,
      BureaucracyDoctrine.OrganOfTheWorkersState, true, 5),
    new(Ideology.Maoism, SocialClass.Peasantry, ModeOfProduction.Communist,
      BureaucracyDoctrine.NewBourgeoisie, true, 5),
    new(Ideology.Trotskyism, SocialClass.Proletariat, ModeOfProduction.Communist,
      BureaucracyDoctrine.ParasiticCaste, true, 3),
    new(Ideology.CouncilCommunism, SocialClass.Proletariat, ModeOfProduction.Communist,
      BureaucracyDoctrine.StateCapitalism, true, 2),
    new(Ideology.Anarchism, SocialClass.Proletariat, ModeOfProduction.Communist,
      BureaucracyDoctrine.StateCapitalism, true, 2)
  }.ToDictionary(profile => profile.Ideology);

  #endregion

  #region Methods

  public static IdeologyProfile Profile(this Ideology ideology)
  {
    return Register.TryGetValue(ideology, out var profile)
      ? profile
      : throw new DomainException($"No profile is registered for {ideology}.");
  }

  /// <summary>The tendencies holding a given reading of the bureaucracy.</summary>
  public static IReadOnlyList<Ideology> Holding(BureaucracyDoctrine doctrine)
  {
    return
    [
      ..Register.Values.Where(profile => profile.BureaucracyDoctrine == doctrine).Select(profile => profile.Ideology)
    ];
  }

  #endregion
}
