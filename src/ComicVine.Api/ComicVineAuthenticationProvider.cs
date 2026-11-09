using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace ComicVine.Api
{
    /// <summary>Adds <c>api_key</c> and <c>format=json</c> to every request to the configured host.</summary>
    internal sealed class ComicVineAuthenticationProvider : IAuthenticationProvider
    {
        private readonly string _apiKey;
        private readonly AllowedHostsValidator _hosts;

        public ComicVineAuthenticationProvider(string apiKey, string host)
        {
            _apiKey = apiKey;
            _hosts = new AllowedHostsValidator(new[] { host });
        }

        public Task AuthenticateRequestAsync(
            RequestInformation request,
            Dictionary<string, object>? additionalAuthenticationContext = null,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (_hosts.IsUrlHostValid(request.URI))
            {
                request.QueryParameters["api_key"] = _apiKey;
                request.QueryParameters["format"] = "json";
            }
            return Task.CompletedTask;
        }
    }
}
