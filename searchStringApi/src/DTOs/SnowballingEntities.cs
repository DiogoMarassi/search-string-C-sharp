namespace MyApp.DTOs;

public class SemanticSnowballResponse
{
    public List<SemanticEdge>? Data { get; set; }
}

public class SemanticEdge
{
    public ArticleSemanticRaw? CitingPaper { get; set; }
    public ArticleSemanticRaw? CitedPaper { get; set; }
}
public class ArticleInfo
    {
        public string Source { get; set; } = "semantic_scholar";
        public string? DOI { get; set; }
        public string? Title { get; set; }
        public string? Snippet { get; set; }
        public List<Author>? Authors { get; set; }
        public string? Url { get; set; }
        public string? Venue { get; set; }
        public string? PublicationDate { get; set; }
        public int CitationCount { get; set; }
        public List<string>? Type { get; set; }
        public bool IsOpenAccess { get; set; }
        public int RelevanceMetric { get; set; }
    }

    public class Author
    {
        public string? Name { get; set; }
    }