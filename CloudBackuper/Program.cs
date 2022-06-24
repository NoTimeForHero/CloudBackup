using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CloudBackuper.Core.Quartz;
using CloudBackuper.Utils;
using CloudBackuper.Web;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using Quartz;
using Unity;
using YamlDotNet.Serialization;

namespace CloudBackuper
{
    public sealed class Program : IDisposable
    {
        private static bool DEBUG_MODE;
        private IUnityContainer container = new UnityContainer();
        public static Program Instance { get; private set; }

        private readonly AutoResetEvent waitShutdown = new AutoResetEvent(false);
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        public readonly bool IsService;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            using (var program = new Program(false))
            {
#if DEBUG
                    DEBUG_MODE = true;
#endif
                await program.Run(url => Process.Start(url));
            }
        }

        public Program(bool isService)
        {
            IsService = isService;
            Instance = this;
            Logging.initLogging();
            logger.Warn("Приложение было запущено!");
            logger.Info($"Каталог откуда запущено приложение: {Information.AppPath}");

            TaskScheduler.UnobservedTaskException += (o, ev) => Logging.OnUnhandledError(ev.Exception, Logging.ErrorType.TaskScheduler);
            AppDomain.CurrentDomain.UnhandledException += (o, ev) => Logging.OnUnhandledError(ev.ExceptionObject as Exception, Logging.ErrorType.AppDomain);
        }

        public async Task Run(Action<string> runAfter = null)
        {
            await Reload();

            var config = container.Resolve<Config>();
            runAfter?.Invoke(config.HostingURI);

            waitShutdown.WaitOne();
        }

        public async Task Reload(bool log=false)
        {
            container?.Dispose();
            container = new UnityContainer();
            await Task.Delay(500);

            if (log) logger.Warn("Запрошена перезагрузка конфигурации!");
            var config = YamlTools.Deserialize<Config>(Information.AppPath, Information.Filename_Config, "config.debug.json");

            Logging.applyLoggingSettings(config.Logging);
            container.RegisterInstance(config);

            //var scheduler = await Initializer.GetScheduler(container, config);
            //container.RegisterInstance(scheduler);

            var jsEngine = new JSEngine(Path.Combine(Information.AppPath, Information.Filename_Script));
            container.RegisterInstance(jsEngine);

            container.TryDispose<JobController>();
            var controller = await new JobController(container).Constructor(config);
            container.RegisterInstance(controller);

            var pm = new PluginManager(config);
            container.RegisterInstance(pm);

            var staticFilesPath = Path.Combine(Information.AppPath, "WebApp");
            var webServer = new WebServer(container, staticFilesPath, DEBUG_MODE);
            container.RegisterInstance(webServer);
        }

        public void Debug()
        {
            container.Dispose();
        }

        public void Dispose()
        {
            logger.Warn("Приложение было закрыто!");
            waitShutdown.Set();
            container.Dispose();
            LogManager.Flush();
            LogManager.Shutdown();
        }

        public void Shutdown()
        {
            if (IsService) return;
            logger.Warn("Получен сигнал к завершению приложения!");
            waitShutdown.Set();
        }
    }
}
