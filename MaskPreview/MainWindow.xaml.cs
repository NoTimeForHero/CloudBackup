using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CloudBackuper;
using Ookii.Dialogs.Wpf;

namespace MaskPreview
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DataModel Model { get; }

        public MainWindow()
        {
            Model = new DataModel();
            DataContext = Model;
            InitializeComponent();

            chkInvertMask.SetBinding(ToggleButton.IsCheckedProperty, nameof(DataModel.Inverted));
            folderValue.SetBinding(TextBox.TextProperty, nameof(DataModel.Path));
            listMasks.DumpItemBinding(nameof(DataModel.Masks));
            listFolders.DumpItemBinding(nameof(DataModel.ExcludedFolders));

            var cmdCopy = textYML.ContextMenu?.Items
                .OfType<MenuItem>()
                .FirstOrDefault(x => (x.Tag is string strTag) && strTag == "Copy");
            if (cmdCopy != null) cmdCopy.Click += (o, ev) => Clipboard.SetText(textYML.Text);

            folderButton.Click += (o, ev) =>
            {
                var dialog = new VistaFolderBrowserDialog { SelectedPath = Model.Path };
                if (!dialog.ShowDialog() ?? false) return;
                Model.Path = dialog.SelectedPath;
            };
        }

        public class DataModel : AReactive
        {
            private bool inverted;
            private string path;
            private List<string> masks = new List<string>();
            private List<string> excludedFolders = new List<string>();

            public bool Inverted { get => inverted; set => SetField(ref inverted, value); }
            public string Path { get => path; set => SetField(ref path, value); }
            public List<string> Masks { get => masks; set => SetField(ref masks, value); }
            public List<string> ExcludedFolders { get => excludedFolders; set => SetField(ref excludedFolders, value); }
        }
    }
}

