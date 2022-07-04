using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WinClient.Core
{
    internal class WebApi
    {
        private static HttpClient httpClient = new HttpClient();
        private static StringContent EmptyBody = new StringContent(string.Empty);

        public static async Task startJob(string url, string name)
        {
            httpClient = new HttpClient();
            name = WebUtility.UrlEncode(name);
            var uri = new Uri(string.Format(url, name));
            var response = await httpClient.PostAsync(uri, EmptyBody);
            response.EnsureSuccessStatusCode();
        }

    }
}
