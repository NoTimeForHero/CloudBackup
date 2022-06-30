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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DataModel Model { get; }
        public event Action OnModelChanged;

        public MainWindow()
        {
            InitializeComponent();
            Model = new DataModel();
            chkInvertMask.Checked += (o, ev) =>
            {
                Model.Inverted = chkInvertMask.IsChecked ?? false;
                OnModelChanged?.Invoke();
            };
            listMasks.ItemChanged += (o, ev) =>
            {
                Model.Masks = listMasks.DisplayItems;
                OnModelChanged?.Invoke();
            };
            listFolders.ItemChanged += (o, ev) =>
            {
                Model.ExcludedFolders = listFolders.DisplayItems;
                OnModelChanged?.Invoke();
            };
            folderButton.Click += (o, ev) =>
            {
                // TODO: Заменить на нормальную либу диалогов вроде Ookii.Dialogs.Wpf или WindowsAPICodePack
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.Cancel) return;
                    folderValue.Text = dialog.SelectedPath;
                    Model.Path = dialog.SelectedPath;
                    OnModelChanged?.Invoke();
                }
            };
        }

        public class DataModel
        {
            public bool Inverted { get; set; }
            public string Path { get; set; }
            public string[] Masks { get; set; }
            public string[] ExcludedFolders { get; set; }
        }
    }
}
