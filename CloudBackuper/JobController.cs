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
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        protected IUnityContainer container;
        protected IScheduler scheduler;

        public JobController(IUnityContainer container)
        {
            this.container = container;
            var config = container.Resolve<Config>();
            scheduler = container.Resolve<IScheduler>();
            if (scheduler == null) throw new NullReferenceException("IScheduler is null!");
            Constructor(config);
        }

        protected async void Constructor(Config config)
        {
            var tasks = new List<Task>();
            int index = 0;

            var jobStates = (Dictionary<JobKey, UploadJobState>) scheduler.Context["states"];

            foreach (var cfgJog in config.Jobs)
            {
                var data = new JobDataMap
                {
                    ["index"] = index,
                    ["data"] = cfgJog
                };

                var job = JobBuilder.Create<UploadJob>()
                    .WithIdentity(cfgJog.Name)
                    .UsingJobData(data)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity(cfgJog.Name)
                    .WithCronSchedule(cfgJog.CronSchedule)
                    .StartNow()
                    .Build();

                jobStates.Add(job.Key, new UploadJobState());

                var task = scheduler.ScheduleJob(job, trigger);
                tasks.Add(task);
                index++;
            }

            await Task.WhenAll(tasks);
        }

    }
}