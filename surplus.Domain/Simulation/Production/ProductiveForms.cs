using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// The register of what each form of the labour process is and which modes of
/// production have borne it. Kept as one table for the same reason as
/// <see cref="SocialClasses" />: these are historical facts, not settings.
/// </summary>
public static class ProductiveForms
{
  #region Fields

  private static readonly Dictionary<ProductiveForm, ProductiveFormProfile> Register = new ProductiveFormProfile[]
  {
    // The weaver at their own loom: one pair of hands, and the tool obeys them.
    // It outlives every mode that has yet existed, always as the older form.
    new(ProductiveForm.Handicraft, false, true,
      ModeOfProduction.PrimitiveCommunal, ModeOfProduction.Slave, ModeOfProduction.Feudal,
      ModeOfProduction.Capitalist),

    // Many hands at one work. Older than capital — the irrigation works of Egypt
    // and the corvée on the demesne are cooperation — and it survives it.
    new(ProductiveForm.Cooperation, true, true,
      ModeOfProduction.Slave, ModeOfProduction.Feudal, ModeOfProduction.Capitalist,
      ModeOfProduction.Socialist, ModeOfProduction.Communist),

    // Manufacture: the labourer is divided along with the work, and each becomes
    // the fragment of a labourer. The tool is still theirs, so the subsumption
    // remains formal — this is the transition, not yet the break.
    new(ProductiveForm.Manufacture, true, true, ModeOfProduction.Feudal, ModeOfProduction.Capitalist),

    // Modern industry: the instrument passes to the machine and the worker is
    // left to attend it. This is the technical basis adequate to capital, and
    // the one the modes after it inherit rather than invent.
    new(ProductiveForm.MachineIndustry, true, false,
      ModeOfProduction.Capitalist, ModeOfProduction.Socialist, ModeOfProduction.Communist)
  }.ToDictionary(profile => profile.Form);

  #endregion

  #region Methods

  /// <summary>What this form of the labour process is.</summary>
  public static ProductiveFormProfile Profile(this ProductiveForm form)
  {
    return Register.TryGetValue(form, out var profile)
      ? profile
      : throw new DomainException($"No profile is registered for {form}.");
  }

  /// <summary>The forms of the labour process a mode of production has borne.</summary>
  public static IReadOnlyList<ProductiveForm> Forms(this ModeOfProduction mode)
  {
    return
    [
      ..Register.Values.Where(profile => profile.IsNativeTo(mode)).Select(profile => profile.Form)
    ];
  }

  #endregion
}
