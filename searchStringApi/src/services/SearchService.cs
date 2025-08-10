using System.Text.RegularExpressions;
using Microsoft.OpenApi.Any;
using MyApp.Services.Search;

namespace MyApp.Services;

public sealed class CombinePapersService
{
    private readonly Semantic _semantic;
    private readonly OpenAlex _openAlex;

    public CombinePapersService(Semantic semantic, OpenAlex openAlex)
    {
        _semantic = semantic ?? throw new ArgumentNullException(nameof(semantic));
        _openAlex = openAlex ?? throw new ArgumentNullException(nameof(openAlex));
    }

    public static IReadOnlyList<PaperMinerArticle> FilterArticlesWithPdf(IEnumerable<PaperMinerArticle> articles) =>
        articles
            .Where(a => !string.IsNullOrWhiteSpace(a.ApiPaperMiner.Url))
            .ToList();

    public static IReadOnlyList<PaperMinerArticle> FilterArticlesWithIssn(IEnumerable<PaperMinerArticle> articles) =>
        articles
            .Where(a => !string.IsNullOrWhiteSpace(a.ApiPaperMiner.Issn))
            .ToList();

    public static IReadOnlyList<PaperMinerArticle> FilterArticlesWithPreprint(IEnumerable<PaperMinerArticle> articles)
    {
        var list = new List<PaperMinerArticle>();
        foreach (var article in articles)
        {
            var content = article.ApiPaperMiner;
            var doi = (content.Doi ?? "").ToLowerInvariant();
            var type = (content.Type ?? "").ToLowerInvariant();

            if (doi.Contains("arxiv") || doi.Contains("preprint")) continue;
            if (type == "preprint") continue;

            list.Add(article);
        }
        return list;
    }

    private static int CountNonEmptyFields(ApiPaperMiner article)
    {
        return article.GetType()
            .GetProperties()
            .Select(p => p.GetValue(article))
            .Count(value =>
            {
                if (value == null) return false;

                if (value is string str)
                    return !string.IsNullOrWhiteSpace(str);

                if (value is ICollection<object> coll)
                    return coll.Count > 0;

                return true;
            });
    }

    public static IReadOnlyList<ApiPaperMiner> CombineResults(
        IEnumerable<PaperMinerArticle> semanticPapers,
        IEnumerable<PaperMinerArticle> openAlexPapers)
    {
        Console.WriteLine("Combinando resultados de ambas as fontes...");

        var combined = semanticPapers.Concat(openAlexPapers).ToList();
        var rnd = new Random();
        combined = combined.OrderBy(_ => rnd.Next()).ToList();

        var uniquePapers = new Dictionary<string, ApiPaperMiner>(StringComparer.OrdinalIgnoreCase);
        int duplicados = 0, semDoi = 0;

        foreach (var article in combined)
        {
            var content = article.ApiPaperMiner;
            var doi = content.Doi?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(doi))
            {
                semDoi++;
                var title = Regex.Replace(content.Title ?? "", @"\s+", " ").Trim().ToLowerInvariant();
                var year = (content.Year?.ToString() ?? "").Trim();
                var firstAuthor = content.Authors?.FirstOrDefault() is AuthorDto authDict
                    ? authDict.Name?.Trim().ToLowerInvariant()
                    : "unknown";

                doi = $"{title}_{year}_{firstAuthor}";
            }

            if (uniquePapers.ContainsKey(doi))
            {
                duplicados++;
                var existing = uniquePapers[doi];
                var countExisting = CountNonEmptyFields(existing);
                var countNew = CountNonEmptyFields(content);

                if (countNew > countExisting)
                    uniquePapers[doi] = content;
            }
            else
            {
                uniquePapers[doi] = content;
            }
        }

        Console.WriteLine($"Total de artigos únicos após combinação: {uniquePapers.Count}");
        Console.WriteLine($"Artigos considerados duplicados: {duplicados}");

        return uniquePapers.Values.ToList();
    }

    public static object GroupArticlesByKeyword(IEnumerable<ApiPaperMiner> papers, IEnumerable<string> keywords, int startYear, int endYear) =>
        new
        {
            keyword = keywords,
            start_year = startYear,
            end_year = endYear,
            articles = papers
        };

    // -----------------------
    // Execução principal
    // -----------------------
    public async Task<IReadOnlyList<ApiPaperMiner>> FetchAllPapersAsync(
        IEnumerable<string> keywords,
        int startYear,
        int endYear,
        bool withPdf,
        bool withIssn,
        int securityLimit,
        bool withPreprint,
        CancellationToken cancellationToken = default)
    {
        var semanticPapers = new List<PaperMinerArticle>();
        var openAlexPapers = new List<PaperMinerArticle>();

        foreach (var keyword in keywords)
        {
            Console.WriteLine($"Starting data retrieval for keyword: {keyword}");

            var semanticResults = await _semantic.FetchSemanticAsync(
                new Dictionary<string, List<string>> { { "default", new List<string> { keyword } } },
                startYear,
                endYear,
                securityLimit + 100,
                cancellationToken);

            var openAlexResults = await _openAlex.FetchOpenAlexAsync(
                new Dictionary<string, List<string>> { { "default", new List<string> { keyword } } },
                startYear,
                endYear,
                securityLimit + 100,
                cancellationToken);

            semanticPapers.AddRange(semanticResults);
            openAlexPapers.AddRange(openAlexResults);
        }

        if (withPdf)
        {
            Console.WriteLine("Filtering papers with PDF availability...");
            semanticPapers = FilterArticlesWithPdf(semanticPapers).ToList();
            openAlexPapers = FilterArticlesWithPdf(openAlexPapers).ToList();
        }

        if (withIssn)
        {
            Console.WriteLine("Filtering papers with ISSN availability...");
            semanticPapers = FilterArticlesWithIssn(semanticPapers).ToList();
            openAlexPapers = FilterArticlesWithIssn(openAlexPapers).ToList();
        }

        if (!withPreprint)
        {
            Console.WriteLine("Filtering papers without preprint ...");
            semanticPapers = FilterArticlesWithPreprint(semanticPapers).ToList();
            openAlexPapers = FilterArticlesWithPreprint(openAlexPapers).ToList();
        }

        var combinedPapers = CombineResults(semanticPapers, openAlexPapers);

        var finalData = GroupArticlesByKeyword(combinedPapers.Take(securityLimit), keywords, startYear, endYear);

        Console.WriteLine("All results have been saved.");

        return combinedPapers.Take(securityLimit).ToList();
    }
}
