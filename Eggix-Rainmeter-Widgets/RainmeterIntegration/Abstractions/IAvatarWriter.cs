namespace RainmeterIntegration;

/// <summary>
/// Renders and writes avatar / cover images for a skin. The source image is
/// cover-fitted into a square; pixels outside an inscribed circle can be made
/// transparent (circular crop). Output is always PNG.
/// </summary>
public interface IAvatarWriter
{
    /// <summary>
    /// Renders the cropped image to a PNG byte array without touching disk, so a
    /// UI can preview the exact result before saving.
    /// </summary>
    byte[] RenderAvatar(string sourcePath, int size = 256, bool circular = true);

    /// <summary>
    /// Writes the cropped image to <paramref name="destinationPath"/>. The write
    /// uses a temp file + atomic replace with short retries, so a transient lock
    /// held by the skin (or another viewer) does not fail the operation.
    /// </summary>
    void WriteAvatar(string sourcePath, string destinationPath, int size = 256, bool circular = true);
}
