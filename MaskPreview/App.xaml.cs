using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows;

namespace MaskPreview
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AssemblyResolver.New().DefaultPaths().Register();
            new MainController().Run();
        }
    }
}
