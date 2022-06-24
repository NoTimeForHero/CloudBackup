using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace CloudBackuper.Utils
{
    internal class YamlTools
    {
        public static T Deserialize<T>(string folder, string configName, string debugFile = null)
        {
            var path = Path.Combine(folder, configName);
            using (var reader = new StreamReader(path))
            {
                var deserializer = new DeserializerBuilder().Build();
                var yml = deserializer.Deserialize(reader);
                var json = JsonConvert.SerializeObject(yml, Formatting.Indented);
                if (debugFile != null)
                {
                    var debugPath = Path.Combine(folder, debugFile);
                    File.WriteAllText(debugPath, json);
                }
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

    }
}
