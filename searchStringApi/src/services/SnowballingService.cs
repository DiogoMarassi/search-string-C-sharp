using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using MyApp.Models; // onde está ArticleSemanticRaw
using MyApp.Services.Utils;
using MyApp.DTOs;

public class SnowballingService
{
    private readonly HttpClient _httpClient;

    public SnowballingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ArticleInfo>> GetCitationsByDoiAsync(string doi)
    {
        var url = $"https://api.semanticscholar.org/graph/v1/paper/DOI:{doi}/citations" +
                  "?fields=citingPaper.title,citingPaper.authors,citingPaper.openAccessPdf," +
                  "citingPaper.year,citingPaper.externalIds,citingPaper.publicationVenue," +
                  "citingPaper.abstract,citingPaper.publicationTypes,citingPaper.publicationDate," +
                  "citingPaper.isOpenAccess,citingPaper.influentialCitationCount,citingPaper.citationCount";

        var response = await _httpClient.GetFromJsonAsync<SemanticSnowballResponse>(url);

        return response?.Data?
            .Select(d => d.CitingPaper)
            .Where(p => p != null)
            .Select(TransformPaper!)
            .ToList() ?? new();
    }

    public async Task<List<ArticleInfo>> GetReferencesByDoiAsync(string doi)
    {
        var url = $"https://api.semanticscholar.org/graph/v1/paper/DOI:{doi}/references" +
                  "?fields=citedPaper.title,citedPaper.authors,citedPaper.openAccessPdf," +
                  "citedPaper.year,citedPaper.externalIds,citedPaper.publicationVenue," +
                  "citedPaper.abstract,citedPaper.publicationTypes,citedPaper.publicationDate," +
                  "citedPaper.isOpenAccess,citedPaper.influentialCitationCount,citedPaper.citationCount";

        var response = await _httpClient.GetFromJsonAsync<SemanticSnowballResponse>(url);

        return response?.Data?
            .Select(d => d.CitedPaper)
            .Where(p => p != null)
            .Select(TransformPaper!)
            .ToList() ?? new();
    }

    public async Task<List<ArticleInfo>> GenerateSnowballingAsync(List<string> dois)
    {
        var allResults = new List<ArticleInfo>();

        foreach (var doi in dois)
        {
            if (string.IsNullOrWhiteSpace(doi)) continue;

            var refs = await GetReferencesByDoiAsync(doi);
            var cits = await GetCitationsByDoiAsync(doi);

            allResults.AddRange(refs);
            allResults.AddRange(cits);
        }

        return allResults;
    }

    private static ArticleInfo TransformPaper(ArticleSemanticRaw paper)
    {
        var publicationVenue = paper.PublicationVenue ?? new();
        return new ArticleInfo
        {
            DOI = paper.ExternalIds?.DOI,
            Title = paper.Title,
            Snippet = paper.Abstract,
            Authors = paper.Authors?.Select(a => new Author { Name = a.Name ?? "" }).ToList(),
            Url = paper.OpenAccessPdf?.Url,
            Venue = paper.Venue ?? publicationVenue.Name,
            PublicationDate = paper.PublicationDate,
            CitationCount = paper.CitationCount ?? 0,
            Type = paper.PublicationTypes,
            IsOpenAccess = paper.IsOpenAccess == true,
            RelevanceMetric = paper.InfluentialCitationCount ?? 0
        };
    }
}
