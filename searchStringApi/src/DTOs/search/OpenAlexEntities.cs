using System.Text.Json.Serialization;

internal sealed class OpenAlexResponse
{
    [JsonPropertyName("results")] public List<OpenAlexPaperRaw>? Results { get; set; }
    [JsonPropertyName("meta")] public OpenAlexMeta? Meta { get; set; }
}

internal sealed class OpenAlexMeta
{
    [JsonPropertyName("next_cursor")] public string? NextCursor { get; set; }
}

public sealed class OpenAlexPaperRaw
{
    [JsonPropertyName("publication_year")] public int? PublicationYear { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("authorships")] public List<OpenAlexAuthorship>? Authorships { get; set; }
    [JsonPropertyName("publication_date")] public string? PublicationDate { get; set; }
    [JsonPropertyName("abstract_inverted_index")] public Dictionary<string, List<int>>? AbstractInvertedIndex { get; set; }
    [JsonPropertyName("primary_location")] public OpenAlexPrimaryLocation? PrimaryLocation { get; set; }
    [JsonPropertyName("best_oa_location")] public OpenAlexOALocation? BestOaLocation { get; set; }
    [JsonPropertyName("open_access")] public OpenAlexOpenAccess? OpenAccess { get; set; }
    [JsonPropertyName("cited_by_count")] public int? CitedByCount { get; set; }
    [JsonPropertyName("score")] public int? Score { get; set; }
    [JsonPropertyName("doi")] public string? Doi { get; set; }
    [JsonPropertyName("type_crossref")] public string? TypeCrossref { get; set; }
}

public sealed class OpenAlexAuthorship
{
    [JsonPropertyName("author")] public OpenAlexAuthor? Author { get; set; }
}

public sealed class OpenAlexAuthor
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("display_name")] public string? DisplayName { get; set; }
}

public sealed class OpenAlexPrimaryLocation
{
    [JsonPropertyName("source")] public OpenAlexSource? Source { get; set; }
    [JsonPropertyName("pdf_url")] public string? PdfUrl { get; set; }
}

public sealed class OpenAlexOALocation
{
    [JsonPropertyName("is_oa")] public bool? IsOa { get; set; }
    [JsonPropertyName("pdf_url")] public string? PdfUrl { get; set; }
}

public sealed class OpenAlexOpenAccess
{
    [JsonPropertyName("is_oa")] public bool? IsOa { get; set; }
    [JsonPropertyName("oa_status")] public string? OaStatus { get; set; }
}

public sealed class OpenAlexSource
{
    [JsonPropertyName("issn_l")] public string? IssnL { get; set; }
    [JsonPropertyName("display_name")] public string? DisplayName { get; set; }
}
