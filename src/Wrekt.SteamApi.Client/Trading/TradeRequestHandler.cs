using System.Net;
using System.Net.Cache;
using System.Net.Http;

namespace Wrekt.SteamApi.Client.Trading
{
    public class TradeRequestHandler : WebRequestHandler
    {
        public TradeRequestHandler(CookieContainer cookieContainer)
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
            CookieContainer = cookieContainer;
        }
    }
}
