using Newtonsoft.Json;

namespace Ruler.Engine.Manifest
{
    /// <summary>
    ///     Represents an object stored in the <c>versions</c> field of a <c>versions-manifest.json</c> file.
    /// </summary>
    public class VersionsData
    {
        [JsonProperty("name")]
        public string Name = "";
    }
}