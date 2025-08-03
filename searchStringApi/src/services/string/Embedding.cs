using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;
namespace MyApp.Services.String;

/// Serviço para gerar embeddings reais usando a API do Nomic.
public class Embedding
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public Embedding(string apiKey, HttpClient? httpClient = null)
    {
        _apiKey = apiKey;
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.BaseAddress = new Uri("https://api-atlas.nomic.ai/v1/embedding/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    /// Gera embeddings para uma lista de textos usando o modelo Nomic.
    public async Task<List<float[]>> GenerateAsync(List<string> texts)
    {
        var request = new
        {
            texts = texts,
            task_type = "search_document",
            max_tokens_per_text = 8192,
            dimensionality = 768
        };
        //search_document (embedding document chunks for search & retrieval)
        //search_query (embedding queries for search & retrieval)
        //classification (embeddings for text classification)
        //clustering (embeddings for cluster visualization)

        // Envia POST
        var response = await _httpClient.PostAsJsonAsync("text", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Erro na API Nomic: {response.StatusCode} - {error}");
        }

        // Desserializa JSON
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        // A resposta possui "embeddings" como array de arrays
        var embeddings = new List<float[]>();
        if (json.TryGetProperty("embeddings", out var embeddingsArray))
        {
            foreach (var vector in embeddingsArray.EnumerateArray())
            {
                embeddings.Add(vector.EnumerateArray().Select(v => v.GetSingle()).ToArray());
            }
        }

        return embeddings;
    }
}
