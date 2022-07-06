using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using WinClient.Api;

namespace WinClient
{
    partial class FormStatus : Form
    {
        public static FormStatus Instance { get; private set; }
        protected readonly List<Timer> timers = new List<Timer>();
        protected UserControl currentPanel;

        protected readonly Config config;
        protected readonly bool debugMode;
        protected readonly TimeSpan? configRemain;
        protected TimeSpan remain;

        protected override void SetVisibleCore(bool value)
        {
            if (!IsHandleCreated)
            {
                CreateHandle();
                value = false;
            }

            if (value) timers.ForEach(timer => timer.Start());
            else timers.ForEach(timer => timer.Stop());

            base.SetVisibleCore(value);
        }

        public FormStatus(Config config)
        {
            this.config = config;
            InitializeComponent();
            Instance = this;
            Text = Program.Title;
            btnMinimize.Click += (o, ev) => WindowState = FormWindowState.Minimized;
            btnClose.Click += (o, ev) =>
            {
                if (config.no_background) Close();
                else Program.exitConfirm();
            };
            lblFormTitle.Text = Text;
            lblFormTitle.MouseDown += (o, ev) => Win32.DragWindow(Handle);
            createRoundedBorder(20, 3);

            debugMode = config.debug_mode;
            if (!config.close_on_complete) configRemain = config.shutdown_computer;

            if (configRemain.HasValue)
            {
                Timer timer = new Timer();
                timer.Interval = 1000;
                timer.Tick += (o,ev) => Tick(timer);
                timers.Add(timer);
            }
        }

        private void Tick(Timer timer)
        {
            if (!(currentPanel is PanelCompleted panel)) return;
            panel.lblTimer.Text = remain.ToString();
            if (remain.Seconds >= 0)
            {
                remain = remain.Subtract(TimeSpan.FromSeconds(1));
                return;
            }
            remain = TimeSpan.MaxValue;
            timer.Stop();
            if (debugMode)
            {
                MessageBox.Show("Произошло выключение компьютера!", "Режим отладки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Process.Start(new ProcessStartInfo("shutdown", "/s /t 0") { CreateNoWindow = true, UseShellExecute = false });
            }
            Application.Exit();
        }

        private void createRoundedBorder(int roundSize, int innerPadding)
        {
            Region = Region.FromHrgn(Win32.CreateRoundRectRgn(0, 0, Width, Height, roundSize, roundSize));
            layerMain.Padding = new Padding(innerPadding - 1);
            layerMain.Region = Region.FromHrgn(Win32.CreateRoundRectRgn(innerPadding, innerPadding, layerMain.Width - innerPadding, layerMain.Height - innerPadding, roundSize, roundSize));
        }

        public void TogglePanel(UserControl newPanel)
        {
            if (currentPanel != null)
            {
                panelBody.Controls.Remove(currentPanel);
                currentPanel.Dispose();
            }
            currentPanel = newPanel;
            panelBody.Controls.Add(newPanel);
        }

        public void Update(Api.Message message)
        {
            switch (message.Type)
            {
                case MessageType.Started:
                    TogglePanel(new PanelProgress());
                    Show();
                    return;
                case MessageType.Completed:
                    var completedPanel = new PanelCompleted(configRemain.HasValue);
                    completedPanel.btnClose.Click += (o, ev) => Hide();
                    if (configRemain.HasValue)
                    {
                        remain = configRemain.Value;
                        completedPanel.lblTimer.Text = remain.ToString();
                    }
                    TogglePanel(completedPanel);
                    if (config.close_on_complete) Application.Exit();
                    return;
            }

            if (message.Type == MessageType.ProgressUpdated && currentPanel is PanelProgress pnlProg)
            {
                var state = message.States.Values.FirstOrDefault();
                if (state == null) return;
                if (!state.isBytes)
                {
                    pnlProg.lblProgress.Text = $"{state.current} / {state.total}";
                }
                else
                {
                    pnlProg.lblProgress.Text = Win32.BytesToString(state.current) + " / " + Win32.BytesToString(state.total);
                }
                pnlProg.lblStatus.Text = state.status;
                if (state.current > state.total) state.total = state.current;
                pnlProg.progressBar1.Maximum = (int) state.total;
                pnlProg.progressBar1.Value = (int) state.current;
            }
        }
    }
}
