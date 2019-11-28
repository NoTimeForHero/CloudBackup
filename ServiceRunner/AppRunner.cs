using System.ServiceProcess;
using System.Threading.Tasks;

namespace ServiceRunner
{
    public class AppRunner : ServiceBase
    {
        protected CloudBackuper.Program program;

        public AppRunner()
        {
            CanPauseAndContinue = false;
            CanStop = true;
            CanShutdown = true;
        }

        protected override void OnStart(string[] args)
        {
            program = new CloudBackuper.Program();
            Task.Run(() => program.Run(null));
        }

        protected override void OnStop()
        {
            program.Shutdown();
            program.Dispose();
        }
    }
}
