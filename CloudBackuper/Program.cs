using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CloudBackuper.Utils;
using CloudBackuper.Web;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using Quartz;
using Quartz.Impl;
using Unity;

namespace CloudBackuper
{
    public sealed class Program : IDisposable, IShutdown
    {
        private readonly AutoResetEvent waitShutdown = new AutoResetEvent(false);
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IUnityContainer container = new UnityContainer();
        private readonly Config config;

        private WebServer webServer;

        public const string Filename_Config = "config.json";
        public const string Filename_Script = "scripts.js";

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            using (var program = new Program())
            {
#if DEBUG
                program.RunVoid(program, url => Process.Start(url));
#else
                await program.Run(program);
#endif
            }
        }

        public void RunVoid(IShutdown shutdown, Action<string> runAfter = null) => Run(shutdown, runAfter);

        public static string Title => Assembly.GetAssembly(typeof(Program)).GetTitle("CloudBackup");
        public static string Description => Assembly.GetAssembly(typeof(Program)).GetDescription("CloudBackup Description");

        private string AppPath => Path.GetDirectoryName(Assembly.GetAssembly(GetType()).Location);

        public Program()
        {
            Initializer.initLogging();
            logger.Warn("Приложение было запущено!");

            TaskScheduler.UnobservedTaskException += (o, ev) => Initializer.OnCriticalError(ev.Exception);
            AppDomain.CurrentDomain.UnhandledException += (o, ev) => Initializer.OnCriticalError(ev.ExceptionObject as Exception);

            var appDir = AppPath;
            if (appDir == null) throw new ArgumentException("Invalid path to assembly!");
            logger.Info($"Каталог откуда запущено приложение: {appDir}");

            string json = File.ReadAllText(Path.Combine(AppPath, Filename_Config));
            config = JsonConvert.DeserializeObject<Config>(json);
            Initializer.applyLoggingSettings(config.Logging);
        }

        public async Task Run(IShutdown shutdown, Action<string> runAfter = null)
        {
            var scheduler = await Initializer.GetScheduler(container, config);
            if (config.JobRetrying != null)
            {
                var listener = new JobFailureHandler(config.JobRetrying.MaxRetries, config.JobRetrying.WaitSeconds * 1000);
                scheduler.ListenerManager.AddJobListener(listener);
            }

            container.RegisterInstance(config);
            container.RegisterInstance(scheduler);
            if (shutdown != null) container.RegisterInstance(shutdown);

            var jsEngine = new JSEngine(Path.Combine(AppPath, Filename_Script));
            container.RegisterInstance(jsEngine);

            var controller = await new JobController(container).Constructor(config);
            container.RegisterInstance(controller);

            container.RegisterSingleton<PluginManager>().Resolve<PluginManager>();

            var staticFilesPath = Path.Combine(AppPath, "WebApp");
            logger.Debug($"Путь до папки со статикой: {staticFilesPath}");
            webServer = new WebServer(container, staticFilesPath);

            runAfter?.Invoke(config.HostingURI);

            waitShutdown.WaitOne();
        }

        public void Dispose()
        {
            logger.Warn("Приложение было закрыто!");
            webServer?.Dispose();
            waitShutdown.Set();
            container.Dispose();
            LogManager.Flush();
            LogManager.Shutdown();
        }

        public void Shutdown()
        {
            logger.Warn("Получен сигнал к завершению приложения!");
            waitShutdown.Set();
        }
    }

    public interface IShutdown
    {
        void Shutdown();
    }

    public class Initializer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static async Task<IScheduler> GetScheduler(IUnityContainer container, Config config, bool addAfterHandler = true)
        {
            NameValueCollection props = new NameValueCollection { { "quartz.serializer.type", "binary" } };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();
            scheduler.Context["states"] = new Dictionary<JobKey, UploadJobState>();
            scheduler.Context["container"] = container;
            scheduler.Context["config"] = config;
            if (addAfterHandler) scheduler.ListenerManager.AddJobListener(new JobAfterHandler());
            return scheduler;
        }

        public static void OnCriticalError(Exception ex)
        {
            string message = "Необработанная критическая ошибка! Приложение будет закрыто!";
            logger.Fatal(message);
            logger.Fatal(ex.Message);
            logger.Fatal(ex.ToString());
        }

        public static void initLogging()
        {
            var config = new LoggingConfiguration();

            var logFile = new FileTarget("logfile") { FileName = "debug.log" };
            var logConsole = new ConsoleTarget("logconsole");
            var logMemory = new MemoryTarget("logmemory") { MaxLogsCount = 5000 };

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logFile);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logMemory);

            LogManager.Configuration = config;
        }

        public static void applyLoggingSettings(Config_Logging settings)
        {
            var config = LogManager.Configuration;
            settings = settings ?? Config_Logging.Defaults;
            var webService = settings.WebService;
            var retryingWrapper = settings.RetryingWrapper;

            foreach (var target in config.LoggingRules) target.SetLoggingLevels(settings.LogLevel, LogLevel.Fatal);
            if (webService != null)
            {
                if (webService.Parameters.FirstOrDefault(x => x.Name == "message") == null)
                {
                    var defaultLayout = (new ConsoleTarget()).Layout;
                    webService.Parameters.Add(new MethodCallParameter("message", defaultLayout));
                }

                Target target = webService;
                if (retryingWrapper != null)
                {
                    retryingWrapper.WrappedTarget = webService;
                    target = retryingWrapper;
                }
                config.AddRule(settings.LogLevel, LogLevel.Fatal, target);
            }

            LogManager.ReconfigExistingLoggers();
            LogManager.Configuration.Reload();

            if (webService != null) logger.Info($"Отправка логов: {webService.Url}");
            logger.Info($"Установлен LogLevel: {settings.LogLevel}");
        }
    }
}
