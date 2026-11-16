using Surplus.Application;
using Surplus.Application.Scenarios;
using Surplus.Console;

new Terminal(
  Session.Begin(EighteenThirtySix.Build()), FilePreferenceStore.InUserProfile(), System.Console.In, System.Console.Out
).Run();
