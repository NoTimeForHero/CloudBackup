using System.Collections.Generic;

namespace CloudBackuper
{
    public class Config
    {
        public Config_S3 Cloud { get; set; }
        public List<Config_Job> Jobs { get; set; }
    }

    public class Config_S3
    {
        public string Provider { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Container { get; set; }
        public bool ForcePathStyle { get; set; }
    }

    public class Config_Masks
    {
        public string[] DirectoriesExcluded { get; set; }
        public bool MasksExclude { get; set; }
        public string[] Masks { get; set; }
    }

    public class Config_Job
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CronSchedule { get; set; }
        public string Path { get; set; }
        public Config_Masks Masks { get; set; }

        public override string ToString()
        {
            return $"Job[Name='{Name}',Path='{Path}',CronSchedule='{CronSchedule}']";
        }
    }
}