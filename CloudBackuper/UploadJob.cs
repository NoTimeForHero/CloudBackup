using System;
using System.Threading.Tasks;
using NLog;
using Quartz;

namespace CloudBackuper
{
    class UploadJob : IJob
    {
        protected static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var cfgCloud = dataMap["cloud"] as Config_S3;
            var cfgJob = dataMap["data"] as Config_Job;

            logger.Debug($"Выполняется задача: {cfgJob.Name}");

            var filename = cfgJob.Name.ConvertToValidFilename();
            filename = DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss") + $"_{filename}.zip";

            await Task.Run(() =>
            {
                var s3 = Uploader_S3.GetInstance(cfgCloud);
                using (var zip = new ZipTools(cfgJob.Path, cfgJob.Masks))
                {
                    s3.UploadFile(zip.Filename, filename);
                }
            });
        }
    }
}