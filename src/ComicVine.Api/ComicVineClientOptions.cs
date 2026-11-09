using System;

namespace ComicVine.Api
{
    /// <summary>Settings for connecting to the Comic Vine API.</summary>
    public sealed class ComicVineClientOptions
    {
        public const string DefaultBaseUrl = "https://comicvine.gamespot.com/api";

        /// <summary>API key from comicvine.gamespot.com/api, sent as the <c>api_key</c> query parameter. Required.</summary>
        public string? ApiKey { get; set; }

        /// <summary>Root URL of the API, including the <c>/api</c> suffix.</summary>
        public string BaseUrl { get; set; } = DefaultBaseUrl;

        /// <summary>Value for the User-Agent header. Comic Vine rejects requests without a descriptive one.</summary>
        public string UserAgent { get; set; } = "ComicVine.Api.NET";

        internal void Validate()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new InvalidOperationException("ComicVineClientOptions.ApiKey is required.");
            if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
                throw new InvalidOperationException("ComicVineClientOptions.BaseUrl must be an absolute URL.");
        }
    }
}
