using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Quartz;
using Unity;

namespace CloudBackuper
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    class UploadJob : IJob
    {
        protected static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected UploadJobState getState(IJobExecutionContext context)
        {
            var states = (Dictionary<JobKey, UploadJobState>) context.Scheduler.Context["states"];
            return states[context.JobDetail.Key];
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await RawExecute(context);
            }
            catch (Exception exception)
            {
                var jobState = getState(context);
                // TODO: Reset job status?
                jobState.done("Ошибка!");
                logger.Error($"Задача '{context.JobDetail.Key.Name}' кинула ошибку {exception.GetType().FullName}!");
                logger.Error("Сообщение: " + exception.Message);
                logger.Error(exception.StackTrace);
                throw;
            }
        }

        protected async Task RawExecute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var container = context.Scheduler.Context["container"] as IUnityContainer;
            var uploader = container.Resolve<PluginManager>().Uploader;
            var jobIndex = (int) dataMap["index"];
            var cfgJob = dataMap["data"] as Config_Job;

            logger.Info($"Задача №{jobIndex} запущена: {cfgJob.Name}");

            var jsEngine = container.Resolve<JSEngine>();

            var validFilename = cfgJob.Name.ConvertToValidFilename();
            var filename = jsEngine.getCloudFilename(validFilename) + ".zip";
            //filename = DateTime.Now.ToString("yyyy.MM.dd/HH.mm.ss") + $"_{filename}.zip";

            logger.Debug("Архивация директории: " + cfgJob.Path);
            logger.Debug("Маски файлов: " + string.Join(", ", cfgJob.Masks.Masks));
            logger.Debug("Тип масок: " + (cfgJob.Masks.MasksExclude ? "Whitelist" : "Blacklist"));

            var jobState = getState(context);
            jobState.inProgress = true;

            jobState.status = "Построение списка файлов";
            var files = new List<string>();
            var nodes = FileUtils.GetFilesInDirectory(cfgJob.Path, cfgJob.Masks, flattenFiles: files);

            jobState.status = "Подключение к хранилищу";
            uploader.Connect();

            using (var zip = new ZipTools(cfgJob.Path, files, cfgJob.Password))
            {
                var zipFilename = zip.CreateZip((total, index, name) =>
                {
                    jobState.status = $"Архивация файла: {name}";
                    jobState.current = index;
                    jobState.total = total;
                });

                jobState.isBytes = true;
                uploader.UploadFile(zipFilename, filename, (progress) =>
                {
                    jobState.status = $"Отправка архива {filename}";
                    jobState.current = progress.current;
                    jobState.total = progress.total;
                });
            }

            uploader.Disconnect();
            jobState.done("Задача успешно завершена!");
            logger.Info($"Задача №{jobIndex} завершена: {cfgJob.Name}");
        }
    }

    public class UploadJobState
    {
        public string status { get; set; }
        public bool inProgress { get; set; }
        public bool isBytes { get; set; }

        public long current { get; set; }
        public long total { get; set; }

        public void done(string status)
        {
            this.status = status;
            inProgress = false;
            isBytes = false;
            current = 0;
            total = 0;
        }
    }
}