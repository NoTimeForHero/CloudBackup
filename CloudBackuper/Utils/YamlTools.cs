using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace CloudBackuper.Utils
{
    public class YamlTools
    {
        private static readonly Serializer ymlSerializer = new Serializer();
        private static readonly IDeserializer ymlDeserializer = new DeserializerBuilder().Build();
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static T DeserializeFile<T>(string folder, string configName, string debugFile = null)
        {
            var path = Path.Combine(folder, configName);
            // TODO: StringReader(json)
            using (var reader = new StreamReader(path))
            {
                var yml = ymlDeserializer.Deserialize(reader);
                var json = JsonConvert.SerializeObject(yml, jsonSettings);
                if (debugFile != null)
                {
                    var debugPath = Path.Combine(folder, debugFile);
                    File.WriteAllText(debugPath, json);
                }
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public static T Deserialize<T>(string ymlString) => Deserialize<T>(ymlString, out string _);

        public static T Deserialize<T>(string ymlString, out string json)
        {
            using (var reader = new StringReader(ymlString))
            {
                var yml = ymlDeserializer.Deserialize(reader);
                json = JsonConvert.SerializeObject(yml, jsonSettings);
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public static string Serialize<T>(T value, int startIndent = 0) => Serialize(value, out string _, startIndent);

        public static string Serialize<T>(T value, out string json, int startIndent = 0)
        {
            json = JsonConvert.SerializeObject(value, jsonSettings);
            var expando = JsonConvert.DeserializeObject<ExpandoObject>(json);
            var yml = ymlSerializer.Serialize(expando);
            if (startIndent > 0)
            {
                yml = yml.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => new string(' ', startIndent) + x)
                    .Join(Environment.NewLine);
            }
            return yml;
        }

    }
}
