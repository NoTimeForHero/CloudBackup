﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CloudBackuper;
using CloudBackuper.Core.Quartz;
using CloudBackuper.Utils;
using CloudBackuper.Web;
using NLog;
using NLog.Config;
using NLog.Targets;
using Quartz;
using Unity;
using WebServer = CloudBackuper.Web.WebServer;

namespace TestFrontend
{
    class Program
    {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public static readonly IUnityContainer container = new UnityContainer();

        static async Task Main(string[] args)
        {
            var logCfg = new LoggingConfiguration();
            logCfg.AddRuleForAllLevels(new MemoryTarget("logmemory"));
            logCfg.AddRuleForAllLevels(new ConsoleTarget("logconsole"));
            LogManager.Configuration = logCfg;
            LogManager.ReconfigExistingLoggers();

            var config = new CloudBackuper.Config();
            var controller = await new JobController(container).Constructor(config);
            var scheduler = controller.Scheduler;
            scheduler.ListenerManager.AddJobListener(new JobFailureHandler(3, 300));

            container.RegisterInstance(config);
            container.RegisterInstance(scheduler);
            for (int i = 0; i < 10; i++) FakeJob.AddJob(scheduler, $"TestJob{i}");

            var dirStatic = Directory.Exists("WebApp") ? "WebApp" : null;
            new WebServer(container, developmentMode: true, pathStaticFiles: dirStatic);

            Console.WriteLine("Для выхода напишите 'quit'.");
            while (true)
            {
                bool exit = false;
                var line = Console.ReadLine();
                if (line == null) continue;

                switch (line.Trim())
                {
                    case "test":
                        logger.Trace("Example trace message");
                        logger.Debug("Example debug message");
                        logger.Info("Example info message");
                        logger.Warn("Example warning message");
                        logger.Error("Example error message");
                        logger.Fatal("Example fatal message");
                        break;
                    case "help":
                        logger.Info("test - тест логгинга");
                        logger.Info("help - помощь");
                        logger.Info("quit - выход");
                        break;
                    case "quit":
                        exit = true;
                        break;
                    default:
                        logger.Info("Неизвестная команда!");
                        break;
                }

                if (exit) break;
            }
        }
    }

    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    class FakeJob : IJob
    {
        protected static readonly Random rnd = new Random();
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected UploadJobState getState(IJobExecutionContext context)
        {
            var states = (Dictionary<JobKey, UploadJobState>)context.Scheduler.Context["states"];
            return states[context.JobDetail.Key];
        }

        public static void AddJob(IScheduler scheduler, string name)
        {
            var jobStates = (Dictionary<JobKey, UploadJobState>)scheduler.Context["states"];

            var job = JobBuilder.Create<FakeJob>()
                .WithIdentity(name)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity(name)
                .StartAt(DateTimeOffset.MaxValue)
                .Build();

            jobStates.Add(job.Key, new UploadJobState());
            scheduler.ScheduleJob(job, trigger);
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (rnd.Next(1, 100) > 60) throw new AccessViolationException("Something failed?");

            logger.Info("Job has been started!");
            var jobState = getState(context);

            jobState.Update("Построение списка файлов");
            await Task.Delay(5000);

            jobState.isBytes = false;
            var total = rnd.Next(50, 250);
            jobState.Progress(0, total);
            for (int i = 0; i < total; i++)
            {
                jobState.Update($"Архивация файла Test{i}.dat", false);
                jobState.Progress(i, total);
                await Task.Delay(50);
            }


            jobState.Update("Отправка архива на сайт");
            jobState.isBytes = true;
            total = rnd.Next(15, 60) * 1024 * 1024;
            var current = 0;
            jobState.Progress(current, total);
            while (jobState.current < jobState.total)
            {
                current += rnd.Next(16 * 1024, 256 * 1024);
                jobState.Progress(current, total);
                await Task.Delay(200);
            }

            jobState.Done("Test...");
        }
    }
}
