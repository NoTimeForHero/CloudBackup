using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace MaskPreview
{
    internal class MainController
    {
        protected MainWindow window;
        protected ExternalWrapper wrapper;

        public MainController()
        {
            window = new MainWindow();
            wrapper = new ExternalWrapper(window.Model);

            var Model = window.Model;
            Model.Path = @"C:\Test";
            // Model.Inverted = true;
            Model.Masks.Add(".xslx");
            Model.Masks.Add(".xls");
            Model.Masks.Add(".xml");
            Model.ExcludedFolders.Add("TEMP");
            Model.ExcludedFolders.Add("BIN");

            window.comboConfigurations.onFileSelect += Combo_FileSelect;
            window.comboConfigurations.onSelect += wrapper.SetSection;
            window.Model.PropertyChanged += (o, ev) => Refresh();
            window.panelZip.Initialize(wrapper.zipWrapper);
            AppDomain.CurrentDomain.UnhandledException += (o, ev) => UnhandledException(ev.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (o, ev) => UnhandledException(ev.Exception);

            bool loaded = TryLoadConfig(@"..\config.yml");
            if (!loaded) TryLoadConfig(@"..\..\..\CloudBackuper\config.yml");
        }

        private bool TryLoadConfig(string path)
        {
            if (!File.Exists(path)) return false;
            var config = File.ReadAllText(path);
            window.comboConfigurations.TypedItems = wrapper.GetSectionsInFile(config);
            return true;
        }

        private void Combo_FileSelect()
        {
            var dialog = new OpenFileDialog();
            dialog.FileName = "config.yml";
            dialog.Filter = "Config YML|*.yml";
            dialog.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
            if (dialog.ShowDialog() != true) return;
            TryLoadConfig(dialog.FileName);
        }

        private void UnhandledException(Exception ex) => MessageBox.Show(window, ex?.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);

        private void Refresh()
        {
            window.textYML.Text = wrapper.SerializeModel();
            window.treePreview.Update(wrapper.GetFiles());
        }

        public void Run()
        {
            Refresh();
            window.ShowDialog();
        }
    }
}
