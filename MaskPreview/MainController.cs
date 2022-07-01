using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaskPreview
{
    internal class MainController
    {
        protected MainWindow window;

        public MainController()
        {
            window = new MainWindow();

            var Model = window.Model;
            Model.Path = @"C:\Test";
            Model.Inverted = true;
            Model.Masks.Add(".xslx");
            Model.Masks.Add(".xls");
            Model.Masks.Add(".xml");
            Model.ExcludedFolders.Add("TEMP");
            Model.ExcludedFolders.Add("BIN");

            window.Model.PropertyChanged += (o, ev) => Refresh();
        }

        private void Refresh()
        {
            window.textYML.Text = ExternalWrapper.SerializeModel(window.Model);
            window.treePreview.Prepare(() => ExternalWrapper.Prepare(window.Model));
        }

        public void Run()
        {
            Refresh();
            window.ShowDialog();
        }
    }
}
