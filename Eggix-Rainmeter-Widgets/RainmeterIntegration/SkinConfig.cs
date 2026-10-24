using System.IO;

namespace RainmeterIntegration;

/// <summary>
/// Facade over a Rainmeter skin folder. The host program is expected to run
/// *inside* the skin folder (e.g. ...\Rainmeter\Skins\EggyUI-Air\Home), so all
/// paths are resolved relative to the running assembly's directory.
/// </summary>
public class SkinConfig
{
    public string SkinDirectory { get; }
    public string IniPath { get; }
    public string AvatarPath { get; }
    public string PresetsFolder { get; }

    public SkinConfig(string? skinDirectory = null)
    {
        SkinDirectory = skinDirectory
                        ?? FindSkinDirectory(AppContext.BaseDirectory)
                        ?? AppContext.BaseDirectory;

        IniPath = Path.Combine(SkinDirectory, "home.ini");
        AvatarPath = Path.Combine(SkinDirectory, "avatar.png");
        PresetsFolder = Path.Combine(SkinDirectory, "Avatar");
    }

    /// <summary>Reads the nickname from [MeterLaunch1Text] Text=.</summary>
    public string GetNickname() =>
        IniSkinWriter.GetValue(IniPath, "MeterLaunch1Text", "Text", "Administrator");

    /// <summary>Writes the nickname to [MeterLaunch1Text] Text=.</summary>
    public void SetNickname(string name) =>
        IniSkinWriter.SetValue(IniPath, "MeterLaunch1Text", "Text", name);

    /// <summary>
    /// Rainmeter config name relative to the Skins folder (e.g. "EggyUI-Air\Home").
    /// Returns null when it cannot be derived, in which case the caller should
    /// fall back to a full refresh.
    /// </summary>
    public string? ConfigName
    {
        get
        {
            var skinsRoot = FindSkinsRoot(SkinDirectory);
            if (skinsRoot is null)
            {
                return null;
            }

            var relative = GetRelativePath(skinsRoot, SkinDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return relative.Replace(Path.DirectorySeparatorChar, '\\')
                           .Replace(Path.AltDirectorySeparatorChar, '\\');
        }
    }

    private static string? FindSkinDirectory(string baseDir)
    {
        if (File.Exists(Path.Combine(baseDir, "home.ini")))
        {
            return baseDir;
        }

        var parent = Directory.GetParent(baseDir)?.FullName;
        if (parent is not null && File.Exists(Path.Combine(parent, "home.ini")))
        {
            return parent;
        }

        return null;
    }

    private static string? FindSkinsRoot(string dir)
    {
        var current = dir;
        while (!string.IsNullOrEmpty(current))
        {
            if (string.Equals(Path.GetFileName(current), "Skins", StringComparison.OrdinalIgnoreCase))
            {
                return current;
            }

            var parent = Directory.GetParent(current)?.FullName;
            if (parent is null || parent == current)
            {
                break;
            }

            current = parent;
        }
        return null;
    }

    private static string GetRelativePath(string relativeTo, string path)
    {
        var baseUri = new Uri(relativeTo.EndsWith(Path.DirectorySeparatorChar) || relativeTo.EndsWith(Path.AltDirectorySeparatorChar)
            ? relativeTo
            : relativeTo + Path.DirectorySeparatorChar);
        var targetUri = new Uri(path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar);
        return Uri.UnescapeDataString(baseUri.MakeRelativeUri(targetUri).ToString())
            .Replace('/', Path.DirectorySeparatorChar);
    }
}
