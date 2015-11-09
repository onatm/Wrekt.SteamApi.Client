using System;
using System.Net.Http;
using Hepsi.Http.Client;

namespace Wrekt.SteamApi.Client
{
    public class WrektSteamApiClientFactory : IHttpClientFactory
    {
        private readonly WrektSteamApiClientBuilder builder;

        public WrektSteamApiClientFactory(WrektSteamApiClientBuilder builder)
        {
            this.builder = builder;
        }

        public HttpClient CreateHttpClient(string baseAddress)
        {
            var httpClient = builder.Build();
            httpClient.BaseAddress = new Uri(baseAddress);

            return httpClient;
        }

        public HttpClient CreateHttpClient()
        {
            return builder.Build();
        }
    }
}
