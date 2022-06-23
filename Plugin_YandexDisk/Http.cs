using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CloudBackuper.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Plugin_YandexDisk
{

    internal class WebClient : IDisposable
    {
        protected const string API_URL = "https://cloud-api.yandex.net/v1/disk";
        protected Logger logger = LogManager.GetCurrentClassLogger();
        protected HttpClient client;

        public WebClient(string token)
        {
            var handler = new HttpClientHandler();
            // handler.Proxy = new WebProxy { Address = new Uri("http://localhost:8888") };
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.Expect100Continue = false;
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Authorization", $"OAuth {token}");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("User-Agent", "CloudBackuper");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        }

        private void EnsureSuccess(HttpResponseMessage response, string body)
        {
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                logger.Warn($"Ошибка: {ex.Message}");
                var headers = response.Content.Headers.Select(x => $"{x.Key}: {x.Value.Join(", ")}").Join("\n");
                logger.Warn($"Заголовки:\n{headers}");
                logger.Warn($"Содержимое ответа:\n{body}");
                throw;
            }
        }

        public async Task<string> GetUploadLink(string path, bool overwrite = false)
        {
            var fullPath = API_URL + $"/resources/upload?overwrite={overwrite}&path=" + WebUtility.UrlEncode(path);
            var requestUri = new Uri(fullPath);
            logger.Info($"HTTP запрос ({nameof(GetUploadLink)}):\n{fullPath}");
            var response = await client.GetAsync(requestUri);
            var responseString = await response.Content.ReadAsStringAsync();
            EnsureSuccess(response, responseString);
            var json = JsonConvert.DeserializeObject<JObject>(responseString);
            var href = json["href"]?.ToObject<string>();
            if (href == null) throw new InvalidOperationException("HREF is null!");
            return href;
        }


        public async Task RenameFile(string path, string input, string output)
        {
            var route = API_URL + "/resources/move";
            route += "?from=" + WebUtility.UrlEncode(path + "/" + input);
            route += "&path=" + WebUtility.UrlEncode(path + "/" + output);
            route += "&force_async=true";
            route += "&overwrite=true";
            logger.Info($"HTTP запрос ({nameof(RenameFile)}):\n{route}");
            var requestUri = new Uri(route);
            var response = await client.PostAsync(requestUri, new StringContent(string.Empty));
            EnsureSuccess(response, await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// [Внимание!!!] Не загружать ZIP архивы, иначе 3 мегабайта будут отправляться 25+ секунд.
        /// </summary>
        public async Task UploadFile(string href, Stream fs, Action<UploaderProgress> progress)
        {
            var content = new ProgressStreamContent(fs, progress);
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(href);
            request.Method = HttpMethod.Put;
            request.Content = content;
            logger.Info($"HTTP запрос ({nameof(UploadFile)}):\n{href}");
            var response = await client.SendAsync(request);
            EnsureSuccess(response, await response.Content.ReadAsStringAsync());
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }

    // Inspired by: https://stackoverflow.com/a/35340010
    internal class ProgressStreamContent : HttpContent
    {
        private const int defaultBufferSize = 4096;

        private readonly Action<UploaderProgress> onProgress;
        private readonly Stream content;
        private readonly int bufferSize;
        private bool contentConsumed;

        public ProgressStreamContent(Stream content, Action<UploaderProgress> onProgress, int? bufferSize = null)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

            this.content = content;
            this.bufferSize = bufferSize ?? defaultBufferSize;
            this.onProgress = onProgress;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Contract.Assert(stream != null);

            PrepareContent();

            return Task.Run(() =>
            {
                var progress = new UploaderProgress();
                var buffer = new byte[this.bufferSize];
                var size = content.Length;
                var uploaded = 0;

                using (content) while (true)
                {
                    var length = content.Read(buffer, 0, buffer.Length);
                    if (length <= 0) break;

                    uploaded += length;
                    progress.Update(uploaded, (int)size);
                    onProgress(progress);

                    stream.Write(buffer, 0, length);
                }
            });
        }

        private void PrepareContent()
        {
            if (!contentConsumed)
            {
                contentConsumed = true;
                return;
            }
            if (content.CanSeek)
            {
                content.Position = 0;
                return;
            }
            throw new InvalidOperationException("SR.net_http_content_stream_already_read");
        }

        protected override bool TryComputeLength(out long length)
        {
            length = content.Length;
            return true;
        }
    }
}
