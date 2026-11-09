using System;
using System.IO;

namespace ComicVine.Api.Sample;

internal static class ApiKeySource
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
