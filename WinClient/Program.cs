using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WinClient.Api;

namespace WinClient
{
    class Program
    {
        public const string Title = "Загрузка резервных копий в облако";
        public static readonly StringBuilder Logger = new StringBuilder();

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var config = File.Exists("settings.json") ? JsonConvert.DeserializeObject<Config>(File.ReadAllText("settings.json")) : Config.Default;
            var _ = new Program(args, config);
        }

        private Program(string[] args, Config config)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var status = new ObserverVariable<string>("Ожидание");

            GuiController controller = new GuiController(config);
            var watcher = new SocketClient(config, status);

            watcher.OnMessage += controller.OnMessage;
            watcher.StartAsync();

            MakeTrayIcon(out var menuItem, out var menuDebug);
            status.Changed += value => menuItem.Text = value;

            if (config.debug_mode)
            {
                menuDebug.Visible = true;
                menuDebug.Click += (o, ev) => controller.runDebug();
            }

            if (args.Length == 2 && args[0] == "RunJob")
            {
                startJob(config.urlStartJob, args[1]).GetAwaiter().GetResult();
            }
            //Test(controller);

            Application.Run();
        }

        private async Task startJob(string url, string name)
        {
            var client = new HttpClient();
            name = WebUtility.UrlEncode(name);
            var uri = new Uri(string.Format(url, name));
            var response = await client.PostAsync(uri, new StringContent(string.Empty));
            response.EnsureSuccessStatusCode();
        }

        private static void Test(GuiController controller)
        {
            new Thread(() =>
            {
                Thread.Sleep(500);
                controller.OnMessage(new Api.Message(MessageType.Started).Json);
                Thread.Sleep(1000);
                for (int i = 0; i < 5; i++)
                {
                    var fakeApi = new Api.Message(MessageType.ProgressUpdated)
                    {
                        States = new Dictionary<string, UploadJobState> { { "Job", new UploadJobState { status = $"{i}", total = 5, current = i } } }
                    };
                    controller.OnMessage(fakeApi.Json);
                    Thread.Sleep(100);
                }
                controller.OnMessage(new Api.Message(MessageType.Completed).Json);
                Thread.Sleep(5000);
                controller.OnMessage(new Api.Message(MessageType.Started).Json);
                Thread.Sleep(2000);
                controller.OnMessage(new Api.Message(MessageType.Completed).Json);
            }).Start();
        }

        public static bool exitConfirm()
        {
            var message = "Вы действительно хотите выйти?\n\nОстановка данного приложения НЕ отменит процесс резервного копирования!\nЗа него отвечает отдельный сервис.";
            var result = MessageBox.Show(FormStatus.Instance, message, "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes) Application.Exit();
            return result == DialogResult.Yes;
            // return false;
        }

        void MakeTrayIcon(out MenuItem itemStatus, out MenuItem itemDebug)
        {
            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            trayIcon.Text = Title;
            trayIcon.Visible = true;

            itemStatus = new MenuItem("Неизвестно") {Enabled = false};
            var itemAbout = new MenuItem("Авторы", (o,ev) => new FormAbout().Show());
            var itemExit = new MenuItem("Выйти", (o,ev) => exitConfirm());
            itemDebug = new MenuItem("Отладка") { Visible = false };

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("Состояние:") { Enabled = false });
            menu.MenuItems.Add(itemStatus);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(itemDebug);
            menu.MenuItems.Add(itemAbout);
            menu.MenuItems.Add(itemExit);

            trayIcon.ContextMenu = menu;
        }
    }
}
