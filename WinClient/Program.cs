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
using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WinClient.Api;
using WinClient.Core;
using WinClient.GUI;

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
            string jobToRun = null;

            if (args.Length > 0)
            {
                var parsed = Parser.Default.ParseArguments<Config>(args);
                var cliConfig = parsed.Value;
                if (cliConfig != null)
                {
                    config.no_background = true;
                    config.shutdown_computer = cliConfig.shutdown_computer;
                    config.close_on_complete = cliConfig.close_on_complete;
                    if (cliConfig.debug_mode) config.debug_mode = true;
                    jobToRun = cliConfig.run_job;
                }
            }

            var status = new ObserverVariable<string>("Ожидание");

            GuiController controller = new GuiController(config);
            var watcher = new SocketClient(config, status);

            watcher.OnMessage += controller.OnMessage;
            watcher.StartAsync();

            var icon = new TrayIcon();
            icon.ItemAbout.Click += (o, ev) => new FormAbout().Show();
            icon.ItemExit.Click += (o, ex) => exitConfirm();

            status.Changed += value => icon.ItemStatus.Text = value;

            if (config.debug_mode)
            {
                icon.ItemDebug.Visible = true;
                icon.ItemDebug.Click += (o, ev) => TestController.Test(controller);
            }


            watcher.OnConnected += () =>
            {
                if (jobToRun == null) return;
                WebApi.startJob(config.urlStartJob, jobToRun).GetAwaiter().GetResult();
            };

            if (!config.no_background) icon.Run();
            Application.Run();
        }

        public static bool exitConfirm()
        {
            var message = "Вы действительно хотите выйти?\n\nОстановка данного приложения НЕ отменит процесс резервного копирования!\nЗа него отвечает отдельный сервис.";
            var result = MessageBox.Show(FormStatus.Instance, message, "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes) Application.Exit();
            return result == DialogResult.Yes;
            // return false;
        }
    }
}
