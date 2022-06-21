using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudBackuper.Plugins;

namespace Plugin_AmazonS3
{
    public class Plugin : IPlugin
    {
        public override string Id => "yadisk";
        public override string Name => "Яндекс Диск";
        public override string Description => "Загрузка архивов на Яндекс Диск";
        public override string Author => "NoTimeForHero";
        public override string Url => null;

        public override void Initialize()
        {
        }
    }
}
