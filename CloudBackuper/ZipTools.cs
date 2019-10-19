using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

        public ZipTools(string pathToDirectory, string[] extensions)
        {
            if (!Directory.Exists(pathToDirectory))
            {
                throw new ArgumentException($"Directory doesn't exists on path: {pathToDirectory}! ");
            }
            if (!extensions.All(name => name.StartsWith(".")))
            {
                throw new ArgumentException("All extensions must be start with '.' (for example: '.dat')");
            }
            this.pathToDirectory = pathToDirectory;

            logger.Info($"Путь поиска файлов: {pathToDirectory}");
            logger.Info($"Ищем файлы с расширениями: [{string.Join(", ",extensions)}]");

            string zipFilename = Path.ChangeExtension(Path.GetRandomFileName(), ".zip");
            zipPath = Path.Combine(Path.GetTempPath(), zipFilename);
            logger.Info($"Создан временный файл: {zipPath}");

            Func<string, bool> isMatch = name => extensions.Any(ext => Path.GetExtension(name) == ext);
            files = Directory.GetFiles(pathToDirectory, "*.*", SearchOption.AllDirectories).Where(isMatch).ToList();
        }

        public void CreateZip()
        {
            logger.Debug($"Файлов подходит по маске: {files.Count}");
            WriteToFile(pathToDirectory, files);
        }

        protected void WriteToFile(string pathToDirectory, List<string> files)
        {
            FileStream fsZip = null;
            FileStream fsInput = null;
            ZipArchive zip = null;
            Stream entryStream = null;

            try
            {
                fsZip = new FileStream(zipPath, FileMode.Create);
                zip = new ZipArchive(fsZip, ZipArchiveMode.Create);
                foreach (var path in files)
                {
                    var filename = FileUtils.GetRelativePath(pathToDirectory, path);
                    logger.Info($"Архивируем файл: " + filename);

                    var entry = zip.CreateEntry(filename);
                    entryStream = entry.Open();

                    fsInput = new FileStream(path, FileMode.Open);
                    fsInput.CopyTo(entryStream);

                    fsInput.Dispose();
                    entryStream.Dispose();
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