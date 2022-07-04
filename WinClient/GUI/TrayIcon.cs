using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinClient.GUI
{
    internal class TrayIcon
    {
        public MenuItem ItemStatus { get; }
        public MenuItem ItemAbout { get; }
        public MenuItem ItemExit { get; }
        public MenuItem ItemDebug { get; }

        public ContextMenu menu;
        private bool IsRunning;

        public TrayIcon()
        {

            ItemStatus = new MenuItem("Неизвестно") { Enabled = false };
            ItemAbout = new MenuItem("Авторы");
            ItemExit = new MenuItem("Выйти");
            ItemDebug = new MenuItem("Отладка") { Visible = false };

            menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("Состояние:") { Enabled = false });
            menu.MenuItems.Add(ItemStatus);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(ItemDebug);
            menu.MenuItems.Add(ItemAbout);
            menu.MenuItems.Add(ItemExit);
        }

        public TrayIcon Run()
        {
            if (IsRunning) return this;
            IsRunning = true;

            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            trayIcon.Text = Program.Title;
            trayIcon.Visible = true;

            trayIcon.ContextMenu = menu;
            return this;
        }
    }
}
