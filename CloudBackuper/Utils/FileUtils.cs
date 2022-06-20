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

        public static Node GetFilesInDirectory(
            string path,
            Config_Masks masks,
            GetFiles getFiles = null,
            GetDirectories getDirectories = null,
            Node node = null,
            List<string> flattenFiles = null)
        {
            if (getFiles == null) getFiles = xPath => Directory.GetFiles(xPath, "*");
            if (getDirectories == null) getDirectories = xPath => Directory.GetDirectories(xPath, "*");
            if (node == null) node = new Node { Name = Path.GetFileName(path), FullPath = path };

            var directories = getDirectories(path);
            foreach (var dir in directories)
            {
                var dirName = Path.GetFileName(dir);
                if (masks.DirectoriesExcluded != null && masks.DirectoriesExcluded.Contains(dirName)) continue;

                var dirPath = Path.Combine(path, dir);
                var dirNode = new Node { Name = dirName, FullPath = dirPath };
                dirNode = GetFilesInDirectory(dirPath, masks, getFiles, getDirectories, dirNode, flattenFiles);
                // flattenFiles?.AddRange(dirNode.Files);
                node.Nodes.Add(dirNode);
            }

            var files = getFiles(path);
            var rawFiles = new List<string>();
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file);
                var contains = masks.Masks.Contains(ext, StringComparer.InvariantCultureIgnoreCase);
                if (masks.MasksExclude && contains) continue;
                if (!masks.MasksExclude && !contains) continue;
                flattenFiles?.Add(file);
                rawFiles.Add(file);
            }
            logger.Debug($"Файлов подходящих условиям в '{path}' найдено: {rawFiles.Count}");
            node.Files = rawFiles;
            return node;
        }

        public static Node SliceNodes(Node input, int maxDepth, int maxSize, int currentLevel = 0)
        { 
            return input;
        }

        public class Node : ICloneable
        {
            public string Name;
            public string FullPath;
            public string MetaData;
            public List<string> Files = new List<string>();
            public List<Node> Nodes = new List<Node>();

            public override string ToString()
                => $"[Node Name={Name}, FullPath={FullPath}, Files={Files.Count}, Children={Nodes.Count}]";

            public object Clone()
            {
                var clone = new Node
                {
                    Name = Name,
                    FullPath = FullPath,
                    MetaData = MetaData,
                    Files = new List<string>(Files),
                    Nodes = Nodes.Select(x => (Node)x.Clone()).ToList()
                };
                return clone;
            }
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

        public static bool IsAssembly(string path)
        {
            var extension = Path.GetExtension(path);
            if (extension == ".exe") return true;
            if (extension == ".dll") return true;
            return false;
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