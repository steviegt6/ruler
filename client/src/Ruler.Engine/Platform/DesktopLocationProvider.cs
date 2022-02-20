using System;
using Ruler.Engine.Location;

namespace Ruler.Engine.Platform
{
    /// <summary>
    ///     Provides locations for different desktop environments.
    /// </summary>
    public abstract class DesktopLocationProvider : UserInputLocationProvider
    {
        /// <summary>
        ///     Instantiates a new <see cref="DesktopLocationProvider"/> that corresponds to the current operating system.
        /// </summary>
        /// <returns>A <see cref="DesktopLocationProvider"/> instance that corresponds to the user's operating system.</returns>
        public static ILocationProvider GetDesktopProvider()
        {
            if (OperatingSystem.IsWindows())
                return new WindowsLocationProvider();

            if (OperatingSystem.IsMacOS())
                return new MacLocationProvider();

            if (OperatingSystem.IsLinux())
                return new LinuxLocationProvider();

            throw new PlatformNotSupportedException("Unsupported platform, could not retrieve a desktop provider.");
        }
    }
}