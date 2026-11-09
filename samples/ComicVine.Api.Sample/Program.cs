using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ComicVine.Api;
using ComicVine.Api.Client.Models;
using ComicVine.Api.Client.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using Spectre.Console;

var apiKey = ComicVine.Api.Sample.ApiKeySource.Find();
if (apiKey is null)
{
    AnsiConsole.MarkupLine("[red]No API key found.[/] Set COMICVINE_API_KEY or put it in secrets/apikey.txt.");
    return 1;
}

var services = new ServiceCollection();
services.AddComicVineClient(o =>
{
    o.ApiKey = apiKey;
    o.UserAgent = "ComicVine.Api.Sample";
});
using var provider = services.BuildServiceProvider();
var comicVine = provider.GetRequiredService<IComicVineApi>();

static string E(string? text) => Markup.Escape(text ?? string.Empty);

try
{
    AnsiConsole.Write(new Rule("Publishers (page 1, 5 per page)").LeftJustified());
    var publishers = await AnsiConsole.Status().StartAsync("Loading publishers...", _ =>
        comicVine.Publishers.ListAsync(new ListOptions { PageSize = 5 }.Select("id", "name", "location_city")));
    var publisherTable = new Table().RoundedBorder().AddColumn("Id").AddColumn("Name").AddColumn("City");
    foreach (var p in publishers.Items)
        publisherTable.AddRow($"{p.Id}", E(p.Name), E(p.LocationCity));
    publisherTable.Caption($"{publishers.TotalCount} publishers in total, more pages: {publishers.HasNext}");
    AnsiConsole.Write(publisherTable);

    AnsiConsole.Write(new Rule("Search: \"batman\" characters (streamed, first 5)").LeftJustified());
    var found = await AnsiConsole.Status().StartAsync("Searching...", async _ =>
    {
        var list = new List<SearchResult>();
        await foreach (var r in comicVine.Search.StreamAsync("batman", new[] { GetResourcesQueryParameterType.Character }))
        {
            list.Add(r);
            if (list.Count == 5) break;
        }
        return list;
    });
    var searchTable = new Table().RoundedBorder().AddColumn("Id").AddColumn("Name").AddColumn("Type");
    foreach (var r in found)
        searchTable.AddRow($"{r.Id}", E(r.Name), E(r.ResourceType));
    AnsiConsole.Write(searchTable);

    AnsiConsole.Write(new Rule("Volumes named \"Batman\" (most issues first)").LeftJustified());
    var volumes = await AnsiConsole.Status().StartAsync("Loading volumes...", _ =>
        comicVine.Volumes.ListAsync(new ListOptions { PageSize = 5 }
            .Where("name", "Batman")
            .SortBy("count_of_issues", descending: true)
            .Select("id", "name", "start_year", "count_of_issues", "publisher")));
    var volumeTable = new Table().RoundedBorder()
        .AddColumn("Id").AddColumn("Volume").AddColumn("Start").AddColumn("Publisher").AddColumn(new TableColumn("Issues").RightAligned());
    foreach (var v in volumes.Items)
        volumeTable.AddRow($"{v.Id}", E(v.Name), E(v.StartYear), E(v.Publisher?.Name), $"{v.CountOfIssues}");
    AnsiConsole.Write(volumeTable);

    var first = volumes.Items.FirstOrDefault();
    if (first?.Id is not int volumeId)
    {
        AnsiConsole.MarkupLine("[yellow]No volumes found, skipping issue calls.[/]");
        return 0;
    }

    AnsiConsole.Write(new Rule($"Issues of [[{volumeId}]] {E(first.Name)} (first 5)").LeftJustified());
    var issues = await AnsiConsole.Status().StartAsync("Loading issues...", _ =>
        comicVine.Issues.ListAsync(new ListOptions { PageSize = 5 }
            .Where("volume", volumeId)
            .SortBy("cover_date")
            .Select("id", "name", "issue_number", "cover_date")));
    var issueTable = new Table().RoundedBorder().AddColumn("Id").AddColumn("#").AddColumn("Name").AddColumn("Cover date");
    foreach (var i in issues.Items)
        issueTable.AddRow($"{i.Id}", E(i.IssueNumber), E(i.Name), E(i.CoverDate));
    AnsiConsole.Write(issueTable);

    if (issues.Items.FirstOrDefault()?.Id is int issueId)
    {
        var detail = await AnsiConsole.Status().StartAsync("Loading issue detail...", _ => comicVine.Issues.GetAsync(issueId));
        var grid = new Grid().AddColumn().AddColumn();
        grid.AddRow("[grey]Name[/]", E(detail.Name));
        grid.AddRow("[grey]Volume[/]", E(detail.Volume?.Name));
        grid.AddRow("[grey]Cover[/]", E(detail.CoverDate));
        grid.AddRow("[grey]Credits[/]", $"people: {detail.PersonCredits?.Count ?? 0}, characters: {detail.CharacterCredits?.Count ?? 0}");
        AnsiConsole.Write(new Panel(grid).Header($"Issue detail [[{detail.Id}]]").RoundedBorder());
    }
    return 0;
}
catch (ApiException ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.TooManyRequests)
{
    AnsiConsole.MarkupLine("[red]Rate limited by Comic Vine (HTTP 429).[/] Wait a while and retry.");
    return 2;
}
catch (ApiException ex) when (ex.ResponseStatusCode is 401 or 403)
{
    AnsiConsole.MarkupLine($"[red]Comic Vine rejected the API key (HTTP {ex.ResponseStatusCode}).[/]");
    return 3;
}
catch (ApiException ex)
{
    AnsiConsole.MarkupLine($"[red]Comic Vine returned HTTP {ex.ResponseStatusCode}:[/] {E(ex.Message)}");
    return 4;
}
