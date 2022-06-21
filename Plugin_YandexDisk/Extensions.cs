using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_YandexDisk
{
    internal static class Extensions
    {
        public static string Join(this IEnumerable<string> parts, string separator)
            => string.Join(separator, parts);
    }
}
