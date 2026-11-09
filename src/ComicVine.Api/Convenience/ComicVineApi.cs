using System;
using ComicVine.Api.Client;
using M = ComicVine.Api.Client.Models;
using PublishersQ = ComicVine.Api.Client.Publishers.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using PublishersList = ComicVine.Api.Client.Publishers.GetResponse;
using PublishersItem = ComicVine.Api.Client.Publisher.Item.GetResponse;
using VolumesQ = ComicVine.Api.Client.Volumes.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using VolumesList = ComicVine.Api.Client.Volumes.GetResponse;
using VolumesItem = ComicVine.Api.Client.Volume.Item.GetResponse;
using IssuesQ = ComicVine.Api.Client.Issues.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using IssuesList = ComicVine.Api.Client.Issues.GetResponse;
using IssuesItem = ComicVine.Api.Client.Issue.Item.GetResponse;
using CharactersQ = ComicVine.Api.Client.Characters.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using CharactersList = ComicVine.Api.Client.Characters.GetResponse;
using CharactersItem = ComicVine.Api.Client.Character.Item.GetResponse;
using PeopleQ = ComicVine.Api.Client.People.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using PeopleList = ComicVine.Api.Client.People.GetResponse;
using PeopleItem = ComicVine.Api.Client.Person.Item.GetResponse;
using TeamsQ = ComicVine.Api.Client.Teams.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using TeamsList = ComicVine.Api.Client.Teams.GetResponse;
using TeamsItem = ComicVine.Api.Client.Team.Item.GetResponse;
using StoryArcsQ = ComicVine.Api.Client.Story_arcs.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using StoryArcsList = ComicVine.Api.Client.Story_arcs.GetResponse;
using StoryArcsItem = ComicVine.Api.Client.Story_arc.Item.GetResponse;
using LocationsQ = ComicVine.Api.Client.Locations.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using LocationsList = ComicVine.Api.Client.Locations.GetResponse;
using LocationsItem = ComicVine.Api.Client.Location.Item.GetResponse;
using ConceptsQ = ComicVine.Api.Client.Concepts.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using ConceptsList = ComicVine.Api.Client.Concepts.GetResponse;
using ConceptsItem = ComicVine.Api.Client.Concept.Item.GetResponse;
using ObjectsQ = ComicVine.Api.Client.Objects.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using ObjectsList = ComicVine.Api.Client.Objects.GetResponse;
using ObjectsItem = ComicVine.Api.Client.ObjectNamespace.Item.GetResponse;
using OriginsQ = ComicVine.Api.Client.Origins.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using OriginsList = ComicVine.Api.Client.Origins.GetResponse;
using OriginsItem = ComicVine.Api.Client.Origin.Item.GetResponse;
using PowersQ = ComicVine.Api.Client.Powers.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using PowersList = ComicVine.Api.Client.Powers.GetResponse;
using PowersItem = ComicVine.Api.Client.Power.Item.GetResponse;
using MoviesQ = ComicVine.Api.Client.Movies.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using MoviesList = ComicVine.Api.Client.Movies.GetResponse;
using MoviesItem = ComicVine.Api.Client.Movie.Item.GetResponse;
using EpisodesQ = ComicVine.Api.Client.Episodes.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using EpisodesList = ComicVine.Api.Client.Episodes.GetResponse;
using EpisodesItem = ComicVine.Api.Client.Episode.Item.GetResponse;
using SeriesQ = ComicVine.Api.Client.Series_list.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using SeriesList = ComicVine.Api.Client.Series_list.GetResponse;
using SeriesItem = ComicVine.Api.Client.Series.Item.GetResponse;
using VideosQ = ComicVine.Api.Client.Videos.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using VideosList = ComicVine.Api.Client.Videos.GetResponse;
using VideosItem = ComicVine.Api.Client.Video.Item.GetResponse;
using VideoTypesQ = ComicVine.Api.Client.Video_types.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using VideoTypesList = ComicVine.Api.Client.Video_types.GetResponse;
using VideoTypesItem = ComicVine.Api.Client.Video_type.Item.GetResponse;
using VideoCategoriesQ = ComicVine.Api.Client.Video_categories.EmptyPathSegmentRequestBuilder.EmptyPathSegmentRequestBuilderGetQueryParameters;
using VideoCategoriesList = ComicVine.Api.Client.Video_categories.GetResponse;
using VideoCategoriesItem = ComicVine.Api.Client.Video_category.Item.GetResponse;

namespace ComicVine.Api
{
    /// <summary>Default <see cref="IComicVineApi"/> implementation over a <see cref="ComicVineClient"/>.</summary>
    public sealed class ComicVineApi : IComicVineApi
    {
        public ComicVineApi(ComicVineClient client)
        {
            Raw = client ?? throw new ArgumentNullException(nameof(client));
            var api = client;
            Search = new SearchResource(client);

            Publishers = new Resource<M.Publisher, PublishersQ, PublishersList, PublishersItem>(
                (cfg, ct) => api.Publishers.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Publisher[ResourceIds.Publisher(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Volumes = new Resource<M.Volume, VolumesQ, VolumesList, VolumesItem>(
                (cfg, ct) => api.Volumes.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Volume[ResourceIds.Volume(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Issues = new Resource<M.Issue, IssuesQ, IssuesList, IssuesItem>(
                (cfg, ct) => api.Issues.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Issue[ResourceIds.Issue(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Characters = new Resource<M.Character, CharactersQ, CharactersList, CharactersItem>(
                (cfg, ct) => api.Characters.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Character[ResourceIds.Character(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            People = new Resource<M.Person, PeopleQ, PeopleList, PeopleItem>(
                (cfg, ct) => api.People.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Person[ResourceIds.Person(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Teams = new Resource<M.Team, TeamsQ, TeamsList, TeamsItem>(
                (cfg, ct) => api.Teams.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Team[ResourceIds.Team(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            StoryArcs = new Resource<M.StoryArc, StoryArcsQ, StoryArcsList, StoryArcsItem>(
                (cfg, ct) => api.Story_arcs.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Story_arc[ResourceIds.StoryArc(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Locations = new Resource<M.Location, LocationsQ, LocationsList, LocationsItem>(
                (cfg, ct) => api.Locations.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Location[ResourceIds.Location(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Concepts = new Resource<M.Concept, ConceptsQ, ConceptsList, ConceptsItem>(
                (cfg, ct) => api.Concepts.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Concept[ResourceIds.Concept(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Objects = new Resource<M.ObjectObject, ObjectsQ, ObjectsList, ObjectsItem>(
                (cfg, ct) => api.Objects.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Object[ResourceIds.Object(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Origins = new Resource<M.Origin, OriginsQ, OriginsList, OriginsItem>(
                (cfg, ct) => api.Origins.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Origin[ResourceIds.Origin(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Powers = new Resource<M.Power, PowersQ, PowersList, PowersItem>(
                (cfg, ct) => api.Powers.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Power[ResourceIds.Power(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Movies = new Resource<M.Movie, MoviesQ, MoviesList, MoviesItem>(
                (cfg, ct) => api.Movies.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Movie[ResourceIds.Movie(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Episodes = new Resource<M.Episode, EpisodesQ, EpisodesList, EpisodesItem>(
                (cfg, ct) => api.Episodes.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Episode[ResourceIds.Episode(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Series = new Resource<M.Series, SeriesQ, SeriesList, SeriesItem>(
                (cfg, ct) => api.Series_list.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Series[ResourceIds.Series(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            Videos = new Resource<M.Video, VideosQ, VideosList, VideosItem>(
                (cfg, ct) => api.Videos.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Video[ResourceIds.Video(id)].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            VideoTypes = new Resource<M.VideoType, VideoTypesQ, VideoTypesList, VideoTypesItem>(
                (cfg, ct) => api.Video_types.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Video_type[id].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);

            VideoCategories = new Resource<M.VideoCategory, VideoCategoriesQ, VideoCategoriesList, VideoCategoriesItem>(
                (cfg, ct) => api.Video_categories.EmptyPathSegment.GetAsync(cfg, ct),
                r => r.Results,
                (id, ct) => api.Video_category[id].EmptyPathSegment.GetAsync(cancellationToken: ct),
                r => r.Results);
        }

        /// <summary>Creates a ready-to-use wrapper without a DI container.</summary>
        public static ComicVineApi Create(string apiKey) => new ComicVineApi(ComicVineClientFactory.Create(apiKey));

        public ComicVineClient Raw { get; }
        public ISearchResource Search { get; }
        public IReadResource<M.Publisher> Publishers { get; }
        public IReadResource<M.Volume> Volumes { get; }
        public IReadResource<M.Issue> Issues { get; }
        public IReadResource<M.Character> Characters { get; }
        public IReadResource<M.Person> People { get; }
        public IReadResource<M.Team> Teams { get; }
        public IReadResource<M.StoryArc> StoryArcs { get; }
        public IReadResource<M.Location> Locations { get; }
        public IReadResource<M.Concept> Concepts { get; }
        public IReadResource<M.ObjectObject> Objects { get; }
        public IReadResource<M.Origin> Origins { get; }
        public IReadResource<M.Power> Powers { get; }
        public IReadResource<M.Movie> Movies { get; }
        public IReadResource<M.Episode> Episodes { get; }
        public IReadResource<M.Series> Series { get; }
        public IReadResource<M.Video> Videos { get; }
        public IReadResource<M.VideoType> VideoTypes { get; }
        public IReadResource<M.VideoCategory> VideoCategories { get; }
    }
}
