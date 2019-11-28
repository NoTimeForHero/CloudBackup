using System.IO;

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
    }
}
