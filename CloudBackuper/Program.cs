using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var program = new Program())
            {
                program.Run(program);
            }
        }

        public Program()
        {
            Initializer.initLogging();
            logger.Warn("Приложение было запущено!");

            TaskScheduler.UnobservedTaskException += (o, ev) => Initializer.OnCriticalError(ev.Exception);
            AppDomain.CurrentDomain.UnhandledException += (o, ev) => Initializer.OnCriticalError(ev.ExceptionObject as Exception);

            string json = File.ReadAllText("config.json");
            config = JsonConvert.DeserializeObject<Config>(json);
            Initializer.applyLoggingSettings(config.Logging);
        }

        public async void Run(IShutdown shutdown)
        {
            var scheduler = await Initializer.GetScheduler(config);
            if (config.JobRetrying != null)
            {
                var listener = new JobFailureHandler(config.JobRetrying.MaxRetries, config.JobRetrying.WaitSeconds * 1000);
                scheduler.ListenerManager.AddJobListener(listener);
            }

            container.RegisterInstance(config);
            container.RegisterInstance(scheduler);
            if (shutdown != null) container.RegisterInstance(shutdown);

            new JobController(container);
            webServer = new WebServer(container);

            waitShutdown.WaitOne();
        }

        public void Dispose()
        {
            logger.Warn("Приложение было закрыто!");
            webServer?.Dispose();
            waitShutdown.Set();
            container.Dispose();
            LogManager.Shutdown();
        }

        public void Shutdown()
        {
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

        public static async Task<IScheduler> GetScheduler(Config config)
        {
            NameValueCollection props = new NameValueCollection { { "quartz.serializer.type", "binary" } };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();
            scheduler.Context["states"] = new Dictionary<JobKey, UploadJobState>();
            scheduler.Context["config"] = config;
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
