using Newtonsoft.Json;

namespace Ruler.Engine.Manifest
{
    /// <summary>
    ///     Represents a versions manifest file (<c>versions-manifest.json</c>).
    /// </summary>
    public sealed class VersionsManifest
    {
        [JsonProperty("latest")]
        public string Latest = "";

        [JsonProperty("versions")]
        public Dictionary<string, VersionsData> Versions = new();
    }
}