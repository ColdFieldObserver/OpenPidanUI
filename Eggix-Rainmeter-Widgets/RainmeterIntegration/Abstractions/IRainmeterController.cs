namespace RainmeterIntegration;

/// <summary>
/// Sends bangs to a running Rainmeter instance. This is the supported
/// "external application" control surface of the Rainmeter SDK
/// (docs.rainmeter.net/manual/bangs - "Command Line Arguments").
/// </summary>
public interface IRainmeterController
{
    /// <summary>True when a Rainmeter installation can be located.</summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Refreshes the given skin config (e.g. "EggyUI-Air\Home"). When
    /// <paramref name="configName"/> is null/empty, falls back to !RefreshApp
    /// (refreshes every loaded skin).
    /// </summary>
    void Refresh(string? configName = null);
}
