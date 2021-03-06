﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NLog;
using Ionic.Zip;
using CompressionLevel = Ionic.Zlib.CompressionLevel;

namespace CloudBackuper
{
    class ZipTools : IDisposable
    {
        protected readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        protected readonly string pathToDirectory;
        protected readonly List<string> files;
        protected readonly string password;

        protected readonly List<string> filesToDelete = new List<string>();

        public delegate void OnZipChanged(int total, int currentIndex, string currentName);

        public ZipTools(string pathToDirectory, List<string> files, string password)
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

        public string CreateZip(OnZipChanged callback=null)
        {
            var targetFile = RandomFilename();
            logger.Debug($"Создан временный файл: {targetFile}");

            logger.Debug($"Файлов подходит по маске: {files.Count}");
            WriteToFile(targetFile, pathToDirectory, files, callback);
            filesToDelete.Add(targetFile);

            logger.Info($"Архив успешно создан: {targetFile}");
            if (password == null) return targetFile;

            var encryptedFile = RandomFilename();
            EncryptFile(targetFile, encryptedFile, password, callback);
            filesToDelete.Add(encryptedFile);

            return encryptedFile;
        }

        protected string RandomFilename(string extension = ".zip")
        {
            string filename = Path.ChangeExtension(Path.GetRandomFileName(), extension);
            string path = Path.Combine(Path.GetTempPath(), filename);
            return path;
        }

        // Из-за отсутствия шифрования имён файлов в DotNetZip и во избежание проблем совместимости
        // зашифрованный Zip архив без сжатия через DotNetZip кладётся поверх обычного Zip архива, созданного средствами NET 4.5
        protected void EncryptFile(string fileInput, string fileOutput, string password, OnZipChanged callback)
        {
            using (var zip = new ZipFile(fileOutput))
            {
                callback(1, 1, "шифрование");
                zip.CompressionLevel = CompressionLevel.None;
                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                zip.Password = password;

                zip.AddFile(fileInput, "");
                zip.Save();
            }
        }

        protected void WriteToFile(string targetFile, string pathToDirectory, List<string> files, OnZipChanged callback)
        {
            FileStream fsZip = null;
            FileStream fsInput = null;
            ZipArchive zip = null;
            Stream entryStream = null;

            try
            {
                fsZip = new FileStream(targetFile, FileMode.Create);
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
            logger.Debug($"Удаляем временный файлы: " + string.Join(", ", filesToDelete.Select(x => $"\"{x}\"")));
            filesToDelete.ForEach(File.Delete);
        }
    }
}