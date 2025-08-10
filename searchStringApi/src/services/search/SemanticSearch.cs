using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyApp.Services.Search;

public sealed class Semantic
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // Recomenda-se injetar via IHttpClientFactory (AddHttpClient<SemanticService>())
    public Semantic(HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        // BaseAddress opcional: _http.BaseAddress = new Uri("https://api.semanticscholar.org");
        // Mas vou usar URL absoluta no método para ficar explícito.
    }

    public async Task<IReadOnlyList<PaperMinerArticle>> FetchSemanticAsync(
        Dictionary<string, List<string>> clusters,
        int startYear,
        int endYear,
        int securityLimit,
        CancellationToken cancellationToken = default)
    {
        if (securityLimit <= 0) return Array.Empty<PaperMinerArticle>();

        var formattedQuery = FormatSemanticQuery(clusters);
        var encodedQuery = Uri.EscapeDataString(formattedQuery);

        var fields = string.Join(",",
            "title",
            "year",
            "authors",
            "openAccessPdf",
            "externalIds",
            "publicationVenue",
            "citationCount",
            "influentialCitationCount"
        );

        var baseUrl =
            $"https://api.semanticscholar.org/graph/v1/paper/search/bulk?query={encodedQuery}&fields={fields}&year={startYear}-{endYear}";

        var accumulated = new List<SemanticPaperRaw>(capacity: Math.Min(securityLimit, 1024));
        string? token = null;

        try
        {
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                var url = token is null ? baseUrl : $"{baseUrl}&token={Uri.EscapeDataString(token)}";

                using var resp = await _http.GetAsync(url, cancellationToken);
                resp.EnsureSuccessStatusCode();

                await using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
                var payload = await JsonSerializer.DeserializeAsync<SemanticBulkResponse>(stream, JsonOpts, cancellationToken)
                               ?? new SemanticBulkResponse();

                var batch = payload.Data ?? new List<SemanticPaperRaw>();
                if (batch.Count > 0)
                {
                    // respeita o securityLimit
                    var remaining = securityLimit - accumulated.Count;
                    if (remaining <= 0) break;

                    if (batch.Count > remaining)
                        accumulated.AddRange(batch.Take(remaining));
                    else
                        accumulated.AddRange(batch);
                }

                token = payload.Token;

            } while (token is not null && accumulated.Count < securityLimit);
        }
        catch (HttpRequestException ex)
        {
            // Logue isso no seu logger; aqui devolvo parcial
            Console.Error.WriteLine($"[SemanticService] HttpRequestException: {ex.Message}");
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // cancelado pelo chamador — devolve parcial
        }

        return TransformArticlesSemantic(accumulated);
    }

    public IReadOnlyList<PaperMinerArticle> TransformArticlesSemantic(IEnumerable<SemanticPaperRaw> articles) =>
        articles.Select(TransformSemanticPaper).ToList();

    private static PaperMinerArticle TransformSemanticPaper(SemanticPaperRaw paper)
    {
        var doiLower = (paper.ExternalIds?.Doi ?? string.Empty).ToLowerInvariant();
        var publicationVenue = paper.PublicationVenue ?? new PublicationVenue();
        var articleType = doiLower.Contains("arxiv") ? "preprint" : "unknown"; // mantido para paridade

        return new PaperMinerArticle
        {
            ApiPaperMiner = new ApiPaperMiner
            {
                Engines = new List<string>(),
                Year = paper.Year,
                Title = paper.Title,
                Snippet = paper.Tldr?.Text,
                PublicationDate = paper.PublicationDate,
                Authors = paper.Authors,
                Issn = publicationVenue.Issn,
                CitationCount = paper.CitationCount,
                InfluentialCitationCount = paper.InfluentialCitationCount,
                Doi = paper.ExternalIds?.Doi,
                Url = paper.OpenAccessPdf?.Url,
                SourceName = publicationVenue.Name ?? "No data",
                Type = publicationVenue.Type,
                Source = "semantic_scholar"
            }
        };
    }

    /// Formata clusters no padrão: "('a'|'b')" + "('c'|'d')" e, se existir, exclui com -('x'|'y').
    /// Ignora key "not" na parte positiva.
    private static string FormatSemanticQuery(Dictionary<string, List<string>> clusters)
    {
        if (clusters is null || clusters.Count == 0)
            return string.Empty;

        static string Group(List<string> terms) =>
            "('" + string.Join("'|'", terms.Where(t => !string.IsNullOrWhiteSpace(t))) + "')";

        var positives = clusters
            .Where(kv => !string.Equals(kv.Key, "not", StringComparison.OrdinalIgnoreCase))
            .Select(kv => Group(kv.Value))
            .ToList();

        var sb = new StringBuilder();
        sb.Append(string.Join(" + ", positives));

        if (clusters.TryGetValue("not", out var negatives) && negatives.Count > 0)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append('-').Append(Group(negatives));
        }

        return sb.ToString();
    }
}
