using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace CloudBackuper
{
    public class Config
    {
        [JsonProperty(Required = Required.Always)]
        public string Id { get; set; }
        public Config_Uploader Uploader { get; set; }
        public Config_JobRetrying JobRetrying { get; set; }
        public List<Config_Job> Jobs { get; set; }
        public string HostingURI { get; set; } = "http://localhost:3000";

        [JsonConverter(typeof(ConfigLogging_JsonConverter))]
        public Config_Logging Logging { get; set; }
    }

    public class Config_Uploader
    {
        public string Type { get; set; }

        // Метод позволят обойти проблему сериализации JObject в YML из-за ограничений DotNetYML
        // Взято отсюда: https://stackoverflow.com/questions/65475843/c-sharp-convert-ot-cast-expandoobject-to-specific-class-object
        public ExpandoObject Settings { get; set; }
    }

    public class Config_Logging
    {
        public LogLevel LogLevel;
        public WebServiceTarget WebService;
        public RetryingTargetWrapper RetryingWrapper;

        public static Config_Logging Defaults => new Config_Logging {
            LogLevel = LogLevel.Info
        };
    }

    public class Config_JobRetrying
    {
        public int MaxRetries { get; set; }
        public int WaitSeconds { get; set; }
    }

    public class Config_Masks
    {
        public string[] DirectoriesExcluded { get; set; }
        public bool MasksExclude { get; set; }
        public string[] Masks { get; set; }
    }

    public class Config_Job
    {
        [JsonProperty(Required = Required.Always)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CronSchedule { get; set; }
        public string CopyTo { get; set; }
        public string RunAfter { get; set; }
        public string Path { get; set; }
        public string Password { get; set; }
        public Config_Masks Masks { get; set; }

        public static Logger logger = LogManager.GetLogger("Validator");

        public void Validate()
        {
            bool hasCron = !string.IsNullOrEmpty(CronSchedule);
            bool hasAfter = !string.IsNullOrEmpty(RunAfter);

            if (!hasCron && !hasAfter)
                logger.Info($"Задача \"{Name}\" не имеет расписания и может быть запущена только вручную!");
        }

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

            if (data["WebService"] is JToken webService)
            {
                var parameters = webService["parameters"]?.ToObject<Dictionary<string,string>>() ?? new Dictionary<string, string>();
                webService.OfType<JProperty>().Where(x => x.Name == "parameters").ToList().ForEach(x => x.Remove());

                var value = webService.ToObject<WebServiceTarget>();
                foreach (var param in parameters)
                {
                    var layout = new NLog.Layouts.SimpleLayout(param.Value);
                    value.Parameters.Add(new MethodCallParameter(param.Key, layout));
                }
                output.WebService = value;
            }

            if (data["RetryingWrapper"] is JToken retryWrapper)
            {
                var value = retryWrapper.ToObject<RetryingTargetWrapper>();
                output.RetryingWrapper = value;
            }

            return output;
        }
    }
    #endregion Json Converters
}