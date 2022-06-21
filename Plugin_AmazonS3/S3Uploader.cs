using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Newtonsoft.Json.Linq;
using NLog;

namespace CloudBackuper.Plugins
{
    class S3Uploader : IUploader
    {
        protected AmazonS3Client client;
        protected S3Bucket bucket;
        protected TransferUtility transferUtility;
        protected Settings settings;

        protected static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Task Initialize(object input)
        {
            if (!(input is JObject jVal)) throw new ApplicationException($"Параметр не является JObject!");
            settings = jVal.ToObject<Settings>();
            if (settings == null) throw new ApplicationException($"Не удалось десериализовать JSON в {nameof(Settings)} конфиг!");
            return Task.CompletedTask;
        }

        public async Task Connect()
        {
            AmazonS3Config config = new AmazonS3Config();
            config.ServiceURL = settings.Provider;
            config.ForcePathStyle = settings.ForcePathStyle;
            if (settings.Proxy != null)
            {
                logger.Info($"Используем прокси: {settings.Proxy.Host}:{settings.Proxy.Port}");
                config.ProxyHost = settings.Proxy.Host;
                config.ProxyPort = settings.Proxy.Port;
                if (settings.Proxy.Login != null && settings.Proxy.Password != null)
                {
                    config.ProxyCredentials = new NetworkCredential(settings.Proxy.Login, settings.Proxy.Password);
                    var maskedPass = new string('#', settings.Proxy.Password.Length);
                    logger.Info($"С логином {settings.Proxy.Login} и паролем {maskedPass}");
                }
            }

            client = new AmazonS3Client(settings.Login, settings.Password, config);
            transferUtility = new TransferUtility(client);
            ListBucketsResponse response = await client.ListBucketsAsync();

            bucket = response.Buckets.FirstOrDefault(x => x.BucketName == settings.Container);
            if (bucket == null) throw new InvalidOperationException($"Не найден S3 контейнер: {settings.Container}");

            logger.Info($"Подключены к S3 хранилищу от имени '{settings.Login}' к контейнеру '{settings.Container}'");
        }

        public Task Disconnect()
        {
            logger.Debug("Отключение от S3 хранилища...");
            return Task.CompletedTask;
        }

        public async Task UploadFile(string path, string destName, Action<UploaderProgress> callback=null)
        {
            logger.Debug($"Загружаем в облако архив '{destName}'");

            PutObjectRequest request = new PutObjectRequest();
            request.BucketName = bucket.BucketName;
            request.Key = destName;
            request.FilePath = path;
            request.UseChunkEncoding = false;
            if (callback != null)
            {
                var progress = new UploaderProgress();
                request.StreamTransferProgress += (o, ev) =>
                {
                    progress.Update((int)ev.TransferredBytes, (int)ev.TotalBytes);
                    callback(progress);
                };
            }

            await client.PutObjectAsync(request);
            logger.Info($"Архив '{destName}' успешно загружен в S3!");
        }

        protected class Settings
        {
            public string Provider { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public string Container { get; set; }
            public Config_Proxy Proxy { get; set; }
            public bool ForcePathStyle { get; set; }

            public class Config_Proxy
            {
                public string Host { get; set; }
                public int Port { get; set; }
                public string Login { get; set; }
                public string Password { get; set; }
            }
        }
    }
}