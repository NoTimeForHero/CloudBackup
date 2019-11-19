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

namespace CloudBackuper
{
    class TrayIcon : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private string ApplicationName => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;
        private readonly MenuItem status;
        private readonly MenuItem[] buttons;
        private readonly NotifyIcon notifyIcon;
        private readonly IUnityContainer container;

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
            menu.MenuItems.Clear();
            menu.MenuItems.Add(status);
            menu.MenuItems.AddRange(buttons);
            return menu;
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
            notifyIcon?.Dispose();
        }
    }
}
 
 