using System;
using System.Reflection;
using System.Threading;
using EmbedIO;
using Unity;
using EmbedIO.Actions;
using EmbedIO.WebApi;
using NLog;
using EmbedServer = EmbedIO.WebServer;

namespace CloudBackuper.Web
{
    public class WebServer : IDisposable
    {
        protected CancellationTokenSource tokenSource;
        protected ILogger logger = LogManager.GetCurrentClassLogger();
        protected Program program;
        protected EmbedServer server;

        public WebServer(IUnityContainer container, string pathStaticFiles=null, bool developmentMode=false)
        {
            logger.Debug($"Путь до папки со статикой: {pathStaticFiles}");
            var config = container.Resolve<Config>();
            program = Program.Instance;

            var options = new WebServerOptions()
                .WithMode(HttpListenerMode.Microsoft)
                .WithUrlPrefix(config.HostingURI);

            var frontendSettings = new
            {
                appName = getApplicationName(),
                apiUrl = $"{config.HostingURI}/api",
                isService = program.IsService
            };

            server = new EmbedServer(options);

            // Из-за отсутствия обработчика ошибок в EmbedIO приходится использовать такой странный способ проверки занятости префикса
            // Конкретнее: https://github.com/unosquare/embedio/blob/3.1.3/src/EmbedIO/WebServerBase%601.cs#L208
            // Проверяется только токен отмены, а все ошибки включая запуск HttpListener будут проигнорированы без всякого сообщения
            server.Listener.Start();
            server.Listener.Stop();

            if (developmentMode) server.WithCors();

            // TODO: Вынести в settings.json путь к /ws-status
            server.WithModule(nameof(WebSocketStatus), new WebSocketStatus(container, "/ws-status"));

            server.WithWebApi("/api", m => m.WithController(() => new ControllerMain(container)))
                .WithModule(new ActionModule("/settings.json", HttpVerbs.Get, ctx => ctx.SendDataAsync(frontendSettings)));

            if (pathStaticFiles != null) server.WithStaticFolder("/", pathStaticFiles, true);

            server.StateChanged += (s, e) => logger.Debug($"New State: {e.NewState}");
            tokenSource = new CancellationTokenSource();
            server.RunAsync(tokenSource.Token);
        }

        public void Dispose()
        {
            tokenSource.Cancel();
            server.Dispose();
        }

        protected static string getApplicationName()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            var version = assembly.GetName().Version;
            return $"{title} {version}";
        }
    }
}
