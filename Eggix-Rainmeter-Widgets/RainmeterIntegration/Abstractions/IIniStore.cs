namespace RainmeterIntegration;

/// <summary>
/// Reads and writes a single Rainmeter skin .ini file. Implementations must
/// preserve sections, comments, blank lines and unrelated keys so that editing a
/// single value never disturbs the rest of the skin definition.
/// </summary>
public interface IIniStore
{
    /// <summary>Returns the value of key under section, or defaultValue.</summary>
    string GetValue(string section, string key, string defaultValue = "");

    /// <summary>Sets key under section (creating the section/key if absent).</summary>
    void SetValue(string section, string key, string value);
}
