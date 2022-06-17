using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBackuper.Plugins
{
    public interface IUploader
    {
        /// <param name="settings">JObject</param>
        void Initialize(object settings);
        void Connect();
        void UploadFile(string path, string destName, Action<UploaderProgress> callback = null);
        void Disconnect();
    }

    public class UploaderProgress
    {
        public int current;
        public int total;

        public void Update(int current, int total)
        {
            this.current = current;
            this.total = total;
        }
    }
}
