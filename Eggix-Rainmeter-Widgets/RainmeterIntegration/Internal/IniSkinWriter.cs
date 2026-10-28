using System.IO;

namespace RainmeterIntegration.Internal;

/// <summary>
/// Low-level INI reader/writer that preserves comments, blank lines and unrelated
/// sections. Implementation detail used by <see cref="FileIniStore"/>; not part of
/// the public SDK surface.
/// </summary>
internal static class IniSkinWriter
{
    public static string GetValue(string filePath, string section, string key, string defaultValue = "")
    {
        if (!File.Exists(filePath))
        {
            return defaultValue;
        }

        string? currentSection = null;
        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = line.Substring(1, line.Length - 2);
                continue;
            }

            if (!string.Equals(currentSection, section, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var idx = line.IndexOf('=');
            if (idx <= 0)
            {
                continue;
            }

            var k = line.Substring(0, idx).Trim();
            if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
            {
                return line.Substring(idx + 1).Trim();
            }
        }

        return defaultValue;
    }

    public static void SetValue(string filePath, string section, string key, string value)
    {
        var lines = File.Exists(filePath)
            ? new List<string>(File.ReadAllLines(filePath))
            : new List<string>();

        // Locate the target section header.
        int sectionIndex = -1;
        for (int i = 0; i < lines.Count; i++)
        {
            var trimmed = lines[i].Trim();
            if (trimmed.StartsWith('[') && trimmed.EndsWith(']') &&
                string.Equals(trimmed.Substring(1, trimmed.Length - 2), section, StringComparison.OrdinalIgnoreCase))
            {
                sectionIndex = i;
                break;
            }
        }

        // Section does not exist yet -> append it.
        if (sectionIndex == -1)
        {
            if (lines.Count > 0 && lines[^1].Trim().Length > 0)
            {
                lines.Add(string.Empty);
            }

            lines.Add($"[{section}]");
            lines.Add($"{key}={value}");
            File.WriteAllLines(filePath, lines);
            return;
        }

        // Find the key within the section (stop at the next section header).
        int keyIndex = -1;
        for (int i = sectionIndex + 1; i < lines.Count; i++)
        {
            var trimmed = lines[i].Trim();
            if (trimmed.StartsWith('['))
            {
                break;
            }

            var idx = trimmed.IndexOf('=');
            if (idx <= 0)
            {
                continue;
            }

            if (string.Equals(trimmed.Substring(0, idx).Trim(), key, StringComparison.OrdinalIgnoreCase))
            {
                keyIndex = i;
                break;
            }
        }

        if (keyIndex == -1)
        {
            lines.Insert(sectionIndex + 1, $"{key}={value}");
        }
        else
        {
            lines[keyIndex] = $"{key}={value}";
        }

        File.WriteAllLines(filePath, lines);
    }
}
