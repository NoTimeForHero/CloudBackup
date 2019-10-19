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

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            initLogging();
            logger.Info("Application started!");

            TaskScheduler.UnobservedTaskException += (o, ev) => OnCriticalError(ev.Exception);
            AppDomain.CurrentDomain.UnhandledException += (o, ev) => OnCriticalError(ev.ExceptionObject as Exception);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string json = File.ReadAllText("config.json");
            Config config = JsonConvert.DeserializeObject<Config>(json);

            var container = new UnityContainer();
            container.RegisterInstance(config);
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

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }
    }
}
