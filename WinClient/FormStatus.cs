using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        protected List<Timer> timers = new List<Timer>();
        protected UserControl currentPanel;

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

        public FormStatus()
        {
            InitializeComponent();
            Text = Program.Title;
            btnMinimize.Click += (o, ev) => WindowState = FormWindowState.Minimized;
            btnClose.Click += (o, ev) => Close();
            lblFormTitle.Text = Text;
            lblFormTitle.MouseDown += (o, ev) => Win32.DragWindow(Handle);
            createRoundedBorder(20, 3);
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
                    TogglePanel(new PanelProgress());
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
