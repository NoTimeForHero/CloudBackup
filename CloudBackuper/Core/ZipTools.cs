using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using NLog;
using Ionic.Zip;
using CompressionLevel = Ionic.Zlib.CompressionLevel;

namespace CloudBackuper
{
    public class ZipTools : IDisposable
    {
        protected readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        protected readonly string pathToDirectory;
        protected readonly List<string> files;
        protected readonly string password;
        protected CancellationTokenSource cancellation;

        protected readonly List<string> filesToDelete = new List<string>();

        public delegate void OnZipChanged(int total, int currentIndex, string currentName);

        public ZipTools(string pathToDirectory, List<string> files, string password = null)
        {
            if (!Directory.Exists(pathToDirectory))
            {
                throw new ArgumentException($"Directory doesn't exists on path: {pathToDirectory}! ");
            }

            this.files = files;
            this.pathToDirectory = pathToDirectory;
            this.password = password;

            logger.Debug($"Папка с файлами для архивации: {pathToDirectory}");

        }

        public void Cancel()
        {
            cancellation?.Cancel();
        }

        public string CreateZip(OnZipChanged callback=null, string finalName = null)
        {
            var targetFile = RandomFilename();
            if (password == null && finalName != null) targetFile = finalName;
            logger.Debug($"Создан временный файл: {targetFile}");

            logger.Debug($"Файлов подходит по маске: {files.Count}");
            cancellation = new CancellationTokenSource();
            WriteToFile(targetFile, pathToDirectory, files, callback);

            logger.Info($"Архив успешно создан: {targetFile}");
            if (password == null) return targetFile;

            var encryptedFile = finalName ?? RandomFilename();
            EncryptFile(targetFile, encryptedFile, password, callback);
            File.Delete(targetFile);

            return encryptedFile;
        }

        public static string RandomFilename(string extension = ".zip", string prefix = "")
        {
            string filename = Path.ChangeExtension(Path.GetRandomFileName(), extension);
            string path = Path.Combine(Path.GetTempPath(), prefix + filename);
            return path;
        }

        // Из-за отсутствия шифрования имён файлов в DotNetZip и во избежание проблем совместимости
        // зашифрованный Zip архив без сжатия через DotNetZip кладётся поверх обычного Zip архива, созданного средствами NET 4.5
        protected void EncryptFile(string fileInput, string fileOutput, string password, OnZipChanged callback)
        {
            using (var zip = new ZipFile(fileOutput))
            {
                // TODO: Подробный процесс шифрования файла?
                // TODO: Возможность отмены шифрования
                callback(1, 1, "шифрование");
                zip.CompressionLevel = CompressionLevel.None;
                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                zip.Password = password;

                zip.AddFile(fileInput, "");
                zip.Save();
            }
            filesToDelete.Add(fileOutput);
        }

        protected void WriteToFile(string targetFile, string pathToDirectory, List<string> files, OnZipChanged callback)
        {
            FileStream fsZip = null;
            FileStream fsInput = null;
            ZipArchive zip = null;
            Stream entryStream = null;
            bool forceRemove = false;

            try
            {
                fsZip = new FileStream(targetFile, FileMode.Create);
                zip = new ZipArchive(fsZip, ZipArchiveMode.Create);

                int total = files.Count;
                int index = 0;

                foreach (var path in files)
                {
                    cancellation.Token.ThrowIfCancellationRequested();
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
                filesToDelete.Add(targetFile);
            }
            catch (OperationCanceledException)
            {
                forceRemove = true;
                throw;
            }
            finally
            {
                zip?.Dispose();
                fsZip?.Dispose();

                fsInput?.Dispose();
                entryStream?.Dispose();
                if (forceRemove) TryRemove(targetFile);
            }
        }

        private void TryRemove(string file)
        {
            if (!File.Exists(file)) return;
            File.Delete(file);
        }

        public void Dispose()
        {
            logger.Debug($"Удаляем временный файлы: " + string.Join(", ", filesToDelete.Select(x => $"\"{x}\"")));
            foreach (var file in filesToDelete) TryRemove(file);
            filesToDelete.Clear();
        }
    }
}