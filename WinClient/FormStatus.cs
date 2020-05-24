using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WinClient
{
    public partial class FormStatus : Form
    {
        public FormStatus()
        {
            InitializeComponent();
            Text = "Загрузка резервных копий в облако";
            btnMinimize.Click += (o, ev) => WindowState = FormWindowState.Minimized;
            btnClose.Click += (o, ev) => Close();
            lblFormTitle.MouseDown += (o, ev) => Win32.DragWindow(Handle);
            createRoundedBorder(20, 3);

            PanelProgress panel = new PanelProgress();
            panelBody.Controls.Add(panel);
        }

        private void createRoundedBorder(int roundSize, int innerPadding)
        {
            Region = Region.FromHrgn(Win32.CreateRoundRectRgn(0, 0, Width, Height, roundSize, roundSize));
            layerMain.Padding = new Padding(innerPadding - 1);
            layerMain.Region = Region.FromHrgn(Win32.CreateRoundRectRgn(innerPadding, innerPadding, layerMain.Width - innerPadding, layerMain.Height - innerPadding, roundSize, roundSize));
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
