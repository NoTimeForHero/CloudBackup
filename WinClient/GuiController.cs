using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WinClient.Api;
using Timer = System.Windows.Forms.Timer;

namespace WinClient
{
    class GuiController
    {
        private readonly Config config;
        private FormStatus form;

        public GuiController(Config config)
        {
            this.config = config;
            createForm();
            if (config.debug_mode) runDebug();
        }

        private void createForm()
        {
            var waiter = new AutoResetEvent(false);
            new Thread(() =>
            {
                form = new FormStatus(config);
                form.FormClosing += (o, ev) =>
                {
                    if (ev.CloseReason != CloseReason.UserClosing) return;
                    ev.Cancel = true;
                    form.Hide();
                };
                waiter.Set();
                Application.Run(form);
            }).Start();
            waiter.WaitOne();
        }

        public void runDebug()
        {
            createDebugForm(form);
        }

        private void createDebugForm(Form targetForm)
        {
            new Thread(() =>
            {
                Form formControl = new Form
                {
                    Size = new Size(300, 400),
                    Text = "Управление формой состояния",
                    MinimizeBox = false,
                    MaximizeBox = false
                };

                TableLayoutPanel tbl = new TableLayoutPanel
                {
                    RowCount = 2,
                    ColumnCount = 4,
                    Dock = DockStyle.Fill
                };
                formControl.Controls.Add(tbl);

                TextBox txtLog = new TextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical
                };

                Button btnHide = new Button
                {
                    Dock = DockStyle.Fill,
                    Text = "Hide"
                };
                btnHide.Click += (o, ev) => targetForm.Invoke((Action)(() => {
                    targetForm.Hide();
                }));

                Button btnShow = new Button
                {
                    Dock = DockStyle.Fill,
                    Text = "Show"
                };
                btnShow.Click += (o, ev) => targetForm.Invoke((Action)(() => {
                    targetForm.Show();
                }));

                Button btnClear = new Button
                {
                    Dock = DockStyle.Fill,
                    Text = "Clear"
                };
                btnClear.Click += (o, ev) => {
                    Program.Logger.Clear();
                    UpdateLog();
                };

                tbl.Controls.Add(btnHide);
                tbl.Controls.Add(btnShow);
                tbl.Controls.Add(btnClear);

                tbl.Controls.Add(txtLog);
                tbl.SetRow(txtLog, 1);
                tbl.SetColumnSpan(txtLog, tbl.ColumnCount);

                Timer timerUpdateLog = new Timer();
                timerUpdateLog.Interval = 3000;
                timerUpdateLog.Tick += (o, ev) => UpdateLog();
                timerUpdateLog.Start();

                void UpdateLog()
                {
                    txtLog.Text = Program.Logger.ToString();
                }

                formControl.ShowDialog();

            }).Start();
        }

        public void OnMessage(string text)
        {
            var message = JsonConvert.DeserializeObject<Api.Message>(text);
            try  {  form.Invoke((Action) (() => form.Update(message) )); }
            catch (InvalidOperationException) {}
            catch (NullReferenceException)  {}
        }
    }
}
