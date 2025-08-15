using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MyApp.Services.Search;

public sealed class OpenAlex
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenAlex(HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _http.BaseAddress = new Uri("https://api.openalex.org");
    }

    public async Task<IReadOnlyList<ArticlePaperMiner>> FetchOpenAlexAsync(
        Dictionary<string, List<string>> clusters,
        int startYear,
        int endYear,
        int securityLimit,
        CancellationToken cancellationToken = default)
    {
        if (securityLimit <= 0) return Array.Empty<ArticlePaperMiner>();

        var formattedQuery = FormatOpenAlexQuery(clusters);
        var allPapers = new List<OpenAlexPaperRaw>(Math.Min(securityLimit, 1024));

        string? cursor = "*";

        try
        {
            while (allPapers.Count < securityLimit && cursor is not null)
            {
                var remaining = securityLimit - allPapers.Count;

                var url = $"/works?filter={Uri.EscapeDataString(formattedQuery)},publication_year:{startYear}|{endYear}" +
                          $"&per-page={Math.Min(200, remaining)}&cursor={Uri.EscapeDataString(cursor)}";

                using var resp = await _http.GetAsync(url, cancellationToken);
                resp.EnsureSuccessStatusCode();

                await using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
                var payload = await JsonSerializer.DeserializeAsync<OpenAlexResponse>(stream, JsonOpts, cancellationToken)
                               ?? new OpenAlexResponse();

                if (payload.Results is { Count: > 0 })
                {
                    allPapers.AddRange(payload.Results);
                    cursor = payload.Meta?.NextCursor;
                }
                else
                {
                    break; // sem resultados
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"[OpenAlexService] HttpRequestException: {ex.Message}");
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // cancelado pelo chamador
        }

        return TransformArticlesOpen(allPapers.Take(securityLimit));
    }

    public IReadOnlyList<ArticlePaperMiner> TransformArticlesOpen(IEnumerable<OpenAlexPaperRaw> articles) =>
        articles.Select(TransformOpenAlexPaper).ToList();

    private static ArticlePaperMiner TransformOpenAlexPaper(OpenAlexPaperRaw paper)
    {
        var primaryLocation = paper.PrimaryLocation ?? new OpenAlexPrimaryLocation();
        var source = primaryLocation.Source ?? new OpenAlexSource();

        // extrai DOI com regex
        string? doi = null;
        if (!string.IsNullOrWhiteSpace(paper.Doi))
        {
            var match = Regex.Match(paper.Doi, @"10\.\d{4,9}/[-._;()/:A-Z0-9]+", RegexOptions.IgnoreCase);
            if (match.Success) doi = match.Value;
        }

        return new ArticlePaperMiner
        {
            Source = "open_alex",
            Doi = doi,
            Title = paper.Title,
            Abstract = DecodeOpenAlexAbstract(paper.AbstractInvertedIndex),
            Authors = paper.Authorships?.Select(a => a.Author?.DisplayName ?? "").ToList(),
            Url = primaryLocation.PdfUrl,
            Venue = paper.PrimaryLocation?.Source?.DisplayName ?? "",
            PublicationDate = paper.PublicationDate,
            CitationCount = paper.CitedByCount ?? 0,
            Type = paper.TypeCrossref ?? null,
            IsOpenAccess = (
                paper.OpenAccess?.IsOa == true
                || paper.BestOaLocation?.IsOa == true
                || (!string.IsNullOrWhiteSpace(paper.OpenAccess?.OaStatus) &&
                    new[] { "gold", "green", "bronze", "hybrid" }
                        .Contains(paper.OpenAccess.OaStatus.ToLower()))
            )
            && (
                !string.IsNullOrWhiteSpace(paper.BestOaLocation?.PdfUrl)
                || !string.IsNullOrWhiteSpace(paper.PrimaryLocation?.PdfUrl)
            ),
            RelevanceMetric = paper.Score ?? 0,
        };
    }

    private static string? DecodeOpenAlexAbstract(Dictionary<string, List<int>>? invertedIndex)
    {
        if (invertedIndex is null || invertedIndex.Count == 0)
            return null;

        var wordPositions = new List<(int pos, string word)>();

        foreach (var kvp in invertedIndex)
        {
            foreach (var pos in kvp.Value)
                wordPositions.Add((pos, kvp.Key));
        }

        var sortedWords = wordPositions
            .OrderBy(wp => wp.pos)
            .Select(wp => wp.word);

        return string.Join(" ", sortedWords);
    }

    private static string FormatOpenAlexQuery(Dictionary<string, List<string>> clusters)
    {
        if (clusters is null || clusters.Count == 0)
            return string.Empty;

        static string Group(List<string> terms) =>
            "abstract.search:\"('" + string.Join("'|'", terms.Where(t => !string.IsNullOrWhiteSpace(t))) + "')\"";

        var positives = clusters
            .Where(kv => !string.Equals(kv.Key, "not", StringComparison.OrdinalIgnoreCase))
            .Select(kv => Group(kv.Value))
            .ToList();

        var sb = new StringBuilder();
        sb.Append(string.Join(",", positives));

        if (clusters.TryGetValue("not", out var negatives) && negatives.Count > 0)
        {
            if (sb.Length > 0) sb.Append(',');
            sb.Append("abstract.search:\"!('").Append(string.Join("'|'", negatives)).Append("')\"");
        }

        return sb.ToString();
    }
}
