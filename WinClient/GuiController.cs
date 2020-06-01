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
        private Form form;

        public GuiController(bool debugMode)
        {
            createForm();
            if (debugMode) createDebugForm(form);
        }

        private void createForm()
        {
            var waiter = new AutoResetEvent(false);
            new Thread(() =>
            {
                Console.WriteLine("Created GuiController thread #" + Thread.CurrentThread.ManagedThreadId);
                form = new HiddenTimersForm();
                var label = new Label
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font(form.Font.FontFamily, 18f, FontStyle.Bold)
                };
                form.FormClosing += (o, ev) =>
                {
                    if (ev.CloseReason != CloseReason.UserClosing) return;
                    ev.Cancel = true;
                    form.Hide();
                    //if (!Program.exitConfirm()) ev.Cancel = true;
                };
                form.Controls.Add(label);
                waiter.Set();
                Application.Run(form);
            }).Start();
            waiter.WaitOne();
        }

        public void createDebugForm(Form targetForm)
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
            try
            {
                form.Invoke((Action) (() =>
                {
                    if (message.Type == MessageType.Started) form.Show();
                        var label = form.Controls.OfType<Label>().First();
                    label.Text = message.Type.ToString();
                    if (message.Type == MessageType.ProgressUpdated)
                    {
                        label.Text += "\n" + message.States.FirstOrDefault().Value.status;
                    }
                }));
            }
            catch (InvalidOperationException) {}
            catch (NullReferenceException)  {}
        }
    }

    class HiddenTimersForm : Form
    {
        protected List<Timer> timers = new List<Timer>();

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
    }
}
