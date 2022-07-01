// File: AssemblyResolver.cs
// Created by NoTimeForHero, 2022
// Distributed under the Apache License 2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MaskPreview
{
    internal class AssemblyResolver
    {
        static readonly string AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static AssemblyResolver New() => new AssemblyResolver();
        private static readonly string[] searchExtensions = { "dll", "exe" };
        public readonly List<string> searchPaths = new List<string>();

        public AssemblyResolver DefaultPaths()
        {
            searchPaths.Add(Path.Combine(AssemblyPath, @"..\"));
            searchPaths.Add(Path.Combine(AssemblyPath, @"..\..\..\CloudBackuper\bin\Debug"));
            return this;
        }

        public void Register()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CustomAssemblyResolver;
            AppDomain.CurrentDomain.AssemblyResolve += CustomAssemblyResolver;
        }

        // Modified: https://stackoverflow.com/a/30214970
        private Assembly CustomAssemblyResolver(object sender, ResolveEventArgs ev)
        {
            var assemblyFile = (ev.Name.Contains(','))
                ? ev.Name.Substring(0, ev.Name.IndexOf(','))
                : ev.Name;
            foreach (var extension in searchExtensions)
            {
                var targetFile = assemblyFile + "." + extension;
                foreach (var path in searchPaths)
                {
                    var targetPath = Path.Combine(path, targetFile);
                    if (!File.Exists(targetPath)) continue;
                    try
                    {
                        return Assembly.LoadFrom(targetPath);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Fail during loading assembly: {targetPath}");
                        Console.Error.WriteLine(ex);
                    }
                }
            }
            return null;
        }

    }
}