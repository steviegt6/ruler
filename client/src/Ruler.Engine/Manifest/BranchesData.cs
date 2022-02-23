using Newtonsoft.Json;

namespace Ruler.Engine.Manifest
{
    /// <summary>
    ///     Represents an object stored in the <c>branches</c> field of a <c>branches-manifest.json</c> file.
    /// </summary>
    public class BranchesData
    {
        [JsonProperty("name")]
        public string Name = "";
    }
}