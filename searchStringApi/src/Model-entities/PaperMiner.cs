using System.Text.Json.Serialization;


/*
            "source",
            "DOI",
            "title",
            "abstract",
            "authors",
            "url",
            "venue",
            "publicationDate",
            "citationCount",
            "type",
            "isOpenAccess",
            "relevanceMetric"
*/
public sealed class ArticlePaperMiner
{
    [JsonPropertyName("source")] public string Source { get; init; } = "semantic_scholar";
    [JsonPropertyName("DOI")] public string? Doi { get; init; }
    [JsonPropertyName("title")] public string? Title { get; init; }
    [JsonPropertyName("abstract")] public string? Abstract { get; init; }
    [JsonPropertyName("authors")] public List<string>? Authors { get; init; }
    [JsonPropertyName("url")] public string? Url { get; init; }
    [JsonPropertyName("venue")] public string? Venue { get; init; }
    [JsonPropertyName("publicationDate")] public string? PublicationDate { get; init; }
    [JsonPropertyName("CitationCount")] public int? CitationCount { get; init; }
    [JsonPropertyName("type")] public object? Type { get; init; }
    [JsonPropertyName("isOpenAccess")] public bool? IsOpenAccess { get; init; }
    [JsonPropertyName("relevanceMetric")] public int? RelevanceMetric { get; init; }
    
}
