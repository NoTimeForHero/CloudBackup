using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace WinClient
{
    static class Program
    {
        public const string Title = "Загрузка резервных копий в облако";

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MakeTrayIcon();
            Application.Run();
        }

        static void MakeTrayIcon()
        {
            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            trayIcon.Text = Title;
            trayIcon.Visible = true;

            var itemStatus = new MenuItem("Ожидание");
            itemStatus.Enabled = false;

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
