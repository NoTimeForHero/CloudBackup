using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime;
using Newtonsoft.Json.Linq;
using NLog;

namespace CloudBackuper.Plugins
{
    internal class FakeUploader : IUploader
    {
        public Logger logger = LogManager.GetCurrentClassLogger();

        public void Initialize(JObject settings)
        {
            logger.Info($"{nameof(FakeUploader)}->Initialize()!");
        }

        public void Connect()
        {
            logger.Info($"{nameof(FakeUploader)}->Connect()!");
        }

        public void UploadFile(string path, string destName, EventHandler<StreamTransferProgressArgs> callback = null)
        {
            logger.Info($"{nameof(FakeUploader)}->UploadFile({path}, {destName})");
        }

        public void Disconnect()
        {
            logger.Info($"{nameof(FakeUploader)}->Disconnect()!");
        }
    }
}
