namespace RainmeterIntegration;

/// <summary>
/// Composition root for the Rainmeter integration SDK. Host apps depend on the
/// interfaces and obtain default implementations here; they may substitute their
/// own (e.g. a mock <see cref="IRainmeterLocator"/> for tests) by constructing the
/// concrete types directly.
/// </summary>
public static class RainmeterServices
{
    /// <summary>Default locator: registry -&gt; Program Files -&gt; PATH.</summary>
    public static IRainmeterLocator CreateLocator() => new RegistryRainmeterLocator();

    /// <summary>Default controller driving Rainmeter via command-line bangs.</summary>
    public static IRainmeterController CreateController(IRainmeterLocator? locator = null)
        => new RainmeterController(locator ?? CreateLocator());

    /// <summary>Default avatar renderer/writer (PNG, circular crop, lock-safe write).</summary>
    public static IAvatarWriter CreateAvatarWriter() => new FileAvatarWriter();

    /// <summary>
    /// Builds a skin context for the given layout. The caller must supply its own
    /// <see cref="ISkinLayout"/> (e.g. <c>new SkinLayout("home.ini")</c>) so the
    /// library stays agnostic of any specific skin's file names.
    /// </summary>
    public static ISkinContext CreateSkinContext(ISkinLayout layout, string? skinDirectory = null)
        => new SkinContext(layout, skinDirectory);
}
