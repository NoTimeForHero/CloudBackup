using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Unity;

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

        public static void TryDispose<T>(this IUnityContainer container) where T : IDisposable
        {
            if (!container.IsRegistered<T>()) return;
            var obj = container.Resolve<T>();
            obj.Dispose();
        }

        public static TVal SafeGet<TKey, TVal>(this IDictionary<TKey, TVal> dictionary, TKey key)
        {
            if (!dictionary.ContainsKey(key)) return default;
            return dictionary[key];
        }

        public static T Cast<T>(this ExpandoObject obj) => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));

        public static TOutput MapPresent<TInput, TOutput>(this TInput? input, Func<TInput, TOutput> updater) where TInput : struct
        {
            if (!input.HasValue) return default;
            return updater(input.Value);
        }
    }
}
