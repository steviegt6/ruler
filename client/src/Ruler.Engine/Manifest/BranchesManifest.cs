using Newtonsoft.Json;

namespace Ruler.Engine.Manifest
{
    /// <summary>
    ///     Represents a branches manifest file (<c>branches-manifest.json</c>).
    /// </summary>
    public sealed class BranchesManifest
    {
        [JsonProperty("default")]
        public string Default = "";

        [JsonProperty("branches")]
        public Dictionary<string, BranchesData> Branches = new();
    }
}