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
            listMasks.DumpItemBinding(Model.Masks, (data) => Model.Masks = new ObservableCollection<string>(data));
            listFolders.DumpItemBinding(Model.ExcludedFolders, (data) => Model.ExcludedFolders = new ObservableCollection<string>(data));


            Model.Inverted = true;
            Model.Masks.Add("xslx");
            Model.Masks.Add("xls");
            Model.Masks.Add("xml");
            Model.ExcludedFolders.Add("TEMP");
            Model.ExcludedFolders.Add("BIN");

            folderButton.Click += (o, ev) =>
            {
                // TODO: Заменить на нормальную либу диалогов вроде Ookii.Dialogs.Wpf или WindowsAPICodePack
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.Cancel) return;
                    Model.Path = dialog.SelectedPath;
                }
            };
        }

        public class DataModel : AReactive
        {
            private bool inverted;
            private string path;
            private ObservableCollection<string> masks = new ObservableCollection<string>();
            private ObservableCollection<string> excludedFolders = new ObservableCollection<string>();

            public bool Inverted { get => inverted; set => SetField(ref inverted, value); }
            public string Path { get => path; set => SetField(ref path, value); }
            public ObservableCollection<string> Masks { get => masks; set => SetField(ref masks, value); }
            public ObservableCollection<string> ExcludedFolders { get => excludedFolders; set => SetField(ref excludedFolders, value); }
        }
    }
}

