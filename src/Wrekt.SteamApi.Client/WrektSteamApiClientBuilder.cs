using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Common.Logging;
using Hepsi.Http.Client.Logging;
using Hepsi.Http.Client.QoS;
using Wrekt.SteamApi.Client.Trading;

namespace Wrekt.SteamApi.Client
{
    public class WrektSteamApiClientBuilder
    {
        private readonly Dictionary<int, Func<DelegatingHandler>> handlers = new Dictionary<int, Func<DelegatingHandler>>();

        private HttpMessageHandler InnerHandler { get; set; }

        private string Referer { get; set; }

        public WrektSteamApiClientBuilder WithTradingRequestHandler(CookieContainer cookieContainer, string referer)
        {
            InnerHandler = new TradeRequestHandler(cookieContainer);
            Referer = referer;
            return this;
        }

        public WrektSteamApiClientBuilder WithCircuitBreaker(int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak)
        {
            handlers.Add(4000, () => new CircuitBreakingDelegatingHandler(exceptionsAllowedBeforeBreaking, durationOfBreak, null));
            return this;
        }

        public WrektSteamApiClientBuilder WithRetry(int retryCount)
        {
            handlers.Add(3000, () => new RetryingDelegatingHandler(retryCount, null));
            return this;
        }
        public WrektSteamApiClientBuilder WithWaitAndRetry(int retryCount, Func<int, TimeSpan> sleepDurations)
        {
            handlers.Add(3000, () => new RetryingDelegatingHandler(retryCount, sleepDurations, null));
            return this;
        }

        public WrektSteamApiClientBuilder WithTimeout(TimeSpan timeout)
        {
            handlers.Add(2000, () => new TimeoutingDelegatingHandler(timeout, null));
            return this;
        }

        public WrektSteamApiClientBuilder WithLogging(ILog logger)
        {
            handlers.Add(1000, () => new LoggingDelegatingHandler(logger, null));
            return this;
        }

        public WrektSteamApiClientBuilder WithLogging()
        {
            handlers.Add(1000, () => new LoggingDelegatingHandler(null));
            return this;
        }

        internal HttpClient Build()
        {
            var httpClient = handlers.Any() ? new HttpClient(CreateHttpMessageHandler()) : new HttpClient();

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            httpClient.DefaultRequestHeaders.Add("X-Prototype-Version", "1.7");

            if (!string.IsNullOrEmpty(Referer))
            {
                httpClient.DefaultRequestHeaders.Add("Referer", Referer);
            }

            return httpClient;
        }

        private HttpMessageHandler CreateHttpMessageHandler()
        {
            var httpMessageHandler = InnerHandler ?? new HttpClientHandler();

            handlers.OrderByDescending(handler => handler.Key).Select(handler => handler.Value).Reverse().ToList().ForEach(handler =>
            {
                var delegatingHandler = handler();
                delegatingHandler.InnerHandler = httpMessageHandler;
                httpMessageHandler = delegatingHandler;
            });

            return httpMessageHandler;
        }
    }
}
