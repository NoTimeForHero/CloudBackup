using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace WinClient.GUI
{
    internal class DebugForm
    {
        private Form formControl;
        private Form targetForm;

        public DebugForm(Form targetForm)
        {
            this.targetForm = targetForm;
        }

        private void Initialize(Form targetForm)
        {
            formControl = new Form
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
        }

        public void Run()
        {
            new Thread(() =>
            {
                Initialize(targetForm);
                formControl.ShowDialog();
            }).Start();
        }

    }
}
