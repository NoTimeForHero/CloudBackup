using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinClient
{
    // Хоть это и копипаст, но это проще, чем тянуть в зависимость главный проект
    // или создавать ещё один проект библиотеки классов для API ради одного класса.
    class UploadJobState
    {
        public string status { get; set; }
        public bool inProgress { get; set; }
        public bool isBytes { get; set; }

        public long current { get; set; }
        public long total { get; set; }
    }
}
