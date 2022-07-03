using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MaskPreview
{
    /// <summary>
    /// Логика взаимодействия для PanelMakeZip.xaml
    /// </summary>
    public partial class PanelMakeZip : UserControl
    {
        private ZipWrapper zipWrapper;
        protected enum State
        {
            Prepare,
            Progress,
            Completed
        }
        protected State state;
        protected string Filename;

        private void Toggle(State newState)
        {
            state = newState;
            panelPrepare.Visibility = state == State.Prepare ? Visibility.Visible : Visibility.Hidden;
            panelProgress.Visibility = state == State.Progress ? Visibility.Visible : Visibility.Hidden;
            panelComplete.Visibility = state == State.Completed ? Visibility.Visible : Visibility.Hidden;
        }

        public PanelMakeZip()
        {
            InitializeComponent();
            Toggle(State.Prepare);
        }

        internal void Initialize(ZipWrapper zipWrapper)
        {
            Toggle(State.Prepare);
            this.zipWrapper = zipWrapper;
            lblPassword.Text = ZipWrapper.defaultPassword;
            zipWrapper.OnProgress += ZipWrapper_OnProgress;
            zipWrapper.OnComplete += ZipWrapper_OnComplete;
            btnCreateZip.Click += BtnCreateZip_Click;
            btnOpenZip.Click += BtnOpenZip_Click;
            btnDeleteZip.Click += BtnDeleteZip_Click;
        }

        private void BtnDeleteZip_Click(object sender, RoutedEventArgs e)
        {
            zipWrapper.Dispose();
            Toggle(State.Prepare);
        }

        private void BtnOpenZip_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", $"/e ,/select, \"{Filename}\"");
        }

        private void ZipWrapper_OnComplete(string filename)
        {
            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                Dispatcher.Invoke(() => ZipWrapper_OnComplete(filename));
                return;
            }
            Toggle(State.Completed);
            Filename = filename;
            txtPathZip.Text = filename;
        }

        private void ZipWrapper_OnProgress(string state, int current, int total)
        {
            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                Dispatcher.Invoke(() => ZipWrapper_OnProgress(state, current, total));
                return;
            }
            progressStatus.Text = state;
            progressBarText.Content = $"{current} / {total}";
            progressBar.Value = current;
            progressBar.Maximum = total;
        }

        private void BtnCreateZip_Click(object sender, RoutedEventArgs e)
        {
            Toggle(State.Progress);
            zipWrapper.Run();
        }
    }
}
