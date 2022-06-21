using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;

namespace CloudBackuper.Plugins
{
    internal class FakeUploader : IUploader
    {
        public Logger logger = LogManager.GetCurrentClassLogger();

        public void Initialize(object settings)
        {
            logger.Info($"{nameof(FakeUploader)}->Initialize()!");
        }

        public void Connect()
        {
            logger.Info($"{nameof(FakeUploader)}->Connect()!");
        }

        public async void UploadFile(string path, string destName, Action<UploaderProgress> callback = null)
        {
            logger.Info($"{nameof(FakeUploader)}->UploadFile({path}, {destName})");
            var progress = new UploaderProgress();
            int max = 50 * 1000;
            var step = 1240;
            for (int i = 0; i < max; i += step)
            {
                progress.Update(i, max);
                callback?.Invoke(progress);
                await Task.Delay(300);
            }
        }

        public void Disconnect()
        {
            logger.Info($"{nameof(FakeUploader)}->Disconnect()!");
        }
    }
}
