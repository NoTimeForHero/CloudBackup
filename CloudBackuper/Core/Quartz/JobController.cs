using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using NLog;
using Quartz;
using Quartz.Impl;
using Unity;

namespace CloudBackuper.Core.Quartz
{
    class JobController : IDisposable
    {
        protected static Dictionary<JobKey, IList<JobKey>> runAfter = new Dictionary<JobKey, IList<JobKey>>();
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        protected IUnityContainer container;
        protected IScheduler scheduler;

        public void Dispose()
        {
            scheduler.Clear();
            container?.Dispose();
        }

        public JobController(IUnityContainer container)
        {
            this.container = container;
        }

        public IList<JobKey> getJobsAfter(JobKey current) => runAfter.ContainsKey(current) ? runAfter[current] : null;

        public async Task<JobController> Constructor(Config config)
        {
            NameValueCollection props = new NameValueCollection { { "quartz.serializer.type", "binary" } };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            scheduler = await factory.GetScheduler();
            // TODO: Сделать получение зависимостей только из этого класса
            container.RegisterInstance(scheduler);
            await scheduler.Start();
            scheduler.Context["states"] = new Dictionary<JobKey, UploadJobState>();
            scheduler.Context["container"] = container;
            scheduler.Context["config"] = config;
            scheduler.Context["jobController"] = this;
            scheduler.ListenerManager.AddJobListener(new JobAfterHandler());
            if (config.JobRetrying != null)
            {
                var listener = new JobFailureHandler(config.JobRetrying.MaxRetries, config.JobRetrying.WaitSeconds * 1000);
                scheduler.ListenerManager.AddJobListener(listener);
            }

            var tasks = new List<Task>();
            int index = 0;

            var jobStates = (Dictionary<JobKey, UploadJobState>) scheduler.Context["states"];

            foreach (var cfgJog in config.Jobs)
            {
                var jobValidEx = cfgJog.Validate();
                if (jobValidEx != null) throw jobValidEx;

                var data = new JobDataMap
                {
                    ["index"] = index,
                    ["data"] = cfgJog
                };

                var job = JobBuilder.Create<UploadJob>()
                    .WithIdentity(cfgJog.Name)
                    .UsingJobData(data)
                    .StoreDurably(true)
                    .Build();

                jobStates.Add(job.Key, new UploadJobState());

                if (!string.IsNullOrEmpty(cfgJog.CronSchedule)) // Задача по расписанию Cron
                {

                    var trigger = TriggerBuilder.Create()
                        .WithIdentity(cfgJog.Name)
                        .WithCronSchedule(cfgJog.CronSchedule)
                        .StartNow()
                        .Build();

                    var task = scheduler.ScheduleJob(job, trigger);
                    tasks.Add(task);

                } else if (!string.IsNullOrEmpty(cfgJog.RunAfter)) // Задача запускаемая после другой задачи
                {
                    var afterKey = JobKey.Create(cfgJog.RunAfter);
                    if (!runAfter.ContainsKey(afterKey)) runAfter[afterKey] = new List<JobKey>();
                    runAfter[afterKey].Add(job.Key);

                    var task = scheduler.AddJob(job, true);
                    tasks.Add(task);
                }

                index++;
            }

            await Task.WhenAll(tasks);
            return this;
        }
    }
}