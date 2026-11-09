using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ComicVine.Api.Client;
using ComicVine.Api.Client.Search;
using M = ComicVine.Api.Client.Models;

namespace ComicVine.Api
{
    /// <summary>
    /// Read access to Comic Vine with plain ids, filter objects and automatic paging. Results are the generated
    /// Kiota models. Every call honors its <see cref="CancellationToken"/>. A failed call throws
    /// <see cref="Microsoft.Kiota.Abstractions.ApiException"/> with the HTTP status in <c>ResponseStatusCode</c>.
    /// <c>GetAsync</c> reports an unknown id as 404, although Comic Vine itself answers HTTP 200 with status_code 101.
    /// </summary>
    public interface IComicVineApi
    {
        IReadResource<M.Publisher> Publishers { get; }
        IReadResource<M.Volume> Volumes { get; }
        IReadResource<M.Issue> Issues { get; }
        IReadResource<M.Character> Characters { get; }
        IReadResource<M.Person> People { get; }
        IReadResource<M.Team> Teams { get; }
        IReadResource<M.StoryArc> StoryArcs { get; }
        IReadResource<M.Location> Locations { get; }
        IReadResource<M.Concept> Concepts { get; }
        IReadResource<M.ObjectObject> Objects { get; }
        IReadResource<M.Origin> Origins { get; }
        IReadResource<M.Power> Powers { get; }
        IReadResource<M.Movie> Movies { get; }
        IReadResource<M.Episode> Episodes { get; }
        IReadResource<M.Series> Series { get; }
        IReadResource<M.Video> Videos { get; }
        IReadResource<M.VideoType> VideoTypes { get; }
        IReadResource<M.VideoCategory> VideoCategories { get; }
        ISearchResource Search { get; }

        /// <summary>The generated Kiota client, for endpoints this wrapper does not cover.</summary>
        ComicVineClient Raw { get; }
    }

    /// <summary>A catalog resource that can be listed and streamed.</summary>
    public interface IListResource<TModel>
    {
        /// <summary>Fetches one page starting at the zero-based <paramref name="offset"/>.</summary>
        Task<Page<TModel>> ListAsync(ListOptions? options = null, int offset = 0, CancellationToken cancellationToken = default);

        /// <summary>Yields every match, requesting further pages as the caller enumerates.</summary>
        IAsyncEnumerable<TModel> StreamAsync(ListOptions? options = null, CancellationToken cancellationToken = default);
    }

    /// <summary>A catalog resource that also has a detail endpoint.</summary>
    public interface IReadResource<TModel> : IListResource<TModel>
    {
        /// <summary>Fetches one entry by its plain numeric id. The type prefix is added for you.</summary>
        Task<TModel> GetAsync(int id, CancellationToken cancellationToken = default);
    }

    /// <summary>Full-text search across resource types.</summary>
    public interface ISearchResource
    {
        /// <param name="query">Text to search for.</param>
        /// <param name="resources">Resource types to search. Searches all when null or empty.</param>
        /// <param name="offset">Zero-based index of the first result.</param>
        /// <param name="pageSize">Results per request, 1 to 100. Comic Vine defaults to 10.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        Task<Page<M.SearchResult>> SearchAsync(
            string query,
            IReadOnlyCollection<GetResourcesQueryParameterType>? resources = null,
            int offset = 0,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>Yields every match, requesting further pages as the caller enumerates.</summary>
        IAsyncEnumerable<M.SearchResult> StreamAsync(
            string query,
            IReadOnlyCollection<GetResourcesQueryParameterType>? resources = null,
            CancellationToken cancellationToken = default);
    }
}
