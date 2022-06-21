using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudBackuper.Plugins;
using Newtonsoft.Json.Linq;
using YandexDisk.Client;
using YandexDisk.Client.Http;

namespace Plugin_YandexDisk
{
    internal class YaDiskUploader : IUploader
    {
        private Settings settings;
        private IDiskApi diskApi;

        public Task Initialize(object input)
        {
            if (!(input is JObject jVal)) throw new ApplicationException($"Параметр не является JObject!");
            settings = jVal.ToObject<Settings>();
            if (settings == null) throw new ApplicationException($"Не удалось десериализовать JSON в {nameof(Settings)} конфиг!");
            return Task.CompletedTask;
        }

        public Task Connect()
        {
            diskApi = new DiskHttpApi(settings.OAuthToken);
            return Task.CompletedTask;
        }

        public async Task UploadFile(string path, string destName, Action<UploaderProgress> callback = null)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var uploadPath = settings.UploadDir + "/" + destName;
                var uploadLink = await diskApi.Files.GetUploadLinkAsync(uploadPath, true);
                // TODO: Обновление прогресса загрузки?
                await diskApi.Files.UploadAsync(uploadLink, fs);
            }
        }

        public Task Disconnect()
        {
            diskApi.Dispose();
            return Task.CompletedTask;
        }
    }

    internal class Settings
    {
        public string OAuthToken { get; set; }
        public string UploadDir { get; set; }
    }
}
