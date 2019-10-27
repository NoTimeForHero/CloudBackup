using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using NLog;

namespace CloudBackuper
{
    class ZipTools : IDisposable
    {
        protected readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        protected readonly string zipPath;
        protected readonly string pathToDirectory;
        protected readonly List<string> files;

        public string Filename => zipPath;

        public delegate void OnZipChanged(int total, int currentIndex, string currentName);

        public ZipTools(string pathToDirectory, List<string> files)
        {
            if (!Directory.Exists(pathToDirectory))
            {
                throw new ArgumentException($"Directory doesn't exists on path: {pathToDirectory}! ");
            }

            this.files = files;
            this.pathToDirectory = pathToDirectory;

            logger.Debug($"Папка с файлами для архивации: {pathToDirectory}");

            string zipFilename = Path.ChangeExtension(Path.GetRandomFileName(), ".zip");
            zipPath = Path.Combine(Path.GetTempPath(), zipFilename);
            logger.Debug($"Создан временный файл: {zipPath}");

        }

        public void CreateZip(OnZipChanged callback=null)
        {
            logger.Debug($"Файлов подходит по маске: {files.Count}");
            WriteToFile(pathToDirectory, files, callback);
            logger.Info($"Архив успешно создан: {zipPath}");
        }

        protected void WriteToFile(string pathToDirectory, List<string> files, OnZipChanged callback)
        {
            FileStream fsZip = null;
            FileStream fsInput = null;
            ZipArchive zip = null;
            Stream entryStream = null;

            try
            {
                fsZip = new FileStream(zipPath, FileMode.Create);
                zip = new ZipArchive(fsZip, ZipArchiveMode.Create);

                int total = files.Count;
                int index = 0;

                foreach (var path in files)
                {
                    var filename = FileUtils.GetRelativePath(pathToDirectory, path);
                    logger.Trace($"Архивируем файл: " + filename);
                    callback?.Invoke(total, index, filename);

                    var entry = zip.CreateEntry(filename);
                    entryStream = entry.Open();

                    fsInput = new FileStream(path, FileMode.Open);
                    fsInput.CopyTo(entryStream);

                    fsInput.Dispose();
                    entryStream.Dispose();

                    index++;
                }
            }
            finally
            {
                zip?.Dispose();
                fsZip?.Dispose();

                fsInput?.Dispose();
                entryStream?.Dispose();
            }
        }

        public void Dispose()
        {
            logger.Debug($"Удаляем временный файл: {zipPath}");
            File.Delete(zipPath);
        }
    }
}