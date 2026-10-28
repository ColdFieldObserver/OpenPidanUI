namespace RainmeterIntegration;

/// <summary>
/// Locates a Rainmeter installation on the current machine. Implementations are
/// swappable (registry, fixed path, mock for tests) so host apps never depend on
/// a concrete lookup strategy.
/// </summary>
public interface IRainmeterLocator
{
    /// <summary>Full path to Rainmeter.exe, or null when not found.</summary>
    string? FindExecutable();
}
