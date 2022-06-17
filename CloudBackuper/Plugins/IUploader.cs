using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime;
using Newtonsoft.Json.Linq;

namespace CloudBackuper.Plugins
{
    internal interface IUploader
    {
        void Initialize(JObject settings);
        void Connect();
        void UploadFile(string path, string destName, EventHandler<StreamTransferProgressArgs> callback = null);
        void Disconnect();
    }
}
