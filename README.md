# ComicVine.Api

Typed .NET client for the [Comic Vine](https://comicvine.gamespot.com/api/) API. The client code is generated from `docs/openapi.json` with [Kiota](https://learn.microsoft.com/openapi/kiota/). The library targets `netstandard2.0` and `net10.0`.

The OpenAPI file is hand-built from the [official documentation](https://comicvine.gamespot.com/api/documentation) and corrected against live responses. `docs/openapi.json` is the source of truth for the client, so fix spec mistakes there and regenerate.

## Use

`IComicVineApi` is a thin wrapper over the generated client. It takes plain numeric ids (the type prefix is added for you), builds filter, sort and field-list strings from a `ListOptions` object, and pages for you. Results are the generated Kiota models, so properties are nullable.

```csharp
IComicVineApi comicVine = ComicVineApi.Create(apiKey);

var volumes = await comicVine.Volumes.ListAsync(new ListOptions { PageSize = 5 }
    .Where("name", "Batman")
    .SortBy("count_of_issues", descending: true)
    .Select("id", "name", "start_year", "count_of_issues"));
Console.WriteLine($"{volumes.TotalCount} matches, more pages: {volumes.HasNext}");

var batman = await comicVine.Characters.GetAsync(1699);

await foreach (var issue in comicVine.Issues.StreamAsync(new ListOptions().Where("volume", 796).SortBy("cover_date")))
    Console.WriteLine($"{issue.IssueNumber} {issue.Name}");

await foreach (var hit in comicVine.Search.StreamAsync("batman", new[] { GetResourcesQueryParameterType.Character }))
    Console.WriteLine(hit.Name);
```

Every resource (`Publishers`, `Volumes`, `Issues`, `Characters`, `People`, `Teams`, `StoryArcs`, `Locations`, `Concepts`, `Objects`, `Origins`, `Powers`, `Movies`, `Episodes`, `Series`, `Videos`, `VideoTypes`, `VideoCategories`) offers `GetAsync`, `ListAsync` and `StreamAsync`. `Search` offers `SearchAsync` and `StreamAsync`. `StreamAsync` requests the next page only when you enumerate that far, and every method takes a `CancellationToken`. `Raw` exposes the generated client for anything else.

With dependency injection (registers `ComicVineClient` and `IComicVineApi` on `IHttpClientFactory`, with Kiota's retry, redirect and compression handlers):

```csharp
services.AddComicVineClient(o =>
{
    o.ApiKey = configuration["ComicVine:ApiKey"];
    o.UserAgent = "MyApp/1.0";
});
```

The client sends `api_key` and `format=json` on every request. Comic Vine rejects requests without a User-Agent, so set `UserAgent` to something that names your app.

## Use the Kiota client directly

```csharp
var client = ComicVineClientFactory.Create(apiKey);
var page = await client.Publishers.EmptyPathSegment.GetAsync(c => c.QueryParameters.Limit = 5);
var batman = await client.Character[ResourceIds.Character(1699)].EmptyPathSegment.GetAsync();
```

Kiota names collection endpoints `.EmptyPathSegment`. Detail endpoints take the prefixed id, which `ResourceIds` builds.

## Things to know

- Detail endpoints need a type-prefixed id such as `4005-1699`. The wrapper adds the prefix. `ResourceIds` builds it for raw-client calls. `video_type` and `video_category` take plain integer ids.
- The series list lives at `series_list` (`client.Series_list`). `client.Series[...]` is the detail endpoint. The wrapper's `Series` resource hides the split.
- Filter and sort field names are the API's own snake_case names (`count_of_issues`, `cover_date`). The docs do not list which fields each endpoint accepts.
- Failed calls throw `Microsoft.Kiota.Abstractions.ApiException` with `ResponseStatusCode`. Comic Vine answers an unknown id with HTTP 200 and `status_code` 101. The wrapper turns that into a 404 `ApiException`. Through the raw client you must check `StatusCode` yourself. An invalid key is a real HTTP 401.
- `limit` is capped at 100 per request. Page with `Offset`. Comic Vine rate-limits per resource, so prefer the DI path for built-in retry.
- Dates arrive as strings (`2008-06-06 11:08:00`). Most scalar fields are nullable strings. Related resources use the shared `ResourceRef` model.
- `/promos/` and `/chats/` are in the spec from the docs but were not verified. The live `/promos/` returns an odd results shape.

## Regenerate the spec and client

`tools/generate_openapi.py` writes `docs/openapi.json`. It holds the field lists from the docs plus the corrections found against the live API.

```
python tools/generate_openapi.py docs/openapi.json
```

Then regenerate the client with the Kiota tool (`dotnet tool install -g Microsoft.OpenApi.Kiota`).

```
cd src/ComicVine.Api
kiota generate -l CSharp -d ../../docs/openapi.json -o Generated -n ComicVine.Api.Client -c ComicVineClient --exclude-backward-compatible --clean-output
```

## Sample and tests

```
dotnet run --project samples/ComicVine.Api.Sample
dotnet test --solution ComicVineApi.slnx
```

Both read the key from `COMICVINE_API_KEY` or `secrets/apikey.txt` (git-ignored). Live tests skip when no key is found. Offline tests run without one.
