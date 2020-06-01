using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinClient
{
    class SocketClient
    {
        private readonly Uri uri; 
        private readonly Config config;
        private readonly ObserverVariable<String> status;
        private readonly CancellationToken token;

        public event Action<string> OnMessage;

        public SocketClient(Config config, ObserverVariable<string> status, CancellationToken? token = null)
        {
            this.config = config;
            this.status = status;
            this.token = token ?? CancellationToken.None;
            uri = new Uri(config.watch);
        }

        public void StartAsync()
        {
            Task.Factory.StartNew(Start, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private async Task Start()
        {
            Reconnect:
            // Если не создавать класс на каждой итерации подключения, можно словить ошибку при некоторых условиях
            // Например остановка сервера WebSocket поломает ClientWebSocket и методы Close не помогают
            // Либо я что-то делаю неправильно
            ClientWebSocket socket = new ClientWebSocket();
            try
            {
                status.Value = "Подключение";
                await socket.ConnectAsync(uri, CancellationToken.None);
                if (socket.State != WebSocketState.Open) throw new WebException($"Invalid WebSocketState: {socket.State}");
                status.Value = "Подключено";
                Program.Logger.Append("Установлено подключение по протоколу WebSocket к адресу: " + uri);
                while (!token.IsCancellationRequested)
                {
                    var buffer = new ArraySegment<byte>(new byte[8192]);
                    WebSocketReceiveResult result;
                    MemoryStream ms = new MemoryStream();
                    do
                    {
                        result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                        await ms.WriteAsync(buffer.Array, buffer.Offset, result.Count, token);
                    } while (!result.EndOfMessage);
                    var message = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                    OnMessage?.Invoke(message);
                }
            }
            catch (WebSocketException ex)
            {
                status.Value = "Ошибка";
                Program.Logger.Append($"Ошибка: {ex}" + Environment.NewLine + Environment.NewLine);
                await Task.Delay(1500, token);
                goto Reconnect;
            }
            finally
            {
                socket.Dispose();
            }
        }


    }
}
