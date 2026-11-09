using System;
using System.IO;
using System.Threading.Tasks;
using TUnit.Core;

namespace ComicVine.Api.Tests;

internal static class LiveApiKey
{
    public static string? Find()
    {
        var fromEnv = Environment.GetEnvironmentVariable("COMICVINE_API_KEY");
        if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv.Trim();

        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "secrets", "apikey.txt");
            if (File.Exists(candidate))
            {
                var text = File.ReadAllText(candidate).Trim();
                return text.Length == 0 ? null : text;
            }
        }
        return null;
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
internal sealed class RequiresApiKeyAttribute : SkipAttribute
{
    public RequiresApiKeyAttribute() : base("No Comic Vine API key (COMICVINE_API_KEY or secrets/apikey.txt)") { }

    public override Task<bool> ShouldSkip(TestRegisteredContext context) =>
        Task.FromResult(LiveApiKey.Find() is null);
}
