using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Unity;

namespace CloudBackuper.Core.Quartz
{
    public class JobController : IDisposable
    {
        protected Dictionary<JobKey, IList<JobKey>> runAfter = new Dictionary<JobKey, IList<JobKey>>();
        protected Dictionary<JobKey, Config_Job> configs = new Dictionary<JobKey, Config_Job>();
        protected Dictionary<JobKey, ITrigger> triggers = new Dictionary<JobKey, ITrigger>();
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        protected IUnityContainer container;
        protected IScheduler scheduler;
        public IScheduler Scheduler => scheduler;

        public void Dispose()
        {
            scheduler.Clear();
            container?.Dispose();
        }

        public JobController(IUnityContainer container)
        {
            this.container = container;
        }

        public async Task runJobsAfter(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            var jobKey = context.JobDetail.Key;
            var jobsAfter = runAfter.ContainsKey(jobKey) ? runAfter[jobKey] : null;
            if (jobsAfter == null) return;
            var triggerData = context.Trigger.JobDataMap;
            if (triggerData["noRunAfter"] != null) return;
            await Task.WhenAll(jobsAfter.Select(job => scheduler.TriggerJob(job, cancellationToken)));
        }

        public async Task<object> StartJob(string name, bool noRunAfter)
        {
            var tasksDetail = (await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()))
                .Select(key => scheduler.GetJobDetail(key));
            var details = await Task.WhenAll(tasksDetail);
            var job = details.FirstOrDefault(xJob => xJob.Key.Name == name);
            if (job == null)
            {
                logger.Warn($"Не удалось найти задачу: {name}");
                return null;
            }
            var map = new JobDataMap();
            if (noRunAfter) map["noRunAfter"] = true;
            await scheduler.TriggerJob(job.Key, map);
            return job;
        }

        public Task<object> ListJobs()
        {
            // Список задач, запускаемых после текущей
            var runAfterList = configs
                .Where(x => x.Value.RunAfter != null)
                .Select(pair => new { Source = pair.Value.RunAfter, Target = pair.Key.Name })
                .GroupBy(x => x.Source, x => x.Target)
                .ToDictionary(x => x.Key, x => x.ToList());

            var states = (Dictionary<JobKey, UploadJobState>)Scheduler.Context["states"];
            var result =  states.ToDictionary(x => x.Key.Name, x =>
            {
                var job = configs[x.Key];
                var Details = new
                {
                    description = job.Description,
                    copyTo = job.CopyTo,
                    nextLaunch = triggers.SafeGet(x.Key)?.GetNextFireTimeUtc().MapPresent(off => off.UtcDateTime),
                    jobsAfter = runAfterList.SafeGet(x.Key.Name),
                    runAfter = configs[x.Key].RunAfter
                };
                return new
                {
                    job.Id,
                    job.Name,
                    Details,
                    State = x.Value
                };
            });
            return Task.FromResult((object)result);
        }

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

            foreach (var cfgJog in config.Jobs ?? Enumerable.Empty<Config_Job>())
            {
                cfgJog.Validate();

                var data = new JobDataMap
                {
                    ["index"] = index,
                    ["data"] = cfgJog
                };

                var job = JobBuilder.Create<UploadJob>()
                    .WithIdentity(cfgJog.Id)
                    .UsingJobData(data)
                    .StoreDurably(true)
                    .Build();

                configs.Add(job.Key, cfgJog);
                jobStates.Add(job.Key, new UploadJobState());

                if (!string.IsNullOrEmpty(cfgJog.CronSchedule)) // Задача по расписанию Cron
                {

                    var trigger = TriggerBuilder.Create()
                        .WithIdentity(cfgJog.Id)
                        .WithCronSchedule(cfgJog.CronSchedule)
                        .StartNow()
                        .Build();
                    triggers.Add(job.Key, trigger);

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
                else // Задача запускаемая только вручную
                {
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