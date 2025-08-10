using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;   

internal sealed class SemanticBulkResponse
{
    [JsonPropertyName("data")] public List<SemanticPaperRaw>? Data { get; set; }
    [JsonPropertyName("total")] public int? Total { get; set; }
    [JsonPropertyName("token")] public string? Token { get; set; }
}

// Estrutura de um "paper" bruto do Semantic Scholar (campos que usamos)
public sealed class SemanticPaperRaw
{
    [JsonPropertyName("year")] public int? Year { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("tldr")] public SemanticTldr? Tldr { get; set; }
    [JsonPropertyName("publicationDate")] public string? PublicationDate { get; set; }
    [JsonPropertyName("authors")] public List<AuthorDto>? Authors { get; set; }
    [JsonPropertyName("publicationVenue")] public PublicationVenue? PublicationVenue { get; set; }
    [JsonPropertyName("citationCount")] public int? CitationCount { get; set; }
    [JsonPropertyName("influentialCitationCount")] public int? InfluentialCitationCount { get; set; }
    [JsonPropertyName("externalIds")] public ExternalIds? ExternalIds { get; set; }
    [JsonPropertyName("openAccessPdf")] public OpenAccessPdf? OpenAccessPdf { get; set; }
}
public sealed class SemanticTldr { [JsonPropertyName("text")] public string? Text { get; set; } }

public sealed class PublicationVenue
{
    [JsonPropertyName("issn")] public string? Issn { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
}

public sealed class ExternalIds { [JsonPropertyName("DOI")] public string? Doi { get; set; } }

public sealed class OpenAccessPdf { [JsonPropertyName("url")] public string? Url { get; set; } }