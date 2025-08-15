using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;   

internal sealed class SemanticBulkResponse
{
    [JsonPropertyName("data")] public List<ArticleSemanticRaw>? Data { get; set; }
    [JsonPropertyName("total")] public int? Total { get; set; }
    [JsonPropertyName("token")] public string? Token { get; set; }
}

public class ArticleSemanticRaw
{
    public string? PaperId { get; set; }
    public long? CorpusId { get; set; }
    public ExternalIds? ExternalIds { get; set; }
    public string? Url { get; set; }
    public string? Title { get; set; }
    public string? Abstract { get; set; }
    public string? Venue { get; set; }
    public PublicationVenue? PublicationVenue { get; set; }
    public int? Year { get; set; }
    public int? ReferenceCount { get; set; }
    public int? CitationCount { get; set; }
    public int? InfluentialCitationCount { get; set; }
    public bool? IsOpenAccess { get; set; }
    public OpenAccessPdf? OpenAccessPdf { get; set; }
    public List<string>? FieldsOfStudy { get; set; }
    public List<S2FieldOfStudy>? S2FieldsOfStudy { get; set; }
    public List<string>? PublicationTypes { get; set; }
    public string? PublicationDate { get; set; }
    public JournalInfo? Journal { get; set; }
    public Dictionary<string, object>? CitationStyles { get; set; }
    public List<AuthorInfo>? Authors { get; set; }
}

public class ExternalIds
{
    public string? MAG { get; set; }
    public string? DBLP { get; set; }
    public string? ACL { get; set; }
    public string? DOI { get; set; }
    public long? CorpusId { get; set; }
}

public class PublicationVenue
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public List<string>? Alternate_Names { get; set; }
    public string? Url { get; set; }
}

public class OpenAccessPdf
{
    public string? Url { get; set; }
    public string? Status { get; set; }
    public string? License { get; set; }
    public string? Disclaimer { get; set; }
}

public class S2FieldOfStudy
{
    public string? Category { get; set; }
}

public class JournalInfo
{
    public string? Volume { get; set; }
    public string? Pages { get; set; }
    public string? Name { get; set; }
}

public class AuthorInfo
{
    public string? AuthorId { get; set; }
    public string? Name { get; set; }
}
