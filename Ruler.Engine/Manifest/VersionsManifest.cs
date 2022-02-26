using Newtonsoft.Json;

namespace Ruler.Engine.Manifest
{
    /// <summary>
    ///     Represents a versions manifest file (<c>versions-manifest.json</c>).
    /// </summary>
    public class VersionsManifest
    {
        public string Latest { get; set; } = null!;

        public Dictionary<string, VersionsData> Versions { get; set; } = new();
    }
}