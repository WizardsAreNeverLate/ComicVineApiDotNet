using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ComicVine.Api.Client.Models;
using Microsoft.Kiota.Abstractions;
using M = ComicVine.Api.Client.Models;

namespace ComicVine.Api
{
    internal static class Paging
    {
        public static Page<T> Build<T>(Envelope? response, List<T>? results, int offset)
        {
            Ensure(response);
            var items = results ?? new List<T>();
            return new Page<T>(items, response!.NumberOfTotalResults ?? items.Count, offset);
        }

        public static void Ensure(Envelope? response)
        {
            if (response == null)
                throw new ApiException("Comic Vine returned an empty response.");
            var code = response.StatusCode ?? 1;
            if (code == 1) return;
            var http = code == 100 ? 401 : code == 101 ? 404 : 400;
            throw new ApiException(response.Error ?? ("Comic Vine status_code " + code))
            {
                ResponseStatusCode = http
            };
        }

        public static async IAsyncEnumerable<T> Stream<T>(
            Func<int, CancellationToken, Task<Page<T>>> fetch,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var offset = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var page = await fetch(offset, cancellationToken).ConfigureAwait(false);
                foreach (var item in page.Items)
                    yield return item;
                if (!page.HasNext) yield break;
                offset += page.Items.Count;
            }
        }
    }

    internal sealed class Resource<TModel, TQuery, TList, TItem> : IReadResource<TModel>
        where TQuery : class, IListQuery, new()
        where TList : Envelope
        where TItem : Envelope
    {
        private readonly Func<Action<RequestConfiguration<TQuery>>, CancellationToken, Task<TList?>> _list;
        private readonly Func<TList, List<TModel>?> _items;
        private readonly Func<int, CancellationToken, Task<TItem?>> _get;
        private readonly Func<TItem, TModel?> _detail;

        public Resource(
            Func<Action<RequestConfiguration<TQuery>>, CancellationToken, Task<TList?>> list,
            Func<TList, List<TModel>?> items,
            Func<int, CancellationToken, Task<TItem?>> get,
            Func<TItem, TModel?> detail)
        {
            _list = list;
            _items = items;
            _get = get;
            _detail = detail;
        }

        public async Task<Page<TModel>> ListAsync(
            ListOptions? options = null, int offset = 0, CancellationToken cancellationToken = default)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            var o = options ?? new ListOptions();
            var response = await _list(c => o.Apply(c.QueryParameters!, offset), cancellationToken).ConfigureAwait(false);
            return Paging.Build(response, response == null ? null : _items(response), offset);
        }

        public IAsyncEnumerable<TModel> StreamAsync(
            ListOptions? options = null, CancellationToken cancellationToken = default) =>
            Paging.Stream<TModel>((offset, ct) => ListAsync(options, offset, ct), cancellationToken);

        public async Task<TModel> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _get(id, cancellationToken).ConfigureAwait(false);
            Paging.Ensure(response);
            var detail = _detail(response!);
            if (detail == null)
                throw new ApiException("Comic Vine object not found.") { ResponseStatusCode = 404 };
            return detail;
        }
    }

    internal sealed class SearchResource : ISearchResource
    {
        private readonly Client.ComicVineClient _client;

        public SearchResource(Client.ComicVineClient client) => _client = client;

        public async Task<Page<SearchResult>> SearchAsync(
            string query,
            IReadOnlyCollection<Client.Search.GetResourcesQueryParameterType>? resources = null,
            int offset = 0,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("Query is required.", nameof(query));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (pageSize < 1 || pageSize > ListOptions.MaxPageSize) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var response = await _client.Search.EmptyPathSegment.GetAsync(c =>
            {
                c.QueryParameters.Query = query;
                c.QueryParameters.Offset = offset;
                c.QueryParameters.Limit = pageSize;
                if (resources != null && resources.Count > 0)
                    c.QueryParameters.Resources = resources.ToArray();
            }, cancellationToken).ConfigureAwait(false);
            return Paging.Build(response, response?.Results, offset);
        }

        public IAsyncEnumerable<SearchResult> StreamAsync(
            string query,
            IReadOnlyCollection<Client.Search.GetResourcesQueryParameterType>? resources = null,
            CancellationToken cancellationToken = default) =>
            Paging.Stream<SearchResult>(
                (offset, ct) => SearchAsync(query, resources, offset, ListOptions.MaxPageSize, ct), cancellationToken);
    }
}
