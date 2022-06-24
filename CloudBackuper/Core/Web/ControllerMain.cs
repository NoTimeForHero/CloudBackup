// File: ControllerMain.cs
// Created by NoTimeForHero, 2022
// Distributed under the Apache License 2.0

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using NLog;
using NLog.Targets;
using Quartz;
using Quartz.Impl.Matchers;
using Unity;

namespace CloudBackuper.Web
{
    class ControllerMain : WebApiController
    {
        protected MemoryTarget memoryTarget;
        protected Program program;
        protected IScheduler scheduler;
        private PluginManager pm;

        public ControllerMain(IUnityContainer container)
        {
            memoryTarget = LogManager.Configuration.AllTargets.OfType<MemoryTarget>().FirstOrDefault();
            scheduler = container.Resolve<IScheduler>();
            program = Program.Instance;
            pm = container.Resolve<PluginManager>();
        }

        [Route(HttpVerbs.Any, "/logs")]
        public IEnumerable<string> Logs()
        {
            return memoryTarget?.Logs.Reverse();
        }

        [Route(HttpVerbs.Any, "/plugins")]
        public IEnumerable<object> Plugins() => pm.Plugins;

        [Route(HttpVerbs.Any, "/jobs")]
        public object Index()
        {
            var states = (Dictionary<JobKey, UploadJobState>)scheduler.Context["states"];
            return states.ToDictionary(x => x.Key.Name, x => x.Value);
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

        [Route(HttpVerbs.Get, "/reload")]
        public async Task Reload()
        {
            await HttpContext.SendDataAsync(new { Message = "Конфиг приложения будет перезагружен!" });
            await Task.Delay(500);
            await program.Reload(true);
        }

        [Route(HttpVerbs.Delete, "/shutdown")]
        public async Task Shutdown()
        {
            if (program.IsService)
            {
                Response.StatusCode = 400;
                await HttpContext.SendDataAsync(new { Error = "Службу невозможно остановить через веб-интефрейс!" });
                return;
            }

            await HttpContext.SendDataAsync(new { Message = "Приложение будет остановлено через несколько секунд!" });
            await Task.Delay(500);
            program.Shutdown();
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