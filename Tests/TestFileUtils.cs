using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var files = FileUtils.GetFilesInDirectory(@"root", masks, getFiles, getDirectories);
            CollectionAssert.AreEqual(new[] { "test.exe", "test.ini", "test2.exe" }, files.ToArray());

            masks.MasksExclude = true;
            files = FileUtils.GetFilesInDirectory(@"root", masks, getFiles, getDirectories);
            CollectionAssert.AreEqual(new[] { "test.dat", "test.dbf" }, files.ToArray());
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

            var files = FileUtils.GetFilesInDirectory(@"root", masks, getFiles, getDirectories);
            CollectionAssert.AreEqual(new[] { "file_in_include.xml", "file_in_exclude.xml" }, files.ToArray());

            masks.DirectoriesExcluded = new[] { "exclude" };
            files = FileUtils.GetFilesInDirectory(@"root", masks, getFiles, getDirectories);
            CollectionAssert.AreEqual(new[] { "file_in_include.xml" }, files.ToArray());
        }

    }
}
