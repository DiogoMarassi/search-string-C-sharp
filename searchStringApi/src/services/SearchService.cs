using System.Text.Json;

namespace MyApp.Services;

public class SearchService
{
    public async Task<List<JsonElement>> Search(string query, int startYear, int endYear, string publicationTypes)
    {
        var apiKey = Environment.GetEnvironmentVariable("S2_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("API key not found in environment variables.");

        using var http = new HttpClient();
        //http.DefaultRequestHeaders.Add("x-api-key", apiKey);

        string? token = null;
        var allResults = new List<JsonElement>();

        do
        {
            var url = "https://api.semanticscholar.org/graph/v1/paper/search/bulk" +
                      $"?query={Uri.EscapeDataString(query)}" +
                      "&fields=paperId,title,year,authors,url,fieldsOfStudy,openAccessPdf,isOpenAccess,citationCount,referenceCount" +
                      "&limit=1000" +
                      $"&publicationTypes={Uri.EscapeDataString(publicationTypes)}" +
                      "&fieldsOfStudy=Medicine,Public%20Health" +
                      $"&year={startYear}-{endYear}" +
                      (token != null ? $"&token={token}" : "");

            var json = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Adiciona todos os papers da página
            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                allResults.AddRange(data.EnumerateArray());
            }

            // Atualiza token para próxima página
            token = root.TryGetProperty("token", out var tokenElement) ? tokenElement.GetString() : null;

        } while (token != null);

        return allResults;
    }
}
