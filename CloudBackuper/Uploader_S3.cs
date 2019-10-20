using System;
using System.Linq;
using System.Reflection;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using NLog;

namespace CloudBackuper
{
    class Uploader_S3
    {
        protected AmazonS3Client client;
        protected S3Bucket bucket;
        protected TransferUtility transferUtility;

        protected static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static Uploader_S3 _instance;
        private static readonly object padlock = new object();
        public static Uploader_S3 GetInstance(Config_S3 settings)
        {
            lock (padlock)
            {
                return _instance ?? (_instance = new Uploader_S3(settings));
            }
        }

        protected Uploader_S3(Config_S3 settings)
        {
            AmazonS3Config config = new AmazonS3Config();
            config.ServiceURL = settings.Provider;
            config.ForcePathStyle = settings.ForcePathStyle;

            client = new AmazonS3Client(settings.Login, settings.Password, config);
            transferUtility = new TransferUtility(client);
            ListBucketsResponse response = client.ListBuckets();

            bucket = response.Buckets.First(x => x.BucketName == settings.Container);
            if (bucket == null) throw new InvalidOperationException($"Can' find S3 bucket: {settings.Container}");

            logger.Info($"Подключены к S3 хранилищу от имени '{settings.Login}' к контейнеру '{settings.Container}'");
        }

        public void UploadFile(string path, string destName, EventHandler<StreamTransferProgressArgs> callback=null)
        {
            logger.Info($"Загружаем в облако архив '{destName}'");

            PutObjectRequest request = new PutObjectRequest();
            request.BucketName = bucket.BucketName;
            request.Key = destName;
            request.FilePath = path;
            request.UseChunkEncoding = false;
            if (callback != null) request.StreamTransferProgress += callback;

            client.PutObject(request);
            logger.Debug($"Архив '{destName}' успешно загружен!");
        }
    }
}