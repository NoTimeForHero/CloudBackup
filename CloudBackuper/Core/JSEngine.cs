using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using NLog;

namespace CloudBackuper
{
    class JSEngine
    {
        protected Engine engine;
        protected string scriptFile;
        protected readonly Logger logger = LogManager.GetCurrentClassLogger();

        public JSEngine(string scriptFile)
        {
            this.scriptFile = scriptFile;
            string script = File.ReadAllText(scriptFile);

            engine = new Engine();
            engine.Execute(script);
        }

        public string getCloudFilename(string input) 
            => execute(nameof(getCloudFilename), input).AsString();


        protected string Func_FormatDateTime(string format, string ciName=null)
        {
            CultureInfo CI = ciName == null ? CultureInfo.CurrentCulture : new CultureInfo(ciName);
            return DateTime.Now.ToString(format, CI);
        }

        protected string Func_NoSpace(string input, string replacement = null)
        {
            if (replacement == null) replacement = "_";
            return Regex.Replace(input, @"\s", replacement);
        }

        protected string Func_MD5(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] computedHash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return new System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary(computedHash).ToString();
            }
        }

        protected JsValue execute(string function, params object[] args)
        {
            engine.SetValue("datetime", DateTime.Now);
            engine.SetValue("guid", Guid.NewGuid());

            engine.SetValue("md5", (Func<string, string>)Func_MD5);
            engine.SetValue("no_space", (Func<string, string, string>)Func_NoSpace);
            engine.SetValue("debug", (Action<string>)logger.Debug);
            engine.SetValue("info", (Action<string>)logger.Info);
            engine.SetValue("warn", (Action<string>)logger.Warn);
            engine.SetValue("error", (Action<string>)((message) => throw new ApplicationException(message)));
            engine.SetValue("format_datetime", (Func<string, string, string>) Func_FormatDateTime);

            try
            {
                return engine.Invoke(function, args);
            }
            catch (Jint.Runtime.JavaScriptException ex)
            {
                throw new ExecuteException(function, $"Ошибка вызова функции \"{function}\": {ex.Message}", ex);
            }
        }


        public class ExecuteException : InvalidOperationException
        {
            public string Function { get; private set; }

            public ExecuteException(string function, string message, Exception innerException) : base(message, innerException)
            {
                Function = function;
            }
        }
    }
}
