using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Fluent;

namespace CloudBackuper
{
    public class Config
    {
        public Config_S3 Cloud { get; set; }
        public List<Config_Job> Jobs { get; set; }

        [JsonConverter(typeof(ConfigLogging_JsonConverter))]
        public Config_Logging Logging { get; set; }
    }

    public class Config_Logging
    {
        public LogLevel LogLevel { get; set; }

        public static Config_Logging Defaults => new Config_Logging {
            LogLevel = LogLevel.Info
        };
    }


    public class Config_S3
    {
        public string Provider { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Container { get; set; }
        public bool ForcePathStyle { get; set; }
    }

    public class Config_Masks
    {
        public string[] DirectoriesExcluded { get; set; }
        public bool MasksExclude { get; set; }
        public string[] Masks { get; set; }
    }

    public class Config_Job
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CronSchedule { get; set; }
        public string Path { get; set; }
        public Config_Masks Masks { get; set; }

        public override string ToString()
        {
            return $"Job[Name='{Name}',Path='{Path}',CronSchedule='{CronSchedule}']";
        }
    }

    #region Json Converters
    public class ConfigLogging_JsonConverter : JsonConverter<Config_Logging>
    {
        public override void WriteJson(JsonWriter writer, Config_Logging value, JsonSerializer serializer)
            => throw new NotImplementedException();

        public override Config_Logging ReadJson(JsonReader reader, Type objectType, Config_Logging existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Config_Logging output = Config_Logging.Defaults;

            var data = JObject.Load(reader);

            if (data["Level"] is JToken logLevel)
            {
                var value = logLevel.Value<string>();
                output.LogLevel = LogLevel.FromString(value);
            }

            return output;
        }
    }
    #endregion Json Converters
}