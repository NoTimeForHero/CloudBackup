using System.IO;
using System.Linq;
using System.Reflection;

namespace CloudBackuper
{
    public static class Extensions
    {
        public static string ConvertToValidFilename(this string input)
        {
            input = input.Replace(' ', '_');
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(invalidChar, '_');
            }
            return input;
        }

        public static string GetTitle(this Assembly assembly, string defaultValue = "Title")
        {
            return assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)
                       .OfType<AssemblyTitleAttribute>()
                       .FirstOrDefault()?.Title ?? defaultValue;
        }

        public static string GetDescription(this Assembly assembly, string defaultValue = "Description")
        {
            return assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
                .OfType<AssemblyDescriptionAttribute>()
                .FirstOrDefault()?.Description ?? defaultValue;
        }
    }
}
