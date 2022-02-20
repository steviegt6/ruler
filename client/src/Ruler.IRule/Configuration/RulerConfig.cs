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

        [JsonIgnore]
        public readonly IConfigItem<int> LaunchDelayItem;

        [JsonIgnore]
        public readonly IConfigItem<bool> ReviewUpdatesItem;
        
        public RulerConfig()
        {
            LaunchDelayItem = new ConfigItem<int>(
                () => LaunchDelay,
                val => LaunchDelay = val,
                "Could not validate input, must be a whole number (ex.: 1000, 2394)."
            );

            ReviewUpdatesItem = new ConfigItem<bool>(
                () => ReviewUpdates,
                val => ReviewUpdates = val,
                "Could not validate input, must be true/false."
            );
        }
    }
}