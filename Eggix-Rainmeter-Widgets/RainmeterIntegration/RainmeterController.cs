using System.Diagnostics;

namespace RainmeterIntegration;

/// <summary>
/// Default <see cref="IRainmeterController"/>: drives a running Rainmeter instance
/// through the Rainmeter.exe command line (bangs). Depends on an
/// <see cref="IRainmeterLocator"/> so the lookup strategy is swappable.
/// </summary>
public sealed class RainmeterController : IRainmeterController
{
    private readonly IRainmeterLocator _locator;

    public RainmeterController(IRainmeterLocator locator)
    {
        _locator = locator ?? throw new ArgumentNullException(nameof(locator));
    }

    public bool IsAvailable => _locator.FindExecutable() != null;

    public void Refresh(string? configName = null)
    {
        var exe = _locator.FindExecutable();
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
