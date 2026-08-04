using System.Text.RegularExpressions;

namespace KPW.Infrastructure.Services.Ai;

internal static partial class EducationChunkRetriever
{
    public static IReadOnlyList<EducationChunk> RetrieveTopChunks(
        IReadOnlyList<EducationChunk> chunks,
        string message,
        int maxResults = 3)
    {
        var tokens = Tokenize(message);
        if (tokens.Count == 0)
        {
            return [];
        }

        return chunks
            .Select(chunk => new ScoredChunk(chunk, ScoreChunk(tokens, chunk)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(maxResults)
            .Select(x => x.Chunk)
            .ToList();
    }

    public static string BuildGroundingContext(IReadOnlyList<EducationChunk> chunks) =>
        string.Join("\n\n", chunks.Select(c => $"[{c.Title}]\n{c.Content}"));

    private static double ScoreChunk(HashSet<string> queryTokens, EducationChunk chunk)
    {
        var chunkTokens = Tokenize(chunk.Content + " " + chunk.Title);
        if (chunkTokens.Count == 0)
        {
            return 0;
        }

        var overlap = queryTokens.Count(t => chunkTokens.Contains(t));
        return overlap / Math.Sqrt(chunkTokens.Count);
    }

    private static HashSet<string> Tokenize(string text) =>
        WordRegex()
            .Matches(text.ToLowerInvariant())
            .Select(m => m.Value)
            .Where(w => w.Length > 2)
            .ToHashSet();

    private record ScoredChunk(EducationChunk Chunk, double Score);

    [GeneratedRegex(@"\b[a-z0-9']+\b", RegexOptions.Compiled)]
    private static partial Regex WordRegex();
}
