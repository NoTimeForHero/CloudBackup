using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using NLog;
using Quartz;

namespace CloudBackuper
{
    static class Program
    {
        static readonly Logger logger = NLog.LogManager.GetLogger("Program");

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            initLogging();
            logger.Info("Application started!");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string json = File.ReadAllText("config.json");
            Config config = JsonConvert.DeserializeObject<Config>(json);

            TaskScheduler.UnobservedTaskException += (o, ev) => OnCriticalError(ev.Exception);
            AppDomain.CurrentDomain.UnhandledException += (o, ev) => OnCriticalError(ev.ExceptionObject as Exception);

            var jobController = new JobController(config);
            var icon = new TrayIcon(jobController);
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
            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "debug.log" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            NLog.LogManager.Configuration = config;
        }
    }
}
