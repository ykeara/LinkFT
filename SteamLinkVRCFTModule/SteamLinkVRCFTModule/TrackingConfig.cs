using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;

namespace SteamLinkVRCFTModule
{
    public enum TrackingMode
    {
        Auto,
        On,
        Off
    }

    public class TrackingConfig
    {
        private const string ConfigFileName = "tracking_config.json";

        [JsonConverter(typeof(StringEnumConverter))]
        public TrackingMode EyeTracking { get; set; } = TrackingMode.Auto;

        [JsonConverter(typeof(StringEnumConverter))]
        public TrackingMode ExpressionTracking { get; set; } = TrackingMode.Auto;

        public static TrackingConfig Load(string directory, ILogger logger)
        {
            var path = Path.Combine(directory, ConfigFileName);
            try
            {
                if (File.Exists(path))
                {
                    var config = JsonConvert.DeserializeObject<TrackingConfig>(File.ReadAllText(path));
                    if (config != null)
                    {
                        return config;
                    }
                }
                else
                {
                    File.WriteAllText(path, JsonConvert.SerializeObject(new TrackingConfig(), Formatting.Indented));
                }
            }
            catch (System.Exception e)
            {
                logger?.LogWarning("Failed to load {0}, falling back to defaults. {1}", path, e.Message);
            }
            return new TrackingConfig();
        }
    }
}
