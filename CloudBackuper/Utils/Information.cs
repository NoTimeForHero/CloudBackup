using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CloudBackuper.Utils
{
    public class Information
    {
        public const string Filename_Config = "config.yml";
        public const string Filename_Script = "scripts.js";
        public const string UserData_Name = "userdata";

        public static readonly string Title;
        public static readonly string Description;
        // private string AppPath => Path.GetDirectoryName(Assembly.GetAssembly(GetType()).Location);
        public static readonly string AppPath;
        public static readonly string UserDataPath;

        static Information()
        {
            Title = Assembly.GetAssembly(typeof(Program)).GetTitle("CloudBackup");
            Description = Assembly.GetAssembly(typeof(Program)).GetDescription("CloudBackup Description");;
            AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (AppPath == null) throw new ArgumentException("Invalid path to assembly!");
            UserDataPath = Path.Combine(AppPath, UserData_Name);
        }
    }
}
