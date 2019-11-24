﻿using System;
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

        protected UploadJobState getState(IJobExecutionContext context)
        {
            var states = (Dictionary<JobKey, UploadJobState>) context.Scheduler.Context["states"];
            return states[context.JobDetail.Key];
        } 

        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var cfgCloud = context.Scheduler.Context["cloud"] as Config_S3;
            var jobIndex = (int) dataMap["index"];
            var cfgJob = dataMap["data"] as Config_Job;

            logger.Info($"Задача №{jobIndex} запущена: {cfgJob.Name}");

            var filename = cfgJob.Name.ConvertToValidFilename();
            filename = DateTime.Now.ToString("yyyy.MM.dd/HH.mm.ss") + $"_{filename}.zip";

            logger.Debug("Архивация директории: " + cfgJob.Path);
            logger.Debug("Маски файлов: " + string.Join(", ", cfgJob.Masks.Masks));
            logger.Debug("Тип масок: " + (cfgJob.Masks.MasksExclude ? "Whitelist" : "Blacklist"));

            var jobState = getState(context);
            jobState.inProgress = true;
            jobState.status = "Построение списка файлов";

            await Task.Delay(10000);
            var files = FileUtils.GetFilesInDirectory(cfgJob.Path, cfgJob.Masks);

            var s3 = Uploader_S3.GetInstance(cfgCloud);

            using (var zip = new ZipTools(cfgJob.Path, files))
            {
                zip.CreateZip((total, index, name) =>
                {
                    jobState.status = $"Архивация файла: {name}";
                    jobState.current = index;
                    jobState.total = total;
                });

                jobState.isBytes = true;
                s3.UploadFile(zip.Filename, filename, (sender, args) =>
                {
                    jobState.status = $"Отправка архива {filename}";
                    jobState.current = args.TransferredBytes;
                    jobState.total = args.TotalBytes;
                });
            }

            jobState.done();
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

        public void done()
        {
            status = "Задача успешно завершена!";
            inProgress = false;
            isBytes = false;
            current = 0;
            total = 0;
        }
    }
}