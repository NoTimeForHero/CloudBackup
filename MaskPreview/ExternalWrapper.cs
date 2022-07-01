using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudBackuper;
using CloudBackuper.Utils;

namespace MaskPreview
{
    internal class ExternalWrapper
    {
        public static string SerializeModel(MainWindow.DataModel model)
        {
            var cfgRoot = new Config();
            var cfgJob = new Config_Job
            {
                Id = "example_job_1",
                Name = "Example Job 1",
                Path = model.Path
            };
            var cfgMask = new Config_Masks
            {
                Masks = model.Masks.ToArray(),
                DirectoriesExcluded = model.ExcludedFolders.ToArray(),
                MasksExclude = model.Inverted
            };
            cfgRoot.Jobs = new List<Config_Job> { cfgJob };
            cfgJob.Masks = cfgMask;
            return YamlTools.Serialize(cfgRoot);
        }
    }
}
