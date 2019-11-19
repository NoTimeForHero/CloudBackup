using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NLog;
using Quartz;
using Unity;

namespace CloudBackuper
{
    [DisallowConcurrentExecution]
    class UploadJob : IJob
    {
        protected static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var jobIndex = (int) dataMap["index"];
            var cfgCloud = dataMap["cloud"] as Config_S3;
            var cfgJob = dataMap["data"] as Config_Job;

            logger.Info($"Задача №{jobIndex} запущена: {cfgJob.Name}");

            var filename = cfgJob.Name.ConvertToValidFilename();
            filename = DateTime.Now.ToString("yyyy.MM.dd/HH.mm.ss") + $"_{filename}.zip";

            await Task.Run(() =>
            {
                logger.Debug("Архивация директории: " + cfgJob.Path);
                logger.Debug("Маски файлов: " + string.Join(", ", cfgJob.Masks.Masks));
                logger.Debug("Тип масок: " + (cfgJob.Masks.MasksExclude ? "Whitelist" : "Blacklist"));
                var files = FileUtils.GetFilesInDirectory(cfgJob.Path, cfgJob.Masks);

                using (var line = new AppState.Line())
                {
                    var s3 = Uploader_S3.GetInstance(cfgCloud);

                    using (var zip = new ZipTools(cfgJob.Path, files))
                    {
                        zip.CreateZip((total, index, name) =>
                        {
                            line.Data = $"Архивация[{index}/{total}]: {name}";
                        });
                        s3.UploadFile(zip.Filename, filename, (sender, args) =>
                        {
                            line.Data = $"Отправка №{jobIndex+1}: {args.PercentDone}%";
                        });
                    }
                    logger.Info($"Задача №{jobIndex} завершена: {cfgJob.Name}");
                }
            });
        }
    }
}