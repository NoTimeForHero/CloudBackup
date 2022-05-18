using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EmbedIO;
using Quartz;
using Unity;
using EmbedIO.Actions;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using NLog;
using NLog.Targets;
using Quartz.Impl.Matchers;
using EmbedServer = EmbedIO.WebServer;

namespace CloudBackuper.Web
{
    public class WebServer : IDisposable
    {
        protected ILogger logger = LogManager.GetCurrentClassLogger();
        protected IShutdown shutdown;
        protected IScheduler scheduler;
        protected EmbedServer server;

        public WebServer(IUnityContainer container, string pathStaticFiles=null, bool developmentMode=false)
        {
            var config = container.Resolve<Config>();
            scheduler = container.Resolve<IScheduler>();
            shutdown = container.IsRegistered<IShutdown>() ? container.Resolve<IShutdown>() : null;

            var options = new WebServerOptions()
                .WithMode(HttpListenerMode.Microsoft)
                .WithUrlPrefix(config.HostingURI);

            var frontendSettings = new
            {
                appName = getApplicationName(),
                apiUrl = $"{config.HostingURI}/api",
                isService = shutdown == null
            };

            server = new EmbedServer(options);

            // Из-за отсутствия обработчика ошибок в EmbedIO приходится использовать такой странный способ проверки занятости префикса
            // Конкретнее: https://github.com/unosquare/embedio/blob/3.1.3/src/EmbedIO/WebServerBase%601.cs#L208
            // Проверяется только токен отмены, а все ошибки включая запуск HttpListener будут проигнорированы без всякого сообщения
            server.Listener.Start();
            server.Listener.Stop();

            if (developmentMode) server.WithCors();

            server.WithModule(nameof(WebSocketStatus), new WebSocketStatus(scheduler, "/ws-status"));

            server.WithWebApi("/api", m => m.WithController(() => new JobController(shutdown, scheduler)))
                .WithModule(new ActionModule("/settings.json", HttpVerbs.Get, ctx => ctx.SendDataAsync(frontendSettings)));

            if (pathStaticFiles != null) server.WithStaticFolder("/", pathStaticFiles, true);

            server.StateChanged += (s, e) => logger.Debug($"New State: {e.NewState}");
            server.RunAsync();
        }

        public void Dispose()
        {
            server.Dispose();
        }

        protected static string getApplicationName()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            var version = assembly.GetName().Version;
            return $"{title} {version}";
        }

        protected class JobController : WebApiController
        {
            protected MemoryTarget memoryTarget;
            protected IShutdown shutdown;
            protected IScheduler scheduler;

            public JobController(IShutdown shutdown, IScheduler scheduler)
            {
                memoryTarget = LogManager.Configuration.AllTargets.OfType<MemoryTarget>().FirstOrDefault();
                this.scheduler = scheduler;
                this.shutdown = shutdown;
            }

            [Route(HttpVerbs.Any, "/logs")]
            public IEnumerable<string> Logs()
            {
                return memoryTarget?.Logs.Reverse();
            }

            [Route(HttpVerbs.Any, "/jobs")]
            public object Index()
            {
                var states = (Dictionary<JobKey, UploadJobState>)scheduler.Context["states"];
                return states.Select(pair => new
                {
                    Key = pair.Key,
                    State = pair.Value
                });
            }

            [Route(HttpVerbs.Post, "/jobs/start/{name}")]
            public async Task<object> StartJob(string name)
            {
                var tasksDetail = (await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()))
                    .Select(key => scheduler.GetJobDetail(key));
                var details = await Task.WhenAll(tasksDetail);
                var job = details.FirstOrDefault(xJob => xJob.Key.Name == name);
                if (job != null) await scheduler.TriggerJob(job.Key);
                return job;
            }

            [Route(HttpVerbs.Delete, "/shutdown")]
            public object Shutdown()
            {
                if (shutdown == null)
                {
                    Response.StatusCode = 400;
                    return new {Error = "Службу невозможно остановить через веб-интефрейс!"};
                }

                Task.Run(async () =>
                {
                    await Task.Delay(500);
                    shutdown.Shutdown();
                });
                return new {Message = "Приложение будет остановлено через несколько секунд!"};
            }

            // На будущее, если забуду как десериализировать объекты из JSON
            /*
            [Route(HttpVerbs.Post, "/test")]
            public async Task<DataPerson> Test()
            {
                var data = await HttpContext.GetRequestDataAsync<DataPerson>();
                data.id += 25;
                data.name += " and bill";
                return data;
            }
             */

        }
    }
}
