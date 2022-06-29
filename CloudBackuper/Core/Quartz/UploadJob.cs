using System;
using System.Collections.Generic;
using System.IO;
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
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
                jobState.Done("Ошибка!");
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

            logger.Info($"Задача №{jobIndex} запущена: {cfgJob.Name} ({cfgJob.Id})");

            var jsEngine = container.Resolve<JSEngine>();

            var validFilename = cfgJob.Name.ConvertToValidFilename();
            var filename = jsEngine.getCloudFilename(validFilename) + ".zip";
            //filename = DateTime.Now.ToString("yyyy.MM.dd/HH.mm.ss") + $"_{filename}.zip";

            logger.Debug("Архивация директории: " + cfgJob.Path);
            logger.Debug("Маски файлов: " + string.Join(", ", cfgJob.Masks.Masks));
            logger.Debug("Тип масок: " + (cfgJob.Masks.MasksExclude ? "Whitelist" : "Blacklist"));

            var jobState = getState(context);

            jobState.Update("Построение списка файлов");
            var files = new List<string>();
            var nodes = FileUtils.GetFilesInDirectory(cfgJob.Path, cfgJob.Masks, flattenFiles: files);

            jobState.Update("Подключение к хранилищу");
            await uploader.Connect();

            using (var zip = new ZipTools(cfgJob.Path, files, cfgJob.Password))
            {
                var zipFilename = zip.CreateZip((total, current, name) =>
                {
                    jobState.Update($"Архивация файла: {name}", false);
                    jobState.Progress(current, total);
                });
                if (cfgJob.CopyTo != null)
                {
                    jobState.Update($"Копирование архива {filename}");
                    var targetCopy = Path.Combine(cfgJob.CopyTo, filename);
                    File.Copy(zipFilename, targetCopy, true);
                }
                jobState.Update($"Отправка архива {filename}");
                jobState.isBytes = true;
                await uploader.UploadFile(zipFilename, filename,
                    (x) => jobState.Progress(x.current, x.total));
            }

            await uploader.Disconnect();
            jobState.Done("Задача успешно завершена!");
            logger.Info($"Задача №{jobIndex} завершена: {cfgJob.Name} ({cfgJob.Id})");
        }
    }
}