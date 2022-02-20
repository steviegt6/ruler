using Newtonsoft.Json;

namespace Ruler.Engine.Manifest
{
    /// <summary>
    ///     Represents an update manifest.
    /// </summary>
    /// <remarks>
    ///     This is a JSON-oriented class which should appear alongside an archive/single-file download.
    /// </remarks>
    public sealed class ManifestFile
    {
        /// <summary>
        ///     An update's title.
        /// </summary>
        [JsonProperty("title")]
        public string Title = "";

        /// <summary>
        ///     A brief description of the update.
        /// </summary>
        [JsonProperty("desc")]
        public string Description = "";

        // TODO: Arguably redundant, lol.
        /// <summary>
        ///     An SHA256 checksum to verify this archive/single-file program is legitimate.
        /// </summary>
        [JsonProperty("checksum")]
        public string Checksum = "";
    }
}