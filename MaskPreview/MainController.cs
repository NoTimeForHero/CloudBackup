using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

            window.Model.PropertyChanged += (o, ev) => Refresh();
            window.panelZip.Initialize(wrapper.zipWrapper);
            AppDomain.CurrentDomain.UnhandledException += (o, ev) => UnhandledException(ev.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (o, ev) => UnhandledException(ev.Exception);
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
