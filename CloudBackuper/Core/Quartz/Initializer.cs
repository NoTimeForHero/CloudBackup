using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Unity;

namespace CloudBackuper.Core.Quartz
{
    public class Initializer
    {
        public static async Task<IScheduler> GetScheduler(IUnityContainer container, Config config)
        {
            NameValueCollection props = new NameValueCollection { { "quartz.serializer.type", "binary" } };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();
            scheduler.Context["states"] = new Dictionary<JobKey, UploadJobState>();
            scheduler.Context["container"] = container;
            scheduler.Context["config"] = config;
            scheduler.ListenerManager.AddJobListener(new JobAfterHandler());
            if (config.JobRetrying != null)
            {
                var listener = new JobFailureHandler(config.JobRetrying.MaxRetries, config.JobRetrying.WaitSeconds * 1000);
                scheduler.ListenerManager.AddJobListener(listener);
            }
            return scheduler;
        }
    }
}