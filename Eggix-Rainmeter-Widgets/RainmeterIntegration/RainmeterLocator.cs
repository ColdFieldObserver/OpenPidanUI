using Microsoft.Win32;
using System.IO;

namespace RainmeterIntegration;

/// <summary>
/// Locates the Rainmeter.exe installation so the host app can send refresh bangs.
/// </summary>
public static class RainmeterLocator
{
    public static string? FindRainmeterExe()
    {
        // 1. Registry: HKLM/HKCU\Software\Rainmeter -> InstallPath
        foreach (var baseKey in new[] { Registry.LocalMachine, Registry.CurrentUser })
        {
            using var key = baseKey.OpenSubKey(@"Software\Rainmeter");
            var path = key?.GetValue("InstallPath") as string;
            if (!string.IsNullOrEmpty(path))
            {
                var exe = Path.Combine(path, "Rainmeter.exe");
                if (File.Exists(exe))
                {
                    return exe;
                }
            }
        }

        // 2. Standard program files locations.
        foreach (var pf in new[]
                 {
                     Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                     Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                 })
        {
            if (string.IsNullOrEmpty(pf))
            {
                continue;
            }

            var exe = Path.Combine(pf, "Rainmeter", "Rainmeter.exe");
            if (File.Exists(exe))
            {
                return exe;
            }
        }

        // 3. PATH
        return FindOnPath("Rainmeter.exe");
    }

    private static string? FindOnPath(string fileName)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var full = Path.Combine(dir, fileName);
                if (File.Exists(full))
                {
                    return full;
                }
            }
            catch
            {
                // ignore invalid path segments
            }
        }
        return null;
    }
}
