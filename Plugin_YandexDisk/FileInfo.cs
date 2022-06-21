using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysPath = System.IO.Path;

namespace Plugin_YandexDisk
{
    internal class FileInfo
    {
        public string Path { get; }
        public string Name { get; }
        public string SafeName { get; }
        public string NameWithoutExtension { get; }
        public string Extension;
        public bool IsNameSafe { get; }

        public FileInfo(string remotePath, string safeExtension = "bin", string[] unsafeExtensions = null)
        {
            unsafeExtensions = unsafeExtensions ?? new[] { "zip", "rar", "exe" };
            var destParts = remotePath.Split('/');
            Path = destParts.Take(destParts.Length - 1).Join("/");
            Name = destParts[destParts.Length - 1];
            NameWithoutExtension = SysPath.GetFileNameWithoutExtension(Name);
            Extension = SysPath.GetExtension(Name);
            IsNameSafe = unsafeExtensions.Contains(Extension);
            SafeName = IsNameSafe ? Name : $"{NameWithoutExtension}.{safeExtension}";
        }
    }
}
