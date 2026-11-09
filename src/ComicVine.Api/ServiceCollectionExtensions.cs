using System;
using System.Net.Http;
using ComicVine.Api.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace ComicVine.Api
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>Registers <see cref="ComicVineClient"/> and <see cref="IComicVineApi"/> as singletons backed by <c>IHttpClientFactory</c>, with Kiota's default handlers (retry, redirect, compression).</summary>
        public static IServiceCollection AddComicVineClient(
            this IServiceCollection services, Action<ComicVineClientOptions> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            services.Configure(configure);

            var http = services.AddHttpClient(nameof(ComicVineClient));
            http.ConfigurePrimaryHttpMessageHandler(() => KiotaClientFactory.GetDefaultHttpMessageHandler());
            var handlerCount = KiotaClientFactory.CreateDefaultHandlers().Count;
            for (var i = 0; i < handlerCount; i++)
            {
                var index = i;
                http.AddHttpMessageHandler(() => KiotaClientFactory.CreateDefaultHandlers()[index]);
            }

            services.AddSingleton(sp => ComicVineClientFactory.Create(
                sp.GetRequiredService<IOptions<ComicVineClientOptions>>().Value,
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(ComicVineClient))));
            services.AddSingleton<IComicVineApi>(sp => new ComicVineApi(sp.GetRequiredService<ComicVineClient>()));
            return services;
        }
    }
}
