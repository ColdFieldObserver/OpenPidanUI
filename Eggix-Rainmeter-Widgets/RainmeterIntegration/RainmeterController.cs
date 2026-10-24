using System.Diagnostics;

namespace RainmeterIntegration;

/// <summary>
/// Sends bangs to a running Rainmeter instance via the Rainmeter.exe command line.
/// This is the supported "external application" control surface of the Rainmeter SDK
/// (see docs.rainmeter.net/manual/bangs - "Command Line Arguments").
/// </summary>
public class RainmeterController
{
    /// <summary>True when Rainmeter.exe can be located on this machine.</summary>
    public bool IsAvailable => RainmeterLocator.FindRainmeterExe() != null;

    /// <summary>
    /// Refreshes the given skin config (e.g. "EggyUI-Air\Home"). When
    /// <paramref name="configName"/> is null/empty, falls back to !RefreshApp
    /// (refreshes every loaded skin).
    /// </summary>
    public void Refresh(string? configName = null)
    {
        var exe = RainmeterLocator.FindRainmeterExe();
        if (exe is null)
        {
            return;
        }

        var bang = string.IsNullOrWhiteSpace(configName)
            ? "!RefreshApp"
            : $"!Refresh \"{configName}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = bang,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        using var process = Process.Start(startInfo);
        // Fire-and-forget: Rainmeter processes the bang asynchronously.
    }
}
