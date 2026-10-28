namespace RainmeterIntegration;

/// <summary>
/// Minimal <see cref="ISkinLayout"/>: just the skin's primary .ini file name.
/// Host apps supply their own file name (e.g. "home.ini"); the library stays
/// agnostic about avatar/preset naming and any other skin-specific details.
/// </summary>
public sealed class SkinLayout : ISkinLayout
{
    public string IniFileName { get; }

    public SkinLayout(string iniFileName)
    {
        IniFileName = iniFileName ?? throw new ArgumentNullException(nameof(iniFileName));
    }
}
