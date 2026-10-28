using System.IO;

namespace RainmeterIntegration;

/// <summary>
/// Default <see cref="ISkinContext"/>. Resolves a skin directory (searching upward
/// for the configured .ini), derives the Rainmeter config name, and exposes a typed
/// <see cref="IIniStore"/> bound to that .ini. Does not know about avatars, presets
/// or any skin-specific key — those are the host app's concern.
/// </summary>
public sealed class SkinContext : ISkinContext
{
    private readonly ISkinLayout _layout;
    private IIniStore? _settings;

    public string SkinDirectory { get; }
    public string IniPath { get; }
    public string? ConfigName { get; }

    public IIniStore Settings => _settings ??= new FileIniStore(IniPath);

    public SkinContext(ISkinLayout layout, string? skinDirectory = null)
    {
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));

        SkinDirectory = skinDirectory
                        ?? FindSkinDirectory(AppContext.BaseDirectory, _layout.IniFileName)
                        ?? AppContext.BaseDirectory;

        IniPath = Path.Combine(SkinDirectory, _layout.IniFileName);
        ConfigName = ResolveConfigName(SkinDirectory);
    }

    private static string? FindSkinDirectory(string baseDir, string iniFileName)
    {
        if (File.Exists(Path.Combine(baseDir, iniFileName)))
        {
            return baseDir;
        }

        var parent = Directory.GetParent(baseDir)?.FullName;
        if (parent is not null && File.Exists(Path.Combine(parent, iniFileName)))
        {
            return parent;
        }

        return null;
    }

    private static string? ResolveConfigName(string dir)
    {
        var skinsRoot = FindSkinsRoot(dir);
        if (skinsRoot is null)
        {
            return null;
        }

        var relative = GetRelativePath(skinsRoot, dir)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return relative.Replace(Path.DirectorySeparatorChar, '\\')
                       .Replace(Path.AltDirectorySeparatorChar, '\\');
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
