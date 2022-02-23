using Newtonsoft.Json;

namespace Ruler.IRule.Configuration
{
    /// <summary>
    ///     Configuration data for Ruler.IRule.
    /// </summary>
    public class RulerConfig
    {
        [JsonProperty("launchDelay", DefaultValueHandling = DefaultValueHandling.Populate)]
        public int LaunchDelay = 10000;

        [JsonProperty("reviewUpdates", DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool ReviewUpdates = true;

        [JsonProperty("branch", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Branch = Program.DefaultBranch;
    }
}