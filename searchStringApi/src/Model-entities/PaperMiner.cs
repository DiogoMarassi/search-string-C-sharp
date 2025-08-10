using System.Text.Json.Serialization;

public sealed class PaperMinerArticle
{
    // Precisamos serializar com nome "API PaperMiner"
    [JsonPropertyName("API PaperMiner")]
    public ApiPaperMiner ApiPaperMiner { get; init; } = new();
}
public sealed class AuthorDto
{
    [JsonPropertyName("authorId")] public string? AuthorId { get; init; }
    [JsonPropertyName("name")]     public string? Name { get; init; }
}

public sealed class VenueDto
{
    [JsonPropertyName("issn")]       public string? Issn { get; init; }
    [JsonPropertyName("source_name")]public string? SourceName { get; init; }
    [JsonPropertyName("type")]       public string? Type { get; init; }
}

public sealed class OpenAccessPdfDto
{
    [JsonPropertyName("url")] public string? Url { get; init; }
}

public sealed class ApiPaperMiner
{
    [JsonPropertyName("engines")] public List<string> Engines { get; init; } = new();
    [JsonPropertyName("year")] public int? Year { get; init; }
    [JsonPropertyName("title")] public string? Title { get; init; }
    [JsonPropertyName("snippet")] public string? Snippet { get; init; }
    [JsonPropertyName("publication_date")] public string? PublicationDate { get; init; }

    // Troque List<object> -> List<AuthorDto>
    [JsonPropertyName("authors")] public List<AuthorDto>? Authors { get; init; }

    [JsonPropertyName("issn")] public string? Issn { get; init; }
    [JsonPropertyName("CitationCount")] public int? CitationCount { get; init; }
    [JsonPropertyName("influentialCitationCount")] public int? InfluentialCitationCount { get; init; }
    [JsonPropertyName("DOI")] public string? Doi { get; init; }
    [JsonPropertyName("url")] public string? Url { get; init; }

    [JsonPropertyName("source_name")] public string SourceName { get; init; } = "No data";
    [JsonPropertyName("type")] public string? Type { get; init; }

    // mantenha “source” como enum/string
    [JsonPropertyName("source")] public string Source { get; init; } = "semantic_scholar";

    // (opcional) normalize também abstract
    [JsonPropertyName("abstract")] public string? Abstract { get; init; }
}
