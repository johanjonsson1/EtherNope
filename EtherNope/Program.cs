using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;


[assembly: SupportedOSPlatform("windows")]
namespace EtherNope;

public class Program
{
    static async Task Main(string[] args)
    {
        ILogger logger = Log.CreateEventLogLogger();

        if (args.Length < 2)
        {
            logger.LogInformation("Usage: <exe> <disable|enable> <AdapterNameMatch> <DelayInMinutes> <MessageBoxText>");
            return;
        }

        var shouldDisable = args[0].Equals("disable", StringComparison.InvariantCultureIgnoreCase);
        var targetName = args[1];

        if (shouldDisable)
        {
            await ShowMessageBoxAndDelay(args, targetName);
        }

        var adapterManager = new NetworkAdapterManager(logger);
        adapterManager.FindAndChangeAdapterState(targetName, shouldDisable);
    }

    private static async Task ShowMessageBoxAndDelay(string[] args, string targetName)
    {
        var delayMinutes = args.Length >= 3 && int.TryParse(args[2], out var dm) ? dm : 5;
        var messageBoxText = args.Length >= 4 ? args[3] : $"The network adapter '{targetName}' will be disabled in {delayMinutes} minutes.";
        var backgroundThread = new Thread(() =>
        {
            Interop.MessageBoxW(IntPtr.Zero, messageBoxText, nameof(EtherNope), 0);
        });
        backgroundThread.SetApartmentState(ApartmentState.STA);
        backgroundThread.IsBackground = true;
        backgroundThread.Start();

        // delay disabling to prepare the user
        await Task.Delay(TimeSpan.FromMinutes(delayMinutes));
    }
}
