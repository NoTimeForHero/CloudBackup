using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudBackuper;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Tests
{
    [TestClass]
    public class TestFileUtils
    {
        [TestInitialize]
        public void Startup()
        {
            var config = new LoggingConfiguration();
            var target = new ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, target);
            LogManager.Configuration = config;
        }

        [TestMethod]
        public void SimpleFiles()
        {
            Config_Masks masks = new Config_Masks
            {
                DirectoriesExcluded = new string[] {},
                Masks = new[] { ".ini", ".exe" }
            };

            FileUtils.GetFiles getFiles = _ => { return new[] { "test.exe", "test.ini", "test.dat", "test.dbf", "test2.exe" }; };
            FileUtils.GetDirectories getDirectories = _ => { return new string[] { }; };

            var files = new List<string>();
            var nodes = FileUtils.GetFilesInDirectory(@"root", masks, getFiles, getDirectories, flattenFiles: files);
            CollectionAssert.AreEqual(new[] { "test.exe", "test.ini", "test2.exe" }, files.ToArray());

            files = new List<string>();
            masks.MasksExclude = true;
            nodes = FileUtils.GetFilesInDirectory(@"root", masks, getFiles, getDirectories, flattenFiles: files);
            CollectionAssert.AreEqual(new[] { "test.dat", "test.dbf" }, files.ToArray());
        }

        [TestMethod]
        public void CaseInsensetiveFiles()
        {
            Config_Masks masks = new Config_Masks
            {
                DirectoriesExcluded = new string[] { },
                Masks = new[] {".bin"}
            };

            var files = new[] { "TEST.BIN", "test2.Bin", "test3.BIN", "test4.bIN", "test5.bin" };

            // ReSharper disable once AccessToModifiedClosure
            FileUtils.GetFiles getFiles = _ => files;

            FileUtils.GetDirectories getDirectories = _ => { return new string[] { }; };

            var flattenFiles = new List<string>();
            var result = FileUtils.GetFilesInDirectory(@"root", masks, getFiles, getDirectories, flattenFiles: flattenFiles);
            Assert.AreEqual(files.Length, result.Files.Count);
            CollectionAssert.AreEqual(files, flattenFiles.ToArray());
        }

        [TestMethod]
        public void Directories()
        {
            Config_Masks masks = new Config_Masks
            {
                MasksExclude = false,
                Masks = new[] { ".xml" },
            };

            FileUtils.GetDirectories getDirectories = path =>
            {
                switch (path)
                {
                    case "root":
                        return new[] {"include", "exclude"};
                    default:
                        return new string[] { };
                }
            };

            FileUtils.GetFiles getFiles = path =>
            {
                Console.WriteLine($"getFiles({path})");
                switch (path)
                {
                    case @"root\include":
                        return new[] { "file_in_include.xml", "trash.tmp" };
                    case @"root\exclude":
                        return new[] { "file_in_exclude.xml" };
                    default:
                        return new string[] {};
                }
            };

            var flattenFiles = new List<string>();
            var nodes = FileUtils.GetFilesInDirectory(@"root", masks, getFiles, getDirectories, flattenFiles: flattenFiles);
            CollectionAssert.AreEqual(new[] { "file_in_include.xml", "file_in_exclude.xml" }, flattenFiles.ToArray());

            flattenFiles = new List<string>();
            masks.DirectoriesExcluded = new[] { "exclude" };
            nodes = FileUtils.GetFilesInDirectory(@"root", masks, getFiles, getDirectories, flattenFiles: flattenFiles);
            CollectionAssert.AreEqual(new[] { "file_in_include.xml" }, flattenFiles.ToArray());
        }

        [TestMethod]
        public void RandomDeep()
        {
            var masks = new[] { ".xlsx", ".docx" };
            var requiredFiles = new List<string>();
            Func<int, string> nameGenerator = (x) =>
            {
                var filename = Random.String(8);
                if (Random.Chance(20)) return filename + "." + Random.String(3);
                var fullFile = filename + Random.Element(masks);
                requiredFiles.Add(fullFile);
                return fullFile;
            };
            var rootDir = "root";
            var filesystem = FakeFilesystem.Create(rootDir, 5, 10, 3, nameGenerator);
            var config = new Config_Masks { Masks = masks };
            var flattenFiles = new List<string>();
            var nodes = FileUtils.GetFilesInDirectory(rootDir, config, filesystem.GetFiles, filesystem.GetDirectories, flattenFiles: flattenFiles);

            requiredFiles = requiredFiles.OrderBy(x => x).ToList();
            flattenFiles = flattenFiles.OrderBy(x => x).ToList();
            CollectionAssert.AreEqual(requiredFiles, flattenFiles.OrderBy(x => x).ToArray());

        }

    }

    internal class FakeFilesystem
    {
        private readonly Dictionary<string, Node> byRoot = new Dictionary<string, Node>();
        private FakeFilesystem() { }

        public string[] GetFiles(string path)
            => byRoot[path].files.ToArray();

        public string[] GetDirectories(string path)
            => byRoot[path].nodes.Select(x => x.name).ToArray();

        private void Iterate(Node node, string path)
        {
            byRoot[path] = node;
            foreach (var subNode in node.nodes)
            {
                Iterate(subNode, Path.Combine(path, subNode.name));
            }
        }

        public static FakeFilesystem Create(string rootDir, int maxFolder, int maxFiles, int maxDepth, Func<int, string> filenameGenerator)
        {
            var node = Generate(maxFolder, maxFiles, maxDepth, 1, filenameGenerator);
            node.name = rootDir;
            var instance = new FakeFilesystem();
            instance.Iterate(node, node.name);
            return instance;
        }

        private static Node Generate(int maxFolder, int maxFiles, int maxDepth, int depth, Func<int, string> filenameGenerator)
        {
            var root = new Node
            {
                name = Random.String(8),
                files = Random.List(2, maxFiles, filenameGenerator) // x => Random.String(8) + "." + Random.String(3)
            };
            if (depth < maxDepth)
            {
                root.nodes = Random.List(2, maxFolder, (x) => Generate(maxFolder, maxFiles, maxDepth, depth + 1, filenameGenerator));
            }
            return root;
        }

        private class Node
        {
            public string name;
            public List<string> files = new List<string>();
            public List<Node> nodes = new List<Node>();

            public override string ToString()
                => $"[Node Files={files.Count}, Children={nodes.Count}]";
        }
    }


    class Random
    {
        private static System.Random rnd = new System.Random(148);

        public static List<T> List<T>(int min, int count, Func<int, T> func)
            => Enumerable.Range(0, min + rnd.Next(count)).Select(func).ToList();

        // TODO: Заменить T[] на ICollection<T>
        public static T Element<T>(T[] elements)
            => elements[rnd.Next(elements.Length - 1)];


        public static bool Chance(int chance, int max = 100)
            => rnd.Next(0, max) > chance;

        public static string String(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }
    }
}
