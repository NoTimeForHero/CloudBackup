using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CloudBackuper.Plugins;
using NLog;

namespace CloudBackuper
{
    class PluginManager
    {
        protected Logger logger = LogManager.GetCurrentClassLogger();
        protected Config_Uploader config;
        public List<Assembly> Assemblies = new List<Assembly>();
        public List<IPlugin> Plugins = new List<IPlugin>();
        public IUploader Uploader { get; }

        public PluginManager(Config root)
        {
            LookupAssemblies();
            config = root.Uploader;
            var typeName = root.Uploader.Type;
            var type = Type.GetType(typeName);
            if (type == null)
                type = Assemblies
                    .SelectMany(x => x.DefinedTypes)
                    .FirstOrDefault(x => x.FullName == typeName);
            if (type == null) throw new ArgumentException($"Не найден тип: {typeName}");
            if (!typeof(IUploader).IsAssignableFrom(type))
                throw new ApplicationException(
                    $"Тип \"{type.FullName}\" не наследует интерфейс {nameof(IUploader)}!");
            Uploader = (IUploader)Activator.CreateInstance(type);
            Uploader.Initialize(root.Uploader.Settings);
        }

        public IEnumerable<object> ListPlugin()
        {
            return Plugins.Select(plugin => new
            {
                plugin.Id,
                plugin.Name,
                plugin.Description,
                plugin.Url,
                plugin.Author,
                Version = plugin.GetType().Assembly.GetName().Version.ToString()
            });
        }

        public void LookupAssemblies()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            if (location == null) throw new ApplicationException("Не удалось найти путь запуска!");
            var runPath = Path.GetDirectoryName(location);
            var pluginPath = Path.Combine(runPath, "plugins");
            logger.Info($"Директория плагинов: ${pluginPath}");
            if (!Directory.Exists(pluginPath)) return;
            var pluginDirs = Directory.GetDirectories(pluginPath);
            foreach (var pluginDir in pluginDirs)
            {
                var pluginDirName = Path.GetFileName(pluginDir);
                var pluginFile = Directory.GetFiles(pluginDir).FirstOrDefault(x => x.ToLower().EndsWith(".plugin.dll"));
                if (pluginFile == null)
                {
                    logger.Warn($"Не найден DLL файл плагина (xxxxx.plugin.dll) в папке: {pluginDirName}!");
                    continue;
                }
                logger.Info($"Загружаем файл плагина: {Path.GetFileName(pluginFile)}");
                var assembly = Assembly.LoadFrom(pluginFile);
                var pluginType = assembly.DefinedTypes.FirstOrDefault(x => typeof(IPlugin).IsAssignableFrom(x));
                if (pluginType == null)
                {
                    logger.Warn($"Не удалось найти {nameof(IPlugin)} в папке: {pluginDirName}!");
                    continue;
                }
                var plugin = (IPlugin)Activator.CreateInstance(pluginType);
                Assemblies.Add(assembly);
                Plugins.Add(plugin);
                logger.Info($"Загружен плагин \"{plugin.Name}\" (id: {plugin.Id}) из папки {pluginDirName}");
            }
        }
    }
}
