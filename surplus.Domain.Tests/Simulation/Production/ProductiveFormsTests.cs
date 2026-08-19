using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Domain.Tests.Simulation.Production;

public class ProductiveFormsTests
{
  #region Tests

  [Fact]
  public void A_mode_of_production_bears_the_forms_history_gave_it()
  {
    // The machine is the technical basis adequate to capital, and no mode before
    // it had one.
    Assert.DoesNotContain(ProductiveForm.MachineIndustry, ModeOfProduction.Feudal.Forms());
    Assert.Contains(ProductiveForm.MachineIndustry, ModeOfProduction.Capitalist.Forms());

    // Cooperation is far older than capital: Egypt's irrigation works and the
    // corvée on the demesne are cooperation, and it outlives capital too.
    Assert.Contains(ProductiveForm.Cooperation, ModeOfProduction.Slave.Forms());
    Assert.Contains(ProductiveForm.Cooperation, ModeOfProduction.Communist.Forms());

    // The commune knows the tool and nothing beyond it.
    Assert.Equal([ProductiveForm.Handicraft], ModeOfProduction.PrimitiveCommunal.Forms());
  }

  [Fact]
  public void Every_form_of_the_labor_process_is_registered()
  {
    foreach (var form in Enum.GetValues<ProductiveForm>())
    {
      Assert.Equal(form, form.Profile().Form);
    }
  }

  [Fact]
  public void Handicraft_alone_rests_on_a_single_pair_of_hands()
  {
    Assert.False(ProductiveForm.Handicraft.Profile().SetsManyHandsInMotion);
    Assert.True(ProductiveForm.Cooperation.Profile().SetsManyHandsInMotion);
    Assert.True(ProductiveForm.Manufacture.Profile().SetsManyHandsInMotion);
    Assert.True(ProductiveForm.MachineIndustry.Profile().SetsManyHandsInMotion);
  }

  [Fact]
  public void Real_subsumption_begins_where_the_instrument_leaves_the_workers_hand()
  {
    // Manufacture divides the labourer but leaves them their tool: the
    // subsumption is still formal. The factory reverses the relation.
    Assert.False(ProductiveForm.Manufacture.Profile().RevolutionisesTheLaborProcess);
    Assert.True(ProductiveForm.MachineIndustry.Profile().RevolutionisesTheLaborProcess);
  }

  [Fact]
  public void The_factory_alone_makes_the_worker_serve_the_instrument()
  {
    Assert.True(ProductiveForm.Handicraft.Profile().WorkerWieldsTheInstrument);
    Assert.True(ProductiveForm.Cooperation.Profile().WorkerWieldsTheInstrument);
    Assert.True(ProductiveForm.Manufacture.Profile().WorkerWieldsTheInstrument);
    Assert.False(ProductiveForm.MachineIndustry.Profile().WorkerWieldsTheInstrument);

    Assert.Equal(
      "MachineIndustry (the worker is the machine's)", ProductiveForm.MachineIndustry.Profile().ToString()
    );
    Assert.Equal("Handicraft (the tool is the worker's)", ProductiveForm.Handicraft.Profile().ToString());
  }

  #endregion
}
