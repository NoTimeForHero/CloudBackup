using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WinClient
{
    class Program
    {
        public const string Title = "Загрузка резервных копий в облако";

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var config = File.Exists("config.json") ? JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json")) : Config.Default;
            var _ = new Program(config);
        }

        private Program(Config config)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MakeTrayIcon(out var menuItem);

            var status = new ObserverVariable<string>("Ожидание");
            status.Changed += value => menuItem.Text = value;

            Form form = new Form();
            var label = new Label {Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(form.Font.FontFamily, 18f, FontStyle.Bold)};
            form.Controls.Add(label);
            var watcher = new SocketClient(config, status);
            watcher.OnMessage += text =>
            {
                var json = JObject.Parse(text);
                var type = json["Type"]?.ToObject<string>();
                var message = $"Type: {type}\n";
                if (type == "ProgressUpdated")
                {
                    var states = json["States"]?.ToObject<Dictionary<string, UploadJobState>>();
                    message += string.Join("\n", states?.Select(pair => $"{pair.Key} = {pair.Value.status}") ?? new string[0]);
                }
                if (!form.IsDisposed) form.Invoke((Action)(() =>
                {
                    label.Text = message;
                }));
            };
            watcher.StartAsync();
            form.Show();

            Application.Run();
        }

        void MakeTrayIcon(out MenuItem itemStatus)
        {
            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            trayIcon.Text = Title;
            trayIcon.Visible = true;

            itemStatus = new MenuItem("Неизвестно") {Enabled = false};
            var itemAbout = new MenuItem("Авторы", (o,ev) => new FormAbout().Show());
            var itemExit = new MenuItem("Выйти", (o,ev) =>
            {
                var message = "Вы действительно хотите выйти?\n\nОстановка данного приложения НЕ отменит процесс резервного копирования!\nЗа него отвечает отдельный сервис.";
                var result = MessageBox.Show(message, "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes) Application.Exit();
            });

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
