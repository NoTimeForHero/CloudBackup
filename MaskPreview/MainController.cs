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
            window.Model.PropertyChanged += (o, ev) =>
            {
                window.textYML.Text = ExternalWrapper.SerializeModel(window.Model);
            };
        }

        public void Run()
        {
            window.textYML.Text = ExternalWrapper.SerializeModel(window.Model);
            window.ShowDialog();
        }
    }
}
