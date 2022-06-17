using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Quartz;

namespace CloudBackuper.Utils
{
    public class JobAfterHandler : IJobListener
    {
        public string Name => "JobAfterHandler";

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            var scheduler = context.Scheduler;
            var jobController = scheduler.Context["jobController"] as JobController;
            var jobsAfter = jobController?.getJobsAfter(context.JobDetail.Key);
            if (jobsAfter != null) await Task.WhenAll(jobsAfter.Select(job => scheduler.TriggerJob(job, cancellationToken)));
        }
    }

    public class JobFailureHandler : IJobListener
    {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected const string retryKey = "retries";
        protected readonly int waitMs;
        protected readonly int maxRetries;

        public event Action retriesFailed;
        public event Action<Exception> onError;

        public JobFailureHandler(int maxRetries, int waitMs)
        {
            this.maxRetries = maxRetries;
            this.waitMs = waitMs;
        }

        public string Name => "JobFailureListener";
        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken()) => Task.CompletedTask;

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            logger.Trace("JobToBeExecuted");
            var data = context.JobDetail.JobDataMap;
            if (!data.ContainsKey(retryKey)) data[retryKey] = 0;
            data[retryKey] = (int)data[retryKey] + 1;
            return Task.CompletedTask;
        }

        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = new CancellationToken())
        {
            logger.Trace("JobWasExecuted");
            var data = context.JobDetail.JobDataMap;
            var exception = jobException?.InnerException?.InnerException;
            if (exception == null)
            {
                data[retryKey] = 0;
                var scheduler = context.Scheduler;
                var jobController = scheduler.Context["jobController"] as JobController;
                var jobsAfter = jobController?.getJobsAfter(context.JobDetail.Key);
                if (jobsAfter != null) await Task.WhenAll(jobsAfter.Select(job => scheduler.TriggerJob(job, cancellationToken)));
                return;
            }

            onError?.Invoke(exception);
            logger.Error($"Задача '{context.JobDetail.Key.Name}' кинула ошибку {exception.GetType().FullName}!");
            logger.Error("Сообщение: " + exception.Message);
            logger.Error(exception.StackTrace);

            if ((int)data[retryKey] >= maxRetries)
            {
                logger.Fatal($"Задача '{context.JobDetail.Key.Name}' не была выполнена за {maxRetries} попыток!");
                data[retryKey] = 0;
                retriesFailed?.Invoke();
                return;
            }

            var trigger = TriggerBuilder.Create()
                .WithIdentity("Retry_" + Guid.NewGuid())
                .StartAt(DateTime.Now.AddMilliseconds(waitMs))
                .Build();
            await context.Scheduler.RescheduleJob(context.Trigger.Key, trigger, cancellationToken);
        }
    }
}
