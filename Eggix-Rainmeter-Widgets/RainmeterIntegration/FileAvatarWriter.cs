using RainmeterIntegration.Internal;

namespace RainmeterIntegration;

/// <summary>
/// <see cref="IAvatarWriter"/> that renders to memory and writes to disk with the
/// temp-file + retry strategy that survives transient file locks.
/// </summary>
public sealed class FileAvatarWriter : IAvatarWriter
{
    public byte[] RenderAvatar(string sourcePath, int size = 256, bool circular = true)
        => AvatarWriter.RenderAvatar(sourcePath, size, circular);

    public void WriteAvatar(string sourcePath, string destinationPath, int size = 256, bool circular = true)
        => AvatarWriter.WriteAvatar(sourcePath, destinationPath, size, circular);
}
