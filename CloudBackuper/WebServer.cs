using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrystalQuartz.Owin;
using Microsoft.Owin.Hosting;
using Owin;
using Quartz;
using Unity;

namespace CloudBackuper
{
    public class WebServer : IDisposable
    {
        protected IScheduler scheduler;
        protected IDisposable webApp;

        public WebServer(IUnityContainer container)
        {
            scheduler = container.Resolve<IScheduler>();
            var config = container.Resolve<Config>();
            webApp = WebApp.Start(config.HostingURI, Start);
        }

        protected void Start(IAppBuilder app)
        {
            app.UseCrystalQuartz(() => scheduler);
            app.Run(context =>
            {
                context.Response.Redirect("/quartz");
                return Task.CompletedTask;
            });
        }

        public void Dispose()
        {
            webApp?.Dispose();
        }
    }
}
