using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class SnowballingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public SnowballingService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        //_apiKey = config["API_KEY"] ?? throw new InvalidOperationException("API_KEY não configurada");
    }

    // Modelo simplificado de artigo
    public class ArticleInfo
    {
        public string? Title { get; set; }
        public string? Abstract { get; set; }
        public List<Author>? Authors { get; set; }
        public string? Url { get; set; }
        public string? DOI { get; set; }
        public int? Year { get; set; }
        public string? Issn { get; set; }
        public int? CitationCount { get; set; }
    }

    public class Author
    {
        public string? Name { get; set; }
    }

    public async Task<List<ArticleInfo>> GetCitationsByDoiAsync(string doi)
    {
        var url = $"https://api.semanticscholar.org/graph/v1/paper/DOI:{doi}/citations";
        var queryParams = "?fields=citingPaper.title,citingPaper.authors,citingPaper.openAccessPdf,citingPaper.year,citingPaper.externalIds,citingPaper.publicationVenue,citingPaper.abstract";

        var request = new HttpRequestMessage(HttpMethod.Get, url + queryParams);
        request.Headers.Add("x-api-key", _apiKey);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));

        var response = await _httpClient.SendAsync(request, cts.Token);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        var json = await JsonDocument.ParseAsync(stream, cancellationToken: cts.Token);

        var results = new List<ArticleInfo>();

        if (json.RootElement.TryGetProperty("data", out var dataArray))
        {
            foreach (var item in dataArray.EnumerateArray())
            {
                if (!item.TryGetProperty("citingPaper", out var paper) ||
                    paper.ValueKind != JsonValueKind.Object)
                {
                    // p.ex. { "citedPaper": null } -> ignora
                    continue;
                }

                // Log seguro (sem GetProperty direto)
                if (paper.TryGetProperty("title", out var titleEl) && titleEl.ValueKind == JsonValueKind.String)
                    Console.WriteLine("Processing paper: " + titleEl.GetString());

                results.Add(ParsePaper(paper));
            }
        }

        return results;
    }

    public async Task<List<ArticleInfo>> GetReferencesByDoiAsync(string doi)
    {
        var url = $"https://api.semanticscholar.org/graph/v1/paper/DOI:{doi}/references";
        var queryParams = "?fields=citedPaper.title,citedPaper.authors,citedPaper.openAccessPdf,citedPaper.year,citedPaper.externalIds,citedPaper.publicationVenue,citedPaper.abstract";

        var request = new HttpRequestMessage(HttpMethod.Get, url + queryParams);
        //request.Headers.Add("x-api-key", _apiKey);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));

        var response = await _httpClient.SendAsync(request, cts.Token);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cts.Token);

        var results = new List<ArticleInfo>();

        if (json.RootElement.TryGetProperty("data", out var dataArray) &&
            dataArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in dataArray.EnumerateArray())
            {
                if (!item.TryGetProperty("citedPaper", out var paper) ||
                    paper.ValueKind != JsonValueKind.Object)
                {
                    // p.ex. { "citedPaper": null } -> ignora
                    continue;
                }

                // Log seguro (sem GetProperty direto)
                if (paper.TryGetProperty("title", out var titleEl) && titleEl.ValueKind == JsonValueKind.String)
                    Console.WriteLine("Processing paper: " + titleEl.GetString());

                results.Add(ParsePaper(paper));
            }
        }

        return results;
    }


    public async Task<List<ArticleInfo>> GenerateSnowballingAsync(List<string> listaDeDois)
    {
        var allResults = new List<ArticleInfo>();
        Console.WriteLine("Entrou aqui", listaDeDois.Count);
        Console.WriteLine("Lista de DOIs: " + string.Join(", ", listaDeDois));
        foreach (var doi in listaDeDois)
        {
            if (string.IsNullOrWhiteSpace(doi))
                continue;

            var refs = await GetReferencesByDoiAsync(doi);
            allResults.AddRange(refs);

            var cits = await GetCitationsByDoiAsync(doi);
            allResults.AddRange(cits);
        }

        return allResults;
    }

    private ArticleInfo ParsePaper(JsonElement paper)
    {
        // Se não for objeto, devolve vazio (evita crash)
        if (paper.ValueKind != JsonValueKind.Object)
            return new ArticleInfo { Authors = new List<Author>() };

        // ---- DOI
        string? doi = null;
        if (paper.TryGetProperty("externalIds", out var externalIds) &&
            externalIds.ValueKind == JsonValueKind.Object &&
            externalIds.TryGetProperty("DOI", out var doiEl) &&
            doiEl.ValueKind == JsonValueKind.String)
        {
            doi = doiEl.GetString();
        }

        // ---- URL PDF
        string? url = null;
        if (paper.TryGetProperty("openAccessPdf", out var openAccessPdf) &&
            openAccessPdf.ValueKind == JsonValueKind.Object &&
            openAccessPdf.TryGetProperty("url", out var urlEl) &&
            urlEl.ValueKind == JsonValueKind.String)
        {
            url = urlEl.GetString();
        }

        // ---- ISSN
        string? issn = null;
        if (paper.TryGetProperty("publicationVenue", out var publicationVenue) &&
            publicationVenue.ValueKind == JsonValueKind.Object &&
            publicationVenue.TryGetProperty("issn", out var issnEl) &&
            issnEl.ValueKind == JsonValueKind.String)
        {
            issn = issnEl.GetString();
        }

        // ---- Título, abstract, ano
        string? title = null;
        if (paper.TryGetProperty("title", out var titleEl) &&
            titleEl.ValueKind == JsonValueKind.String)
            title = titleEl.GetString();

        string? abs = null;
        if (paper.TryGetProperty("abstract", out var absEl) &&
            absEl.ValueKind == JsonValueKind.String)
            abs = absEl.GetString();

        int? year = null;
        if (paper.TryGetProperty("year", out var yearEl) &&
            yearEl.ValueKind == JsonValueKind.Number)
            year = yearEl.GetInt32();

        // ---- Autores (array pode ser nulo; itens podem não ter "name")
        var authors = new List<Author>();
        if (paper.TryGetProperty("authors", out var authorsArray) &&
            authorsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var a in authorsArray.EnumerateArray())
            {
                string? name = null;
                if (a.ValueKind == JsonValueKind.Object &&
                    a.TryGetProperty("name", out var nameEl) &&
                    nameEl.ValueKind == JsonValueKind.String)
                {
                    name = nameEl.GetString();
                }
                authors.Add(new Author { Name = name });
            }
        }

        var citationCount = paper.TryGetProperty("citationCount", out var citationCountEl) &&
                            citationCountEl.ValueKind == JsonValueKind.Number
            ? citationCountEl.GetInt32()
            : (int?)null;

        return new ArticleInfo
        {
            Title = title,
            Abstract = abs,
            Authors = authors,
            Url = url,
            DOI = doi,
            Year = year,
            Issn = issn,
            CitationCount = citationCount
        };
    }


}
