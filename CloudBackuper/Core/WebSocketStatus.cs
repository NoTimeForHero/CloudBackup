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
using Unity;

namespace CloudBackuper
{
    class WebSocketStatus : WebSocketModule, IJobListener
    {
        protected readonly IScheduler scheduler;
        protected readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected readonly CancellationTokenSource tokenSource;
        protected readonly int updateInterval;
        protected Dictionary<JobKey, UploadJobState> jobStates;

        protected CancellationTokenSource checkRunningJobsTokenSource;

        public WebSocketStatus(IUnityContainer container, string urlPath, CancellationTokenSource tokenSource = null, int updateIntervalMs = 300) : base(urlPath, true)
        {
            logger.Info($"Запущен WebSocket сервер по пути '{urlPath}' с интервалом обновления {updateIntervalMs} миллисекунд");
            if (tokenSource == null) tokenSource = new CancellationTokenSource();
            updateInterval = updateIntervalMs;
            this.tokenSource = tokenSource;
            scheduler = container.Resolve<IScheduler>();
            scheduler.ListenerManager.AddJobListener(this);
        }

        protected override void OnStart(CancellationToken cancellationToken)
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
            await BroadcastProgress(Caller.EventLoop);
        }

        protected void BroadcastIfAllJobsCompleted()
        {
            checkRunningJobsTokenSource?.Cancel();
            checkRunningJobsTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    var delay = Math.Min(2000, updateInterval * 3);
                    await Task.Delay(delay, checkRunningJobsTokenSource.Token);
                    var jobs = await scheduler.GetCurrentlyExecutingJobs(tokenSource.Token);
                    if (jobs.Count > 0) return;
                    await BroadcastAsync(new Message(MessageType.Completed).Json);
                }
                catch (TaskCanceledException)
                {
                    logger.Debug("Задача проверки завершенности была отменена новой задачей!");
                }
            }, checkRunningJobsTokenSource.Token);
        }

        protected async Task BroadcastProgress(Caller caller)
        {
            var jobs = await scheduler.GetCurrentlyExecutingJobs(tokenSource.Token);
            if (jobs.Count == 0) return;
            var states = new Dictionary<string, UploadJobState>();
            foreach (var job in jobs)
            {
                var key = job.JobDetail.Key;
                if (!jobStates.ContainsKey(key)) continue;
                states[key.Name] = jobStates[key];
            }
            await BroadcastAsync(new Message(MessageType.ProgressUpdated){States=states}.Json);
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

        public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            await BroadcastAsync(new Message(MessageType.Started){Name=context.JobDetail.Key.Name}.Json);
            await BroadcastProgress(Caller.Started);
        }

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
            => BroadcastProgress(Caller.Vetoed);

        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = new CancellationToken())
        {
            await BroadcastProgress(Caller.Completed);
            BroadcastIfAllJobsCompleted();
        }

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

        protected enum MessageType
        {
            Started,
            Completed,
            ProgressUpdated
        }

        protected class Message
        {
            public MessageType Type { get; set; }
            public string Name { get; set; }
            public IDictionary<string,UploadJobState> States { get; set; }

            public Message(MessageType Type) => this.Type = Type;

            [JsonIgnore]
            public string Json => JsonConvert.SerializeObject(this);
        }
    }
}
