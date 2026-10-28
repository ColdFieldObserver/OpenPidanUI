namespace RainmeterIntegration;

/// <summary>
/// A resolved view of a Rainmeter skin: its directory, the absolute path of its
/// primary .ini, its config name (relative to the Skins root), and a typed store
/// for reading/writing that .ini.
///
/// Avatar/preset paths are intentionally absent — those are host-application
/// conventions, derived from <see cref="SkinDirectory"/> by the caller as needed.
/// This keeps the library generic across skins rather than tied to one.
/// </summary>
public interface ISkinContext
{
    /// <summary>Directory that contains the skin .ini (the skin root).</summary>
    string SkinDirectory { get; }

    /// <summary>Absolute path to the skin's primary .ini file.</summary>
    string IniPath { get; }

    /// <summary>
    /// Rainmeter config name relative to the Skins folder (e.g. "EggyUI-Air\Home"),
    /// or null when it cannot be derived (caller should then do a full refresh).
    /// </summary>
    string? ConfigName { get; }

    /// <summary>Typed access to the skin .ini; read/write any key, e.g. a nickname.</summary>
    IIniStore Settings { get; }
}
