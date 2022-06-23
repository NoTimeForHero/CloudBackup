using System;
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
using Unity;

namespace CloudBackuper
{
    public sealed class Program : IDisposable, IShutdown
    {
        private static bool DEBUG_MODE;
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
                    DEBUG_MODE = true;
#endif
                await program.Run(program, url => Process.Start(url));
            }
        }

        public void RunVoid(IShutdown shutdown, Action<string> runAfter = null) => Run(shutdown, runAfter);

        public Program()
        {
            Logging.initLogging();
            logger.Warn("Приложение было запущено!");

            TaskScheduler.UnobservedTaskException += (o, ev) => Logging.OnUnhandledError(ev.Exception, Logging.ErrorType.TaskScheduler);
            AppDomain.CurrentDomain.UnhandledException += (o, ev) => Logging.OnUnhandledError(ev.ExceptionObject as Exception, Logging.ErrorType.AppDomain);

            logger.Info($"Каталог откуда запущено приложение: {Information.AppPath}");
            string json = File.ReadAllText(Path.Combine(Information.AppPath, Filename_Config));
            config = JsonConvert.DeserializeObject<Config>(json);
            Logging.applyLoggingSettings(config.Logging);
        }

        public async Task Run(IShutdown shutdown, Action<string> runAfter = null)
        {
            var scheduler = await Initializer.GetScheduler(container, config);

            container.RegisterInstance(config);
            container.RegisterInstance(scheduler);
            if (shutdown != null) container.RegisterInstance(shutdown);

            var jsEngine = new JSEngine(Path.Combine(Information.AppPath, Filename_Script));
            container.RegisterInstance(jsEngine);

            var controller = await new JobController(container).Constructor(config);
            container.RegisterInstance(controller);

            container.RegisterSingleton<PluginManager>().Resolve<PluginManager>();

            var staticFilesPath = Path.Combine(Information.AppPath, "WebApp");
            logger.Debug($"Путь до папки со статикой: {staticFilesPath}");
            webServer = new WebServer(container, staticFilesPath, DEBUG_MODE);

            runAfter?.Invoke(config.HostingURI);

            waitShutdown.WaitOne();
        }

        public void Restart()
        {

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
}
