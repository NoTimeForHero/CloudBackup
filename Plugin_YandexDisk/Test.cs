using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudBackuper.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Plugin_YandexDisk
{
    internal class Test
    {
        public static async Task Main()
        {
            var config = new LoggingConfiguration();
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, new ConsoleTarget("logconsole"));
            LogManager.Configuration = config;

            var token = Environment.GetEnvironmentVariable("YANDEX_DISK_TOKEN", EnvironmentVariableTarget.User);
            //token = "test";
            var uploadDir = "/test1/test2/test3";
            dynamic settings = JsonConvert.SerializeObject(new Settings { OAuthToken = token, UploadDir = uploadDir });
            settings = JsonConvert.DeserializeObject<JObject>(settings);

            var uploader = new YaDiskUploader();
            await uploader.Initialize(settings);
            await uploader.Connect();

            Console.WriteLine("Begin uploading...");
            await uploader.UploadFile(@"C:\Test\Small.zip", "folder1/Example.zip", OnProgress);
            Console.WriteLine("End uploading...");

            Console.ReadLine();
        }

        private static void OnProgress(UploaderProgress state)
        {
            Console.SetCursorPosition(0, 0);
            var percent = (float)state.current / state.total * 100;
            Console.WriteLine($"Downloaded {state.current} bytes from {state.total} ({percent:F2}%)");
        }
    }
}
