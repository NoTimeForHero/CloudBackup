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
        private MainWindow.DataModel model;
        public ZipWrapper zipWrapper { get; }

        public ExternalWrapper(MainWindow.DataModel model)
        {
            this.model = model;
            zipWrapper = new ZipWrapper(model);
        }

        public string SerializeModel()
        {
            var cfgRoot = new Config();
            var cfgJob = new Config_Job
            {
                Id = "example_job_1",
                Name = "Example Job 1",
                Path = model.Path
            };
            cfgRoot.Jobs = new List<Config_Job> { cfgJob };
            cfgJob.Masks = GetMasks();
            return YamlTools.Serialize(cfgRoot);
        }

        public Lazy<ViewNode> GetFiles()
        {
            if (model.Path == null) return null;
            var path = model.Path;
            var cfgMask = GetMasks();
            return new Lazy<ViewNode>(() =>
            {
                var node = FileUtils.GetFilesInDirectory(path, cfgMask);
                return Traverse(node);
            });
        }

        protected Config_Masks GetMasks() =>
            new Config_Masks
            {
                Masks = model.Masks.ToArray(),
                DirectoriesExcluded = model.ExcludedFolders.ToArray(),
                MasksExclude = model.Inverted
            };

        private static ViewNode Traverse(FileUtils.Node input)
        {
            var output = new ViewNode { Name = input.Name };
            var folders = input.Nodes
                .Select(Traverse)
                .Where(x => x != null);
            var files = input.Files
                .Select(name => new ViewNode { Name = name, File = true });
            var merged = Enumerable.Empty<ViewNode>().Concat(folders).Concat(files).ToArray();
            if (merged.Length == 0) return null;
            output.Nodes = new ObservableCollection<ViewNode>(merged);
            return output;
        }
    }

    internal class ZipWrapper : IDisposable
    {
        public const string defaultPassword = "example1234";
        private readonly MainWindow.DataModel model;
        private string filename;
        private ZipTools zip;

        public delegate void DelegateOnProgress(string state, int current, int total);
        public delegate void DelegateOnComplete(string filename);

        public event DelegateOnProgress OnProgress;
        public event DelegateOnComplete OnComplete;

        internal ZipWrapper(MainWindow.DataModel model)
        {
            this.model = model;
        }

        public async void Run()
        {
            zip?.Dispose();
            await Task.Factory.StartNew(RunData, TaskCreationOptions.LongRunning);
        }

        private void RunData()
        {
            var path = model.Path;
            var mask = new Config_Masks
            {
                Masks = model.Masks.ToArray(),
                DirectoriesExcluded = model.ExcludedFolders.ToArray(),
                MasksExclude = model.Inverted
            };
            var files = new List<string>();
            FileUtils.GetFilesInDirectory(path, mask, flattenFiles: files);
            filename = ZipTools.RandomFilename(prefix: "MaskPreview_");

            zip = new ZipTools(path, files, defaultPassword);
            zip.CreateZip((total, current, name) =>
                OnProgress?.Invoke($"Архивация файла: {name}", current, total), filename);
            OnComplete?.Invoke(filename);
        }

        public void Dispose()
        {
            zip.Dispose();
        }
    }

    public class ViewNode
    {
        public string Name { get; set; }
        public bool File { get; set; }
        public ObservableCollection<ViewNode> Nodes { get; set; } = new ObservableCollection<ViewNode>();
    }
}
