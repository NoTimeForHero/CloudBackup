using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudBackuper.Plugins;

namespace CloudBackuper
{
    class UploadManager
    {
        protected Config_Uploader config;
        protected IUploader uploader;

        public UploadManager(Config root)
        {
            config = root.Uploader;
            var typeName = root.Uploader.Type;
            var type = Type.GetType(typeName);
            if (type == null) throw new ArgumentException($"Не найден тип: {typeName}");
            if (!typeof(IUploader).IsAssignableFrom(type))
                throw new ApplicationException(
                    $"Тип \"{type.FullName}\" не наследует интерфейс {nameof(IUploader)}!");
            uploader = (IUploader)Activator.CreateInstance(type);
            uploader.Initialize(root.Uploader.Settings);
        }

        public IUploader Resolve() => uploader;
    }
}
