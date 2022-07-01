using System;
using System.Collections.Generic;
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
using CloudBackuper;

namespace MaskPreview
{
    /// <summary>
    /// Логика взаимодействия для FileTreeViewer.xaml
    /// </summary>
    public partial class FileTreeViewer : UserControl
    {
        protected Func<ViewNode> callback;

        private enum State
        {
            NeedRefresh,
            DisplayData
        }

        public void Prepare(Func<ViewNode> callback)
        {
            this.callback = callback;
            Toggle(State.NeedRefresh);
        }

        private void Build()
        {
            treeRoot.Items.Clear();
            treeRoot.Items.Add(callback());
            Toggle(State.DisplayData);
            if (treeRoot.ItemContainerGenerator.ContainerFromItem(treeRoot.Items[0])
                is TreeViewItem control) control.IsExpanded = true;
        }

        private void Toggle(State state)
        {
            switch (state)
            {
                case State.NeedRefresh:
                    // treeRoot.Visibility = Visibility.Hidden;
                    panelRefresh.Visibility = Visibility.Visible;
                    return;
                case State.DisplayData:
                    // treeRoot.Visibility = Visibility.Visible;
                    panelRefresh.Visibility = Visibility.Hidden;
                    return;
            }
        }

        public FileTreeViewer()
        {
            InitializeComponent();
            btnRefresh.Click += (o, ev) => Build();
            // Toggle(State.DisplayData);
        }
    }
}
