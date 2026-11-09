using System;
using System.Linq;
using System.Threading.Tasks;
using ComicVine.Api.Client;
using ComicVine.Api.Client.Search;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace ComicVine.Api.Tests;

[Category("Live")]
[RequiresApiKey]
[NotInParallel("comicvine-live")]
public class LiveTests
{
    private static ComicVineClient NewClient() => ComicVineClientFactory.Create(LiveApiKey.Find()!);

    [Test]
    public async Task Publisher_list_and_detail()
    {
        var c = NewClient();
        var page = await c.Publishers.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 2);
        await Assert.That(page!.NumberOfTotalResults!.Value).IsGreaterThan(0);
        var first = page.Results!.First();
        var detail = await c.Publisher[ResourceIds.Publisher(first.Id!.Value)].EmptyPathSegment.GetAsync();
        await Assert.That(detail!.Results!.Id).IsEqualTo(first.Id);
    }

    [Test]
    public async Task Character_list_and_detail()
    {
        var c = NewClient();
        var page = await c.Characters.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 2);
        var first = page!.Results!.First();
        var detail = await c.Character[ResourceIds.Character(first.Id!.Value)].EmptyPathSegment.GetAsync();
        await Assert.That(detail!.Results!.Id).IsEqualTo(first.Id);
    }

    [Test]
    public async Task Character_detail_for_batman_has_expected_shape()
    {
        var detail = await NewClient().Character[ResourceIds.Character(1699)].EmptyPathSegment.GetAsync();
        var batman = detail!.Results!;
        await Assert.That(batman.Name).IsEqualTo("Batman");
        await Assert.That(batman.Publisher!.Name).IsEqualTo("DC Comics");
        await Assert.That(batman.Image!.OriginalUrl).IsNotNull();
        await Assert.That(batman.Teams!).IsNotEmpty();
    }

    [Test]
    public async Task Issue_list_and_detail()
    {
        var c = NewClient();
        var page = await c.Issues.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 2);
        var first = page!.Results!.First();
        var detail = await c.Issue[ResourceIds.Issue(first.Id!.Value)].EmptyPathSegment.GetAsync();
        await Assert.That(detail!.Results!.Id).IsEqualTo(first.Id);
    }

    [Test]
    public async Task Volume_filter_sort_and_detail()
    {
        var c = NewClient();
        var page = await c.Volumes.EmptyPathSegment.GetAsync(q =>
        {
            q.QueryParameters.Filter = "name:Batman";
            q.QueryParameters.Sort = "count_of_issues:desc";
            q.QueryParameters.Limit = 3;
        });
        await Assert.That(page!.Results!).IsNotEmpty();
        var first = page.Results!.First();
        await Assert.That(first.Name!.Contains("Batman", StringComparison.OrdinalIgnoreCase)).IsTrue();
        var detail = await c.Volume[ResourceIds.Volume(first.Id!.Value)].EmptyPathSegment.GetAsync();
        await Assert.That(detail!.Results!.Id).IsEqualTo(first.Id);
    }

    [Test]
    public async Task Field_list_limits_returned_fields()
    {
        var page = await NewClient().Publishers.EmptyPathSegment.GetAsync(q =>
        {
            q.QueryParameters.Limit = 1;
            q.QueryParameters.FieldList = new[] { "id", "name" };
        });
        var publisher = page!.Results!.Single();
        await Assert.That(publisher.Id).IsNotNull();
        await Assert.That(publisher.Name).IsNotNull();
        await Assert.That(publisher.Description).IsNull();
    }

    [Test]
    public async Task Offset_pages_through_results()
    {
        var c = NewClient();
        var p1 = await c.Publishers.EmptyPathSegment.GetAsync(q => { q.QueryParameters.Limit = 1; q.QueryParameters.Offset = 0; });
        var p2 = await c.Publishers.EmptyPathSegment.GetAsync(q => { q.QueryParameters.Limit = 1; q.QueryParameters.Offset = 1; });
        await Assert.That(p2!.Results!.Single().Id).IsNotEqualTo(p1!.Results!.Single().Id);
    }

    [Test]
    public async Task Search_returns_typed_results()
    {
        var found = await NewClient().Search.EmptyPathSegment.GetAsync(q =>
        {
            q.QueryParameters.Query = "batman";
            q.QueryParameters.Resources = new[] { GetResourcesQueryParameterType.Character };
            q.QueryParameters.Limit = 3;
        });
        await Assert.That(found!.Results!).IsNotEmpty();
        await Assert.That(found.Results!.All(r => r.ResourceType == "character")).IsTrue();
    }

    [Test]
    public async Task Types_lists_resource_types()
    {
        var types = await NewClient().Types.EmptyPathSegment.GetAsync();
        await Assert.That(types!.Results!.Any(t => t.DetailResourceName == "character" && t.Id == 4005)).IsTrue();
    }

    [Test]
    public async Task Unknown_detail_id_returns_status_101_with_no_result()
    {
        var detail = await NewClient().Character[ResourceIds.Character(2147000000)].EmptyPathSegment.GetAsync();
        await Assert.That(detail!.StatusCode).IsEqualTo(101);
        await Assert.That(detail.Results?.Id).IsNull();
    }

    [Test]
    public async Task Invalid_key_throws()
    {
        var bad = ComicVineClientFactory.Create("not-a-real-key");
        var ex = await Assert.ThrowsAsync<Microsoft.Kiota.Abstractions.ApiException>(() =>
            bad.Publishers.EmptyPathSegment.GetAsync());
        await Assert.That(ex!.ResponseStatusCode).IsEqualTo(401);
    }
}

[Category("Live")]
[RequiresApiKey]
[NotInParallel("comicvine-live")]
public class LiveResourceShapeTests
{
    private static ComicVineClient NewClient() => ComicVineClientFactory.Create(LiveApiKey.Find()!);

    [Test] public async Task Concepts() { var r = await NewClient().Concepts.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Concept[ResourceIds.Concept(r!.Results![0].Id!.Value)].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task Episodes() { var r = await NewClient().Episodes.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Episode[ResourceIds.Episode(r!.Results![0].Id!.Value)].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task Locations() { var r = await NewClient().Locations.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Location[ResourceIds.Location(r!.Results![0].Id!.Value)].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task Movies() { var r = await NewClient().Movies.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Movie[ResourceIds.Movie(r!.Results![0].Id!.Value)].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task Objects() { var r = await NewClient().Objects.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Object[ResourceIds.Object(r!.Results![0].Id!.Value)].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task Origins() { var r = await NewClient().Origins.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Origin[ResourceIds.Origin(r!.Results![0].Id!.Value)].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task People() { var r = await NewClient().People.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Person[ResourceIds.Person(r!.Results![0].Id!.Value)].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task Powers() { var r = await NewClient().Powers.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Power[ResourceIds.Power(r!.Results![0].Id!.Value)].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task SeriesList() { var r = await NewClient().Series_list.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Series[ResourceIds.Series(r!.Results![0].Id!.Value)].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task StoryArcs() { var r = await NewClient().Story_arcs.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Story_arc[ResourceIds.StoryArc(r!.Results![0].Id!.Value)].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task Teams() { var r = await NewClient().Teams.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Team[ResourceIds.Team(r!.Results![0].Id!.Value)].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task Videos() { var r = await NewClient().Videos.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Video[ResourceIds.Video(r!.Results![0].Id!.Value)].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task VideoTypes() { var r = await NewClient().Video_types.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Video_type[r!.Results![0].Id!.Value].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
    [Test] public async Task VideoCategories() { var r = await NewClient().Video_categories.EmptyPathSegment.GetAsync(q => q.QueryParameters.Limit = 1); var d = await NewClient().Video_category[r!.Results![0].Id!.Value].EmptyPathSegment.GetAsync(); await Assert.That(d!.Results!.Name).IsNotNull(); }
}
