namespace RainmeterIntegration;

/// <summary>
/// Identifies which .ini file is the skin's primary configuration file. This is the
/// only skin-specific fact the generic library needs in order to open and edit the
/// right file; avatar file name, presets folder and nickname key are all owned by
/// the host application, not by this library.
/// </summary>
public interface ISkinLayout
{
    /// <summary>Skin definition file name, e.g. "home.ini".</summary>
    string IniFileName { get; }
}
