using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WinClient.Api
{
    // Хоть это и копипаст, но это проще, чем тянуть в зависимость главный проект
    // или создавать ещё один проект библиотеки классов для API ради нескольких классов.

    class UploadJobState
    {
        public string status { get; set; }
        public bool inProgress { get; set; }
        public bool isBytes { get; set; }

        public long current { get; set; }
        public long total { get; set; }
    }

    class Message
    {
        public MessageType Type { get; set; }
        public string Name { get; set; }
        public IDictionary<string, UploadJobState> States { get; set; }

        public Message(MessageType Type) => this.Type = Type;

        [JsonIgnore]
        public string Json => JsonConvert.SerializeObject(this);
    }

    enum MessageType
    {
        Started,
        Completed,
        ProgressUpdated
    }
}
