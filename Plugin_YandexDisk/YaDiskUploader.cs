using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CloudBackuper.Plugins;
using Newtonsoft.Json.Linq;

namespace Plugin_YandexDisk
{
    internal class YaDiskUploader : IUploader
    {
        private Settings settings;
        private WebClient webClient;

        public Task Initialize(object input)
        {
            if (!(input is JObject jVal)) throw new ApplicationException($"Параметр не является JObject!");
            settings = jVal.ToObject<Settings>();
            if (settings == null) throw new ApplicationException($"Не удалось десериализовать JSON в {nameof(Settings)} конфиг!");
            return Task.CompletedTask;
        }

        public Task Connect()
        {
            webClient = new WebClient(settings.OAuthToken);
            return Task.CompletedTask;
        }

        public async Task UploadFile(string path, string remote, Action<UploaderProgress> callback = null)
        {
            var file = new FileInfo(remote);

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var uploadPath = settings.UploadDir + "/" + file.Path;
                var uploadLink = await webClient.GetUploadLink(uploadPath + "/" + file.SafeName, true);
                await webClient.UploadFile(uploadLink, fs, callback);
                if (!file.IsNameSafe) await webClient.RenameFile(uploadPath, file.SafeName, file.Name);
            }
        }

        public Task Disconnect()
        {
            webClient.Dispose();
            return Task.CompletedTask;
        }
    }

    internal class Settings
    {
        public string OAuthToken { get; set; }
        public string UploadDir { get; set; }
    }
}
