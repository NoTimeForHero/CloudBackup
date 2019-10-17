using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CloudBackuper
{
    // Best way to convert absolute path to relative
    // Author: https://stackoverflow.com/a/485516
    // For example, input:
    // ["C:\Windows\System32\", "C:\Windows\System32\drivers\etc\hosts"]
    // output: ".\drivers\etc\hosts"
    class FileUtils
    {
        public static string GetRelativePath(string fromPath, string toPath)
        {
            int fromAttr = GetPathAttribute(fromPath);
            int toAttr = GetPathAttribute(toPath);

            StringBuilder path = new StringBuilder(260); // MAX_PATH
            if (PathRelativePathTo(
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

        private static int GetPathAttribute(string path)
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
        private static extern int PathRelativePathTo(StringBuilder pszPath, string pszFrom, int dwAttrFrom, string pszTo, int dwAttrTo);
    }
}