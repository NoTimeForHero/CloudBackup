using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Plugin_YandexDisk
{
    internal static class Extensions
    {
        public static string Join(this IEnumerable<string> parts, string separator)
            => string.Join(separator, parts);


        public static bool TryParse<T>(this string json, out T value, T defValue = default)
        {
            try
            {
                value = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine(ex);
                value = defValue;
                return false;
            }
        }
    }
}
