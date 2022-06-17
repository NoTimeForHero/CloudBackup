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
        public override string Id => "amazon_s3";
        public override string Name => "Amazon S3 Plugin";
        public override string Description => "Данный плагин позволяет загружать архивы в Amazon S3";
        public override string Author => "NoTimeForHero";
        public override string Url => null;

        public override void Initialize()
        {
        }
    }
}
