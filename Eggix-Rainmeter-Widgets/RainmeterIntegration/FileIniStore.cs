using RainmeterIntegration.Internal;

namespace RainmeterIntegration;

/// <summary>
/// <see cref="IIniStore"/> backed by a single on-disk Rainmeter skin .ini file.
/// Delegates to the structure-preserving low-level writer.
/// </summary>
public sealed class FileIniStore : IIniStore
{
    private readonly string _filePath;

    public FileIniStore(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public string GetValue(string section, string key, string defaultValue = "")
        => IniSkinWriter.GetValue(_filePath, section, key, defaultValue);

    public void SetValue(string section, string key, string value)
        => IniSkinWriter.SetValue(_filePath, section, key, value);
}
