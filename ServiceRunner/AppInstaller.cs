using System.ComponentModel;
using System.ServiceProcess;

namespace ServiceRunner
{
    [RunInstaller(true)]
    public class AppInstaller : System.Configuration.Install.Installer
    {
        public AppInstaller()
        {
            //InitializeComponent();
            ServiceInstaller serviceInstaller = new ServiceInstaller();
            ServiceProcessInstaller processInstaller = new ServiceProcessInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = CloudBackuper.Utils.Information.Title;
            serviceInstaller.Description = CloudBackuper.Utils.Information.Description;

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
