using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        static void Main()
        {
            var config = File.Exists("settings.json") ? JsonConvert.DeserializeObject<Config>(File.ReadAllText("settings.json")) : Config.Default;
            var _ = new Program(config);
        }

        private Program(Config config)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MakeTrayIcon(out var menuItem);

            var status = new ObserverVariable<string>("Ожидание");
            status.Changed += value => menuItem.Text = value;

            GuiController controller = new GuiController(config.debug_mode);
            var watcher = new SocketClient(config, status);

            watcher.OnMessage += controller.OnMessage;
            watcher.StartAsync();

            Application.Run();
        }

        private static void Test(GuiController controller)
        {
            new Thread(() =>
            {
                controller.OnMessage(new Api.Message(MessageType.Started).Json);
                Thread.Sleep(2000);
                for (int i = 0; i < 10; i++)
                {
                    if (i == 4) controller.OnMessage(new Api.Message(MessageType.Started).Json);
                    var fakeApi = new Api.Message(MessageType.ProgressUpdated)
                    {
                        States = new Dictionary<string, UploadJobState> { { "Job", new UploadJobState { status = $"{i}" } } }
                    };
                    controller.OnMessage(fakeApi.Json);
                    Thread.Sleep(800);
                }
                controller.OnMessage(new Api.Message(MessageType.Completed).Json);
            }).Start();
        }

        public static bool exitConfirm()
        {
            var message = "Вы действительно хотите выйти?\n\nОстановка данного приложения НЕ отменит процесс резервного копирования!\nЗа него отвечает отдельный сервис.";
            var result = MessageBox.Show(message, "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes) Application.Exit();
            return result == DialogResult.Yes;
        }

        void MakeTrayIcon(out MenuItem itemStatus)
        {
            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            trayIcon.Text = Title;
            trayIcon.Visible = true;

            itemStatus = new MenuItem("Неизвестно") {Enabled = false};
            var itemAbout = new MenuItem("Авторы", (o,ev) => new FormAbout().Show());
            var itemExit = new MenuItem("Выйти", (o,ev) => exitConfirm());

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("Состояние:") { Enabled = false });
            menu.MenuItems.Add(itemStatus);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(itemAbout);
            menu.MenuItems.Add(itemExit);

            trayIcon.ContextMenu = menu;
        }
    }
}
