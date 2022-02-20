using System;
using System.Collections.Generic;

namespace Ruler.Engine.Platform
{
    public class WindowsLocationProvider : DesktopLocationProvider
    {
        public override IEnumerable<string> Locations
        {
            get
            {
                yield return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
        }
    }
}