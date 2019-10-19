using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Unity;

namespace CloudBackuper
{
    class JobController
    {
        protected IUnityContainer container;
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        protected IScheduler scheduler;

        public JobController(IUnityContainer container)
        {
            this.container = container;
            var config = container.Resolve<Config>();
            Constructor(config);
        }

        public async void ForceRunJobs()
        {
            var tasks = new List<Task>();
            var jobs = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var job in jobs)
            {
                logger.Info($"Ручной запуск задачи '{job.Name}' из группы '{job.Group}");
                tasks.Add(scheduler.TriggerJob(job));
            }
            await Task.WhenAll(tasks);
        }

        protected async void Constructor(Config config)
        {
            scheduler = await InitSheduler();
            var tasks = new List<Task>();

            foreach (var cfgJog in config.Jobs)
            {
                var data = new JobDataMap
                {
                    ["cloud"] = config.Cloud,
                    ["data"] = cfgJog
                };

                var job = JobBuilder.Create<UploadJob>()
                    .WithIdentity(cfgJog.Name)
                    .UsingJobData(data)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity(cfgJog.Name)
                    //.WithSimpleSchedule(x => x.WithIntervalInSeconds(40).RepeatForever())
                    //.WithCronSchedule("0 * * * * ?")
                    .WithCronSchedule(cfgJog.CronSchedule)
                    .StartNow()
                    .Build();

                var task = scheduler.ScheduleJob(job, trigger);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }

        protected async Task<IScheduler> InitSheduler()
        {
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            IScheduler sched = await factory.GetScheduler();
            await sched.Start();
            return sched;
        }
    }
}