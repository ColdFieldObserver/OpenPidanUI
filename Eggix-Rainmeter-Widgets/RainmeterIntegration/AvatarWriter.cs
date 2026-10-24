using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Threading;

namespace RainmeterIntegration;

/// <summary>
/// Writes avatar images. Any source image is cover-fitted into a square. When
/// <paramref name="circular"/> is true (default) the pixels outside the inscribed
/// circle are made transparent, matching the circular avatar style produced by the
/// AvatarMaker tool.
///
/// The processed PNG is rendered to memory, written to a temp file on the same
/// volume, then atomically replaces the destination. The replace is retried for a
/// short while because the Rainmeter skin (or another viewer) may briefly hold
/// the target file open. This avoids the "file is in use" failure.
/// </summary>
public static class AvatarWriter
{
    public static void WriteCircular(string sourcePath, string destinationPath, int size = 256, bool circular = true)
    {
        byte[] pngBytes = RenderAvatar(sourcePath, size, circular);

        string destination = Path.GetFullPath(destinationPath);
        string dir = Path.GetDirectoryName(destination) ?? ".";
        string tempPath = Path.Combine(dir,
            string.Concat(Path.GetFileNameWithoutExtension(destination),
            ".", Guid.NewGuid().ToString("N").AsSpan(0, 8), ".tmp.png"));

        File.WriteAllBytes(tempPath, pngBytes);

        try
        {
            ReplaceWithRetry(tempPath, destination);
        }
        finally
        {
            // Best-effort cleanup of the temp file if something went wrong.
            try { if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch { /* ignore */ }
        }
    }

    /// <summary>
    /// Renders the cropped avatar to a PNG byte array without touching disk, so
    /// the UI can preview the exact result (circular or square) before saving.
    /// </summary>
    public static byte[] RenderAvatar(string sourcePath, int size = 256, bool circular = true)
    {
        using var image = Image.Load<Rgba32>(sourcePath);
        image.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(size, size),
            Mode = ResizeMode.Crop
        }));

        if (circular)
        {
            // Apply a circular alpha mask: anything outside the inscribed circle
            // becomes transparent.
            image.ProcessPixelRows(accessor =>
            {
                double center = (size - 1) / 2.0;
                double radiusSquared = (size / 2.0) * (size / 2.0);
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        double dx = x - center;
                        double dy = y - center;
                        if (dx * dx + dy * dy > radiusSquared)
                        {
                            row[x].A = 0;
                        }
                    }
                }
            });
        }

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }

    private static void ReplaceWithRetry(string tempPath, string destination)
    {
        const int maxAttempts = 25;   // ~2.5s total
        const int delayMs = 100;
        Exception? lastError = null;

        for (int i = 0; i < maxAttempts; i++)
        {
            // A locked destination surfaces as IOException (MoveFileEx failure)
            // or UnauthorizedAccessException (Win32 ERROR_ACCESS_DENIED /
            // ERROR_SHARING_VIOLATION) on Windows. Catch both and retry.
            try
            {
                // Atomic replace on the same volume (MoveFileEx). Fails if the
                // target is currently locked without delete sharing.
                File.Move(tempPath, destination, overwrite: true);
                return;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                lastError = ex;
            }

            try
            {
                // Fallback: copy over the contents in place.
                File.Copy(tempPath, destination, overwrite: true);
                return;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                lastError = ex;
            }

            Thread.Sleep(delayMs);
        }

        throw new IOException(
            $"无法写入头像文件（被其它进程占用）：{destination}", lastError);
    }
}
