using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmbedIO.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using Quartz;

namespace CloudBackuper
{
    class WebSocketStatus : WebSocketModule, IJobListener
    {
        protected readonly IScheduler scheduler;
        protected readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected readonly CancellationTokenSource tokenSource;
        protected readonly int updateInterval;
        protected volatile bool allJobsCompleted;
        protected Dictionary<JobKey, UploadJobState> jobStates;

        public WebSocketStatus(IScheduler scheduler, string urlPath, CancellationTokenSource tokenSource = null, int updateIntervalMs = 300) : base(urlPath, true)
        {
            logger.Info($"Запущен WebSocket сервер по пути '{urlPath}' с интервалом обновления {updateIntervalMs} миллисекунд");
            if (tokenSource == null) tokenSource = new CancellationTokenSource();
            updateInterval = updateIntervalMs;
            this.tokenSource = tokenSource;
            this.scheduler = scheduler;
            scheduler.ListenerManager.AddJobListener(this);
        }

        public void RunLoop()
        {
            jobStates = scheduler.Context["states"] as Dictionary<JobKey, UploadJobState>;
            Task.Factory.StartNew(async () =>
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    var _ = EventLoop();
                    await Task.Delay(updateInterval, tokenSource.Token);
                }
            }, TaskCreationOptions.LongRunning); // LongRunning - означает что для Задачи по возможности будет выделен отдельный поток
        }

        protected async Task EventLoop()
        {
            if (allJobsCompleted)
            {
                allJobsCompleted = false;
                await BroadcastAsync(JsonConvert.SerializeObject(new { Type = "Completed" }));
                return;
            }

            await BroadcastProgress(Caller.EventLoop);
        }

        protected async Task BroadcastProgress(Caller caller)
        {
            var jobs = await scheduler.GetCurrentlyExecutingJobs(tokenSource.Token);
            if (jobs.Count == 1 && caller == Caller.Completed)
            {
                await Task.Delay(updateInterval, tokenSource.Token);
                allJobsCompleted = true; // Выполнить задачу обновления в следующем тике
                return;
            }
            if (jobs.Count == 0) return;
            var states = new Dictionary<string, UploadJobState>();
            foreach (var job in jobs)
            {
                var key = job.JobDetail.Key;
                if (!jobStates.ContainsKey(key)) continue;
                states[key.Name] = jobStates[key];
            }
            var result = new { Type = "ProgressUpdated", States = states };
            await BroadcastAsync(JsonConvert.SerializeObject(result));
        }

        protected override Task OnClientConnectedAsync(IWebSocketContext context)
        {
            logger.Debug($"Подключился новый клиент: {context.Id}");
            return base.OnClientConnectedAsync(context);
        }

        protected override Task OnClientDisconnectedAsync(IWebSocketContext context)
        {
            logger.Debug($"Отключился клиент: {context.Id}");
            return base.OnClientDisconnectedAsync(context);
        }

        protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result) => Task.CompletedTask;

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
            => BroadcastProgress(Caller.Started);

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
            => BroadcastProgress(Caller.Vetoed);

        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = new CancellationToken())
            => await BroadcastProgress(Caller.Completed);

        protected override void Dispose(bool disposing)
        {
            tokenSource.Cancel();
            base.Dispose(disposing);
        }

        public string Name => nameof(WebSocketStatus);

        protected enum Caller
        {
            EventLoop,
            Started,
            Vetoed,
            Completed
        }
    }
}
