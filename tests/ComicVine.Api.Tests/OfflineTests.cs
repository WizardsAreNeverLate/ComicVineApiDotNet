using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ComicVine.Api.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace ComicVine.Api.Tests;

public class OfflineTests
{
    private const string PublisherPage =
        "{\"error\":\"OK\",\"limit\":1,\"offset\":0,\"number_of_page_results\":1,\"number_of_total_results\":9870,\"status_code\":1," +
        "\"results\":[{\"id\":10,\"name\":\"DC Comics\",\"aliases\":null}],\"version\":\"1.0\"}";

    private static (ComicVineClient Client, FakeHandler Handler) NewClient(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new FakeHandler(body, status);
        var client = ComicVineClientFactory.Create(
            new ComicVineClientOptions { ApiKey = "secret-key", UserAgent = "Tests/1.0" },
            new HttpClient(handler));
        return (client, handler);
    }

    [Test]
    public async Task Requests_carry_api_key_json_format_and_user_agent()
    {
        var (client, handler) = NewClient(PublisherPage);

        var page = await client.Publishers.EmptyPathSegment.GetAsync(c => c.QueryParameters.Limit = 1);

        var request = handler.Requests.Single();
        await Assert.That(request.RequestUri!.AbsolutePath).IsEqualTo("/api/publishers/");
        await Assert.That(request.RequestUri.Query).Contains("api_key=secret-key");
        await Assert.That(request.RequestUri.Query).Contains("format=json");
        await Assert.That(request.RequestUri.Query).Contains("limit=1");
        await Assert.That(request.Headers.UserAgent.ToString()).Contains("Tests/1.0");
        await Assert.That(page!.NumberOfTotalResults).IsEqualTo(9870);
        await Assert.That(page.Results![0].Name).IsEqualTo("DC Comics");
    }

    [Test]
    public async Task Detail_request_uses_prefixed_id()
    {
        var (client, handler) = NewClient("{\"status_code\":1,\"error\":\"OK\",\"results\":{\"id\":1699,\"name\":\"Batman\"}}");

        var detail = await client.Character[ResourceIds.Character(1699)].EmptyPathSegment.GetAsync();

        await Assert.That(handler.Requests.Single().RequestUri!.AbsolutePath).IsEqualTo("/api/character/4005-1699/");
        await Assert.That(detail!.Results!.Name).IsEqualTo("Batman");
    }

    [Test]
    public async Task Error_status_surfaces_as_api_exception()
    {
        var (client, _) = NewClient("{\"status_code\":100,\"error\":\"Invalid API Key\"}", HttpStatusCode.Unauthorized);

        var ex = await Assert.ThrowsAsync<ApiException>(() => client.Publishers.EmptyPathSegment.GetAsync());

        await Assert.That(ex!.ResponseStatusCode).IsEqualTo(401);
    }

    [Test]
    public async Task Missing_api_key_is_rejected()
    {
        await Assert.That(() => ComicVineClientFactory.Create(new ComicVineClientOptions()))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Di_registration_resolves_a_singleton_client()
    {
        var services = new ServiceCollection();
        services.AddComicVineClient(o => o.ApiKey = "k");
        using var provider = services.BuildServiceProvider();

        var a = provider.GetRequiredService<ComicVineClient>();
        var b = provider.GetRequiredService<ComicVineClient>();

        await Assert.That(ReferenceEquals(a, b)).IsTrue();
    }

    [Test]
    public async Task Resource_ids_use_comic_vine_type_prefixes()
    {
        await Assert.That(ResourceIds.Issue(6)).IsEqualTo("4000-6");
        await Assert.That(ResourceIds.Volume(796)).IsEqualTo("4050-796");
        await Assert.That(ResourceIds.Video(3344)).IsEqualTo("2300-3344");
    }
}
