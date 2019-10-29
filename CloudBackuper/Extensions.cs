using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
