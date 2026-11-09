using System;
using System.Net.Http;
using ComicVine.Api.Client;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace ComicVine.Api
{
    /// <summary>Creates a ready-to-use <see cref="ComicVineClient"/> without a DI container.</summary>
    public static class ComicVineClientFactory
    {
        public static ComicVineClient Create(string apiKey) =>
            Create(new ComicVineClientOptions { ApiKey = apiKey });

        public static ComicVineClient Create(ComicVineClientOptions options, HttpClient? httpClient = null)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Validate();

            var client = httpClient ?? KiotaClientFactory.Create();
            if (!client.DefaultRequestHeaders.UserAgent.TryParseAdd(options.UserAgent))
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ComicVine.Api.NET");

            var host = new Uri(options.BaseUrl).Host;
            var adapter = new HttpClientRequestAdapter(
                new ComicVineAuthenticationProvider(options.ApiKey!, host), httpClient: client)
            {
                BaseUrl = options.BaseUrl.TrimEnd('/')
            };
            return new ComicVineClient(adapter);
        }
    }
}
