using System.Text;
using Surplus.Application;
using Surplus.Application.Scenarios;
using Surplus.Console;

// The game writes dashes and arrows; a Windows console left in its legacy code page drops them.
System.Console.OutputEncoding = Encoding.UTF8;

new Terminal(
  Session.Begin(EighteenThirtySix.Build()), FilePreferenceStore.InUserProfile(), System.Console.In, System.Console.Out
).Run();
