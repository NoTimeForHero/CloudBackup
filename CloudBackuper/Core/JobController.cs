using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using Quartz;
using Unity;

namespace CloudBackuper
{
    class JobController
    {
        protected static Dictionary<JobKey, IList<JobKey>> runAfter = new Dictionary<JobKey, IList<JobKey>>();
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        protected IUnityContainer container;
        protected IScheduler scheduler;

        public JobController(IUnityContainer container)
        {
            this.container = container;
            scheduler = container.Resolve<IScheduler>();
            if (scheduler == null) throw new NullReferenceException("IScheduler is null!");
            scheduler.Context["jobController"] = this;
        }

        public IList<JobKey> getJobsAfter(JobKey current) => runAfter.ContainsKey(current) ? runAfter[current] : null;

        public async Task<JobController> Constructor(Config config)
        {
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