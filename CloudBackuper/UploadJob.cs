using System;
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

            logger.Debug($"Задача №{jobIndex} запущена: {cfgJob.Name}");

            var filename = cfgJob.Name.ConvertToValidFilename();
            filename = DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss") + $"_{filename}.zip";

            await Task.Run(() =>
            {
                using (var line = new AppState.Line())
                {
                    var s3 = Uploader_S3.GetInstance(cfgCloud);
                    using (var zip = new ZipTools(cfgJob.Path, cfgJob.Masks))
                    {
                        zip.CreateZip((total, index, name) =>
                        {
                            var msg = $"Архивация[{index}/{total}]: {name}";
                            logger.Debug(msg);
                            line.Data = msg;
                        });
                        s3.UploadFile(zip.Filename, filename, (sender, args) =>
                        {
                            line.Data = $"Отправка №{jobIndex+1}: {args.PercentDone}%";
                        });
                    }
                    logger.Debug($"Задача №{jobIndex} завершена: {cfgJob.Name}");
                }
            });
        }
    }
}