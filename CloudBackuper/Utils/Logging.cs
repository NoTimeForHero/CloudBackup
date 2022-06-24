using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace CloudBackuper.Utils
{
    internal class Logging
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public enum ErrorType
        {
            AppDomain,
            TaskScheduler
        }

        public static void OnUnhandledError(Exception ex, ErrorType type)
        {
            if (ex is AggregateException && 
                ex.InnerException != null && 
                ex.InnerException is HttpListenerException netEx)
            {
                logger.Debug($"Ошибка! {netEx.Message}");
                return;
            }
            string message = $"Необработанная ошибка в {type}!";
            logger.Warn(message);
            logger.Warn(ex.Message);
            logger.Warn(ex.ToString());
        }

        private static LoggingConfiguration MakeDefault()
        {
            var config = new LoggingConfiguration();

            var logFile = new FileTarget("logfile") { FileName = "debug.log" };
            var logConsole = new ConsoleTarget("logconsole");
            var logMemory = new MemoryTarget("logmemory") { MaxLogsCount = 5000 };

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logFile);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logMemory);
            return config;
        }

        public static void initLogging() => LogManager.Configuration = MakeDefault();

        public static void applyLoggingSettings(Config_Logging settings)
        {
            LogManager.Configuration = MakeDefault();
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
