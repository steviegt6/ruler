using System;
using System.Collections.Generic;
using System.IO;

namespace Ruler.Engine.Platform
{
    /// <summary>
    ///     Base provider for *Nix platforms (MacOS/OSX, Linux, etc.).
    /// </summary>
    public abstract class NixLocationProvider : DesktopLocationProvider
    {
        public override IEnumerable<string> Locations
        {
            get
            {
                yield return GetNixPath();
            }
        }

        /// <summary>
        ///     Returns the XDG data path, otherwise a user's <c>/.local/share/</c> folder.
        /// </summary>
        /// <returns></returns>
        protected static string GetNixPath()
        {
            string? xdgPath = Environment.GetEnvironmentVariable("XDG_DATA_HOME");

            if (string.IsNullOrEmpty(xdgPath))
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    ".local",
                    "share"
                );

            return xdgPath;
        }
    }
}