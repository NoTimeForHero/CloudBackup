using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using Quartz;
using Unity;
using EmbedIO.Actions;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using NLog;
using EmbedServer = EmbedIO.WebServer;

namespace CloudBackuper.Web
{
    public class WebServer : IDisposable
    {
        protected ILogger logger = LogManager.GetCurrentClassLogger();
        protected IScheduler scheduler;
        protected EmbedServer server;

        public WebServer(IUnityContainer container)
        {
            var config = container.Resolve<Config>();
            scheduler = container.Resolve<IScheduler>();

            var options = new WebServerOptions()
                .WithMode(HttpListenerMode.EmbedIO)
                .WithUrlPrefix(config.HostingURI);

            server = new EmbedServer(options)
                .WithWebApi("/api", m => m.WithController<TestController>())
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new {Message = "Error"})));

            server.StateChanged += (s, e) => logger.Debug($"New State: {e.NewState}");

            server.Start();
        }

        public void Dispose()
        {
            server.Dispose();
        }

        protected class TestController : WebApiController
        {
            [Route(HttpVerbs.Any, "/")]
            public object Index()
            {
                return new {Message = "Hello world!"};
            }

            [Route(HttpVerbs.Post, "/test")]
            public async Task<DataPerson> Test()
            {
                var data = await HttpContext.GetRequestDataAsync<DataPerson>();
                data.id += 25;
                data.name += " and bill";
                return data;
            }

            public class DataPerson
            {
                public int id { get; set; }
                public string name { get; set; }
            }
        }
    }
}
