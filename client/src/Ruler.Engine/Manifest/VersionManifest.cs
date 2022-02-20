using Newtonsoft.Json;

namespace Ruler.Engine.Manifest
{
    /// <summary>
    ///     Represents a <c>manifest.json</c>.
    /// </summary>
    public class VersionManifest
    {
        [JsonProperty("name")]
        public string Name = "";
        
        [JsonProperty("desc")]
        public string Description = "";
    }
}