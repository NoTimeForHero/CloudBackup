using System.ServiceProcess;

namespace ServiceRunner
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
            ServiceBase.Run(new ServiceBase[] {
                new AppRunner()
            });
        }
    }
}
