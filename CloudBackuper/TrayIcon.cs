﻿using System;
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
        private readonly NotifyIcon notifyIcon;
        private string ApplicationName => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;
        private IUnityContainer container;

        /// <summary>Строка состояния, под названием программы, обновляемая каждые 500мс</summary>
        public string Status;

        public TrayIcon(IUnityContainer container)
        {
            this.container = container;
            notifyIcon = new NotifyIcon();
            notifyIcon.ContextMenu = GetContextMenu();
            notifyIcon.Text = ApplicationName;
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            notifyIcon.Visible = true;
        }

        protected ContextMenu GetContextMenu()
        {
            var menu = new ContextMenu();

            var lblInfo1 = new MenuItem(getInfo()) { Enabled = false };
            menu.MenuItems.Add(lblInfo1);

            // Индикатор состояния, обновляемый по таймеру
            MenuItem itemInfo = new MenuItem();
            menu.MenuItems.Add(itemInfo);

            Timer timer = new Timer();
            timer.Tick += (o, ev) => itemInfo.Text = Status;
            timer.Interval = 500;
            timer.Start();

            // Разделитель
            menu.MenuItems.Add("-");

            var btnForceRun = new MenuItem("Принудительный запуск");
            btnForceRun.Click += btnForceRunClick;
            menu.MenuItems.Add(btnForceRun);

            var btnOpen = new MenuItem("Папка с программой");
            btnOpen.Click += btnOpenClick;
            menu.MenuItems.Add(btnOpen);

            var btnAbout = new MenuItem("Об авторах");
            btnAbout.Click += btnAboutClick;
            menu.MenuItems.Add(btnAbout);

            var btnExit = new MenuItem("Выход из программы");
            btnExit.Click += (o, ev) => Application.Exit();
            menu.MenuItems.Add(btnExit);

            return menu;
        }

        private string getInfo()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{ApplicationName} [{version}]";
        }

        private void btnForceRunClick(object sender, EventArgs e)
        {
            var jobController = container.Resolve<JobController>();
            MessageBox.Show("Все задачи резервного копирования были принудительно запущены!",
                "Оповещение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //jobController.ForceRunJobs();
        }

        private void btnAboutClick(object sender, EventArgs e)
        {
            string text = "Разработчик программы - NoTimeForHero";
            text += "\nhttps://github.com/NoTimeForHero";
            text += "\n\nАвтор иконки: Carlo Rodríguez, iconfinder.com";

            MessageBox.Show(text, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnOpenClick(object sender, EventArgs e)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process.Start("explorer", path);
        }

        public void Dispose()
        {
            notifyIcon?.Dispose();
        }
    }
}