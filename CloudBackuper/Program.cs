using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using Unity;
using Unity.Injection;

namespace CloudBackuper
{
    static class Program
    {
        static readonly Logger logger = LogManager.GetLogger("Program");
        public static readonly IUnityContainer container = new UnityContainer();

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            initLogging();
            logger.Warn("Приложение было запущено!");

            TaskScheduler.UnobservedTaskException += (o, ev) => OnCriticalError(ev.Exception);
            AppDomain.CurrentDomain.UnhandledException += (o, ev) => OnCriticalError(ev.ExceptionObject as Exception);
            Application.ApplicationExit += (o, ev) => logger.Warn("Приложение было закрыто!");

            string json = File.ReadAllText("config.json");
            Config config = JsonConvert.DeserializeObject<Config>(json);
            applyLoggingSettings(config.Logging);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            container.RegisterInstance(config);
            container.RegisterSingleton<AppState>();
            container.RegisterSingleton<JobController>();
            container.RegisterSingleton<TrayIcon>();
            container.Resolve<TrayIcon>(); // RegisterSingleton ленивый, поэтому нужно его пнуть

            Application.Run();
        }

        static void OnCriticalError(Exception ex)
        {
            string message = "Необработанная критическая ошибка! Приложение будет закрыто!";
            logger.Fatal(message);
            logger.Fatal(ex.Message);
            logger.Fatal(ex.ToString());

            MessageBox.Show($"{message}\n\n{ex.Message}", "Критическая ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        static void initLogging()
        {
            var config = new LoggingConfiguration();

            var logfile = new FileTarget("logfile") { FileName = "debug.log" };
            var logconsole = new ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }

        static void applyLoggingSettings(Config_Logging settings)
        {
            var config = LogManager.Configuration;
            settings = settings ?? Config_Logging.Defaults;

            foreach (var target in config.LoggingRules) target.SetLoggingLevels(settings.LogLevel, LogLevel.Fatal);
            LogManager.ReconfigExistingLoggers();
            LogManager.Configuration.Reload();

            logger.Info($"Установлен LogLevel: {settings.LogLevel}");
        }
    }
}
