using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinClient.Api;

namespace WinClient.Core
{
    internal class TestController
    {
        public static void Test(GuiController controller)
        {
            new Thread(() =>
            {
                Thread.Sleep(500);
                controller.OnMessage(new Api.Message(MessageType.Started).Json);
                Thread.Sleep(1000);
                var total = 50 * 1000;
                for (int i = 0; i < total; i += 1024)
                {
                    var fakeApi = new Api.Message(MessageType.ProgressUpdated)
                    {
                        States = new Dictionary<string, UploadJobState> { { "Job", new UploadJobState { status = "Отправка архива...", total = total, current = i } } }
                    };
                    controller.OnMessage(fakeApi.Json);
                    Thread.Sleep(100);
                }
                controller.OnMessage(new Api.Message(MessageType.Completed).Json);
                Thread.Sleep(5000);
                controller.OnMessage(new Api.Message(MessageType.Started).Json);
                Thread.Sleep(2000);
                controller.OnMessage(new Api.Message(MessageType.Completed).Json);
            }).Start();
        }
    }
}
