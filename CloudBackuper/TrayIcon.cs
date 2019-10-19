using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using NLog;
using Unity;
using Timer = System.Windows.Forms.Timer;

namespace CloudBackuper
{
    class TrayIcon : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private string ApplicationName => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;
        private readonly int statusIndex = 1;
        private readonly MenuItem status;
        private readonly MenuItem[] buttons;
        private readonly NotifyIcon notifyIcon;
        private readonly IUnityContainer container;

        private Timer timer;

        public TrayIcon(IUnityContainer container)
        {
            this.container = container;
            status = buildStatus();
            buttons = buildButtons();

            notifyIcon = new NotifyIcon();
            notifyIcon.ContextMenu = GetContextMenu();
            notifyIcon.Text = ApplicationName;
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            notifyIcon.Visible = true;
        }

        protected ContextMenu GetContextMenu()
        {
            var menu = new ContextMenu();
            var state = container.Resolve<AppState>();
            var Lines = state.Status;

            bool needRebuild = false;

            Lines.CollectionChanged += (o, ev) => needRebuild = true;
            buildMenu(0);

            timer = new Timer();
            timer.Tick += updateLabels;
            timer.Interval = 100;
            timer.Start();
            return menu;

            void updateLabels(object o, EventArgs ev)
            {
                if (needRebuild)
                {
                    buildMenu(Lines.Count);
                    needRebuild = false;
                }

                for (int i = 0; i < Lines.Count; i++)
                {
                    menu.MenuItems[statusIndex + i].Text = Lines[i].Data;
                }
            }

            void buildMenu(int count)
            {
                menu.MenuItems.Clear();
                menu.MenuItems.Add(status);
                for (var i = 0; i < count; i++)
                {
                    menu.MenuItems.Add(new MenuItem { Enabled = false });
                }
                menu.MenuItems.AddRange(buttons);
            }
        }

        private MenuItem buildStatus()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var title = $"{ApplicationName} [{version}]";

            var lblInfo1 = new MenuItem(title) { Enabled = false };
            return lblInfo1;
        }

        private MenuItem[] buildButtons()
        {
            var buttons = new List<MenuItem>();

            // Разделитель
            buttons.Add(new MenuItem("-"));

            var btnForceRun = new MenuItem("Принудительный запуск");
            btnForceRun.Click += btnForceRunClick;
            buttons.Add(btnForceRun);

            var btnOpen = new MenuItem("Папка с программой");
            btnOpen.Click += btnOpenClick;
            buttons.Add(btnOpen);

            var btnAbout = new MenuItem("Об авторах");
            btnAbout.Click += btnAboutClick;
            buttons.Add(btnAbout);

            var btnExit = new MenuItem("Выход из программы");
            btnExit.Click += (o, ev) => Application.Exit();
            buttons.Add(btnExit);

            return buttons.ToArray();

            void btnForceRunClick(object sender, EventArgs e)
            {
                var jobController = container.Resolve<JobController>();
                jobController.ForceRunJobs();
                MessageBox.Show("Все задачи резервного копирования были принудительно запущены!",
                    "Оповещение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            void btnAboutClick(object sender, EventArgs e)
            {
                string text = "Разработчик программы - NoTimeForHero";
                text += "\nhttps://github.com/NoTimeForHero";
                text += "\n\nАвтор иконки: Carlo Rodríguez, iconfinder.com";

                MessageBox.Show(text, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            void btnOpenClick(object sender, EventArgs e)
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Process.Start("explorer", path);
            }
        }

        public void Dispose()
        {
            timer?.Dispose();
            notifyIcon?.Dispose();
        }
    }
}
 
 