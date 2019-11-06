using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NLog;

namespace CloudBackuper
{
    public class FileUtils
    {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public delegate string[] GetFiles(string path);
        public delegate string[] GetDirectories(string path);

        public static List<string> GetFilesInDirectory(string path, Config_Masks masks,
            GetFiles getFiles = null, GetDirectories getDirectories = null)
        {
            if (getFiles == null) getFiles = xPath => Directory.GetFiles(xPath, "*");
            if (getDirectories == null) getDirectories = xPath => Directory.GetDirectories(xPath, "*");

            var output = new List<string>();
            var directories = getDirectories(path);
            foreach (var dir in directories)
            {
                var dirName = Path.GetFileName(dir);
                if (masks.DirectoriesExcluded != null && masks.DirectoriesExcluded.Contains(dirName)) continue;

                var dirPath = Path.Combine(path, dir);
                var dirFiles = GetFilesInDirectory(dirPath, masks, getFiles, getDirectories);
                output.AddRange(dirFiles);
            }

            var files = getFiles(path);
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file);
                var contains = masks.Masks.Contains(ext, StringComparer.InvariantCultureIgnoreCase);
                if (masks.MasksExclude && contains) continue;
                if (!masks.MasksExclude && !contains) continue;
                output.Add(file);

            }
            logger.Debug($"Файлов подходящих условиям в '{path}' найдено: {output.Count}");
            return output;
        }

        // Best way to convert absolute path to relative
        // Author: https://stackoverflow.com/a/485516
        // For example, input:
        // ["C:\Windows\System32\", "C:\Windows\System32\drivers\etc\hosts"]
        // output: ".\drivers\etc\hosts"
        public static string GetRelativePath(string fromPath, string toPath)
        {
            int fromAttr = RelativePath.GetPathAttribute(fromPath);
            int toAttr = RelativePath.GetPathAttribute(toPath);

            StringBuilder path = new StringBuilder(260); // MAX_PATH
            if (RelativePath.PathRelativePathTo(
                    path,
                    fromPath,
                    fromAttr,
                    toPath,
                    toAttr) == 0)
            {
                throw new ArgumentException("Paths must have a common prefix");
            }
            var output = path.ToString();
            output = output.Substring(2, output.Length - 2);
            return output;
        }

        private static class RelativePath
        {
            public static int GetPathAttribute(string path)
            {
                DirectoryInfo di = new DirectoryInfo(path);
                if (di.Exists)
                {
                    return FILE_ATTRIBUTE_DIRECTORY;
                }

                FileInfo fi = new FileInfo(path);
                if (fi.Exists)
                {
                    return FILE_ATTRIBUTE_NORMAL;
                }

                throw new FileNotFoundException();
            }

            private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
            private const int FILE_ATTRIBUTE_NORMAL = 0x80;

            [DllImport("shlwapi.dll", SetLastError = true)]
            public static extern int PathRelativePathTo(StringBuilder pszPath, string pszFrom, int dwAttrFrom, string pszTo, int dwAttrTo);
        }
    }
}