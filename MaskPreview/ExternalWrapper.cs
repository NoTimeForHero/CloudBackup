using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public static ViewNode Prepare(MainWindow.DataModel model)
        {
            if (model.Path == null) return null;
            var cfgMask = new Config_Masks
            {
                Masks = model.Masks.ToArray(),
                DirectoriesExcluded = model.ExcludedFolders.ToArray(),
                MasksExclude = model.Inverted
            };
            var node = FileUtils.GetFilesInDirectory(model.Path, cfgMask);
            return Traverse(node);
        }

        private static ViewNode Traverse(FileUtils.Node input)
        {
            var output = new ViewNode { Name = input.Name };
            var folders = input.Nodes
                .Select(Traverse);
            var files = input.Files
                .Select(name => new ViewNode { Name = name, File = true });
            var merged = Enumerable.Empty<ViewNode>().Concat(folders).Concat(files);
            output.Nodes = new ObservableCollection<ViewNode>(merged);
            return output;
        }
    }

    public class ViewNode
    {
        public string Name { get; set; }
        public bool File { get; set; }
        public ObservableCollection<ViewNode> Nodes { get; set; } = new ObservableCollection<ViewNode>();
    }
}
