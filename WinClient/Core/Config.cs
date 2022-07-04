using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace WinClient
{
    class Config
    {
        public bool topmost { get; set; }
        [Option(shortName: 'd', longName: "debug", HelpText = "Debug mode")]
        public bool debug_mode { get; set; }
        public string urlWatch { get; set; }
        public string urlStartJob { get; set; }

        [Option(shortName: 's', longName: "shutdown", HelpText = "Interval before shutdown")]
        public TimeSpan? shutdown_computer { get; set; }

        public bool no_background { get; set; }

        /// <summary>
        /// Выйти из приложения вместо выключения компьютера
        /// [ВНИМАНИЕ] Приоритетнее параметра shutdown_computer
        /// </summary>
        [Option(shortName: 'c', longName: "close", HelpText = "Exit after complete instead shutdown")]
        public bool close_on_complete { get; set; }

        [Option(shortName: 'j', longName: "job", HelpText = "Job name to run")]
        public string run_job { get; set; }

        public static Config Default => new Config
        {
            topmost = true,
            urlWatch = "ws://localhost:3000/ws-status",
            urlStartJob = "http://localhost:3000/api/jobs/start/{0}",
            shutdown_computer = null
        };
    }
}
