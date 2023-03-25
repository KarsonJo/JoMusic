using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MusicCrawler
{
    public static class HttpClientHolder
    {
        public static HttpClient Client { get; }

        static HttpClientHolder()
        {
            var handler = new SocketsHttpHandler
            {
                UseCookies = false,
                // Sets how long a connection can be in the pool to be considered reusable (by default - infinite)
                PooledConnectionLifetime = TimeSpan.FromMinutes(1),
            };

            Client = new HttpClient(handler, disposeHandler: false);

            Client.Timeout = TimeSpan.FromSeconds(20);
            Client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
        }

    }
}
