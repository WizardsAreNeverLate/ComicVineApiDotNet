using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace ComicVine.Api.Tests;

public class ConvenienceTests
{
    private static string Page(int total, int offset, params int[] ids) =>
        "{\"error\":\"OK\",\"limit\":2,\"offset\":" + offset + ",\"number_of_page_results\":" + ids.Length +
        ",\"number_of_total_results\":" + total + ",\"status_code\":1,\"results\":[" +
        string.Join(",", ids.Select(i => "{\"id\":" + i + ",\"name\":\"P" + i + "\"}")) + "],\"version\":\"1.0\"}";

    private static (ComicVineApi Api, FakeHandler Handler) NewApi(FakeHandler handler) =>
        (new ComicVineApi(ComicVineClientFactory.Create(
            new ComicVineClientOptions { ApiKey = "k" }, new HttpClient(handler))), handler);

    [Test]
    public async Task List_maps_options_to_the_query_string_and_reports_paging()
    {
        var (api, handler) = NewApi(new FakeHandler(Page(5, 2, 3, 4)));
        var options = new ListOptions { PageSize = 2 }
            .Where("name", "Batman").Where("publisher", 10)
            .SortBy("count_of_issues", descending: true).Select("id", "name");

        var page = await api.Volumes.ListAsync(options, offset: 2);

        var query = Uri.UnescapeDataString(handler.Requests.Single().RequestUri!.Query);
        await Assert.That(query).Contains("filter=name:Batman,publisher:10");
        await Assert.That(query).Contains("sort=count_of_issues:desc");
        await Assert.That(query).Contains("field_list=id,name");
        await Assert.That(query).Contains("limit=2");
        await Assert.That(query).Contains("offset=2");
        await Assert.That(page.Items.Select(v => v.Id)).IsEquivalentTo(new int?[] { 3, 4 });
        await Assert.That(page.TotalCount).IsEqualTo(5);
        await Assert.That(page.HasNext).IsTrue();
    }

    [Test]
    public async Task Stream_follows_offsets_until_the_last_page()
    {
        var handler = new SequenceHandler(Page(3, 0, 1, 2), Page(3, 2, 3));
        var api = new ComicVineApi(ComicVineClientFactory.Create(
            new ComicVineClientOptions { ApiKey = "k" }, new HttpClient(handler)));

        var ids = new List<int?>();
        await foreach (var p in api.Publishers.StreamAsync(new ListOptions { PageSize = 2 }))
            ids.Add(p.Id);

        await Assert.That(ids).IsEquivalentTo(new int?[] { 1, 2, 3 });
        await Assert.That(handler.Requests.Count).IsEqualTo(2);
        await Assert.That(handler.Requests[1].RequestUri!.Query).Contains("offset=2");
    }

    [Test]
    public async Task Get_adds_the_type_prefix()
    {
        var (api, handler) = NewApi(new FakeHandler(
            "{\"status_code\":1,\"error\":\"OK\",\"results\":{\"id\":1699,\"name\":\"Batman\"}}"));

        var character = await api.Characters.GetAsync(1699);

        await Assert.That(handler.Requests.Single().RequestUri!.AbsolutePath).IsEqualTo("/api/character/4005-1699/");
        await Assert.That(character.Name).IsEqualTo("Batman");
    }

    [Test]
    public async Task Get_uses_plain_ids_for_video_types()
    {
        var (api, handler) = NewApi(new FakeHandler(
            "{\"status_code\":1,\"error\":\"OK\",\"results\":{\"id\":7,\"name\":\"3-Min. Expert\"}}"));

        await api.VideoTypes.GetAsync(7);

        await Assert.That(handler.Requests.Single().RequestUri!.AbsolutePath).IsEqualTo("/api/video_type/7/");
    }

    [Test]
    public async Task Get_reports_status_101_as_404()
    {
        var (api, _) = NewApi(new FakeHandler("{\"error\":\"Object Not Found\",\"status_code\":101,\"results\":[]}"));

        var ex = await Assert.ThrowsAsync<ApiException>(() => api.Characters.GetAsync(1));

        await Assert.That(ex!.ResponseStatusCode).IsEqualTo(404);
    }

    [Test]
    public async Task List_reports_filter_errors_as_400()
    {
        var (api, _) = NewApi(new FakeHandler("{\"error\":\"Filter Error\",\"status_code\":104,\"results\":[]}"));

        var ex = await Assert.ThrowsAsync<ApiException>(() => api.Issues.ListAsync());

        await Assert.That(ex!.ResponseStatusCode).IsEqualTo(400);
    }

    [Test]
    public async Task Invalid_page_size_is_rejected()
    {
        await Assert.That(() => new ListOptions { PageSize = 101 }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Di_registers_the_wrapper()
    {
        var services = new ServiceCollection();
        services.AddComicVineClient(o => o.ApiKey = "k");
        using var provider = services.BuildServiceProvider();

        var api = provider.GetRequiredService<IComicVineApi>();

        await Assert.That(ReferenceEquals(api.Raw, provider.GetRequiredService<Client.ComicVineClient>())).IsTrue();
    }

    private sealed class SequenceHandler : HttpMessageHandler
    {
        private readonly Queue<string> _bodies;

        public SequenceHandler(params string[] bodies) => _bodies = new Queue<string>(bodies);

        public List<HttpRequestMessage> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_bodies.Dequeue(), System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}

[Category("Live")]
[RequiresApiKey]
[NotInParallel("comicvine-live")]
public class LiveConvenienceTests
{
    private static ComicVineApi NewApi() => ComicVineApi.Create(LiveApiKey.Find()!);

    [Test]
    public async Task Stream_yields_publishers_across_pages()
    {
        var ids = new List<int?>();
        await foreach (var p in NewApi().Publishers.StreamAsync(new ListOptions { PageSize = 2 }.Select("id", "name")))
        {
            ids.Add(p.Id);
            if (ids.Count == 5) break;
        }

        await Assert.That(ids.Count).IsEqualTo(5);
        await Assert.That(ids.Distinct().Count()).IsEqualTo(5);
    }

    [Test]
    public async Task Filtered_sorted_volume_list_then_issue_detail()
    {
        var api = NewApi();
        var volumes = await api.Volumes.ListAsync(new ListOptions { PageSize = 3 }
            .Where("name", "Batman").SortBy("count_of_issues", descending: true));
        await Assert.That(volumes.Items).IsNotEmpty();

        var issues = await api.Issues.ListAsync(new ListOptions { PageSize = 2 }
            .Where("volume", volumes.Items[0].Id!.Value).SortBy("cover_date"));
        var issue = await api.Issues.GetAsync(issues.Items[0].Id!.Value);

        await Assert.That(issue.Id).IsEqualTo(issues.Items[0].Id);
        await Assert.That(issue.Volume!.Id).IsEqualTo(volumes.Items[0].Id);
    }

    [Test]
    public async Task Date_range_filter_returns_issues_in_range()
    {
        var page = await NewApi().Issues.ListAsync(new ListOptions { PageSize = 5 }
            .WhereBetween("cover_date", new DateTime(2020, 1, 1), new DateTime(2020, 1, 31))
            .Select("id", "cover_date"));

        await Assert.That(page.Items).IsNotEmpty();
        await Assert.That(page.Items.All(i => i.CoverDate!.StartsWith("2020-01", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task Search_streams_matching_characters()
    {
        var names = new List<string?>();
        await foreach (var r in NewApi().Search.StreamAsync("batman",
            new[] { Client.Search.GetResourcesQueryParameterType.Character }))
        {
            names.Add(r.Name);
            if (names.Count == 3) break;
        }

        await Assert.That(names.Count).IsEqualTo(3);
    }

    [Test]
    public async Task Get_unknown_id_throws_404()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() => NewApi().Characters.GetAsync(2147000000));

        await Assert.That(ex!.ResponseStatusCode).IsEqualTo(404);
    }
}
