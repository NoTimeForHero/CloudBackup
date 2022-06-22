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

        public Task Initialize(object settings)
        {
            logger.Info($"{nameof(FakeUploader)}->Initialize()!");
            return Task.CompletedTask;
        }

        public Task Connect()
        {
            logger.Info($"{nameof(FakeUploader)}->Connect()!");
            return Task.CompletedTask;
        }

        public async Task UploadFile(string path, string destName, Action<UploaderProgress> callback = null)
        {
            logger.Info($"{nameof(FakeUploader)}->UploadFile({path}, {destName})");
            var progress = new UploaderProgress();
            int max = 20 * 1024 * 1024;
            var step = 10 * 1240;
            for (int i = 0; i < max; i += step)
            {
                progress.Update(i, max);
                callback?.Invoke(progress);
                await Task.Delay(50);
            }
        }

        public Task Disconnect()
        {
            logger.Info($"{nameof(FakeUploader)}->Disconnect()!");
            return Task.CompletedTask;
        }
    }
}
