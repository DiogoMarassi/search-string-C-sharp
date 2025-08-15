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


    private static int CountNonEmptyFields(ArticlePaperMiner article)
    {
        return article.GetType()
            .GetProperties()
            .Select(p => p.GetValue(article))
            .Count(value =>
            {
                if (value == null) return false;

                if (value is string str)
                    return !string.IsNullOrWhiteSpace(str);

                if (value is System.Collections.ICollection coll)
                    return coll.Count > 0;

                return true;
            });
    }


    public static IReadOnlyList<ArticlePaperMiner> CombineResults(
        IEnumerable<ArticlePaperMiner> semanticPapers,
        IEnumerable<ArticlePaperMiner> openAlexPapers)
    {
        Console.WriteLine("Combinando resultados de ambas as fontes...");

        var combined = semanticPapers.Concat(openAlexPapers).ToList();

        // Embaralhar para não privilegiar uma fonte
        var rnd = new Random();
        combined = combined.OrderBy(_ => rnd.Next()).ToList();

        var uniquePapers = new Dictionary<string, ArticlePaperMiner>();
        int duplicados = 0, semDoi = 0;

        foreach (var content in combined)
        {
            var doi = content.Doi?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(doi))
            {
                semDoi++;
                var title = Regex.Replace(content.Title ?? "", @"\s+", " ").Trim().ToLowerInvariant();
                var date = content.PublicationDate ?? "";
                doi = $"{title}_{date}"; // Usa como chave fake
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
        Console.WriteLine($"Artigos sem DOI: {semDoi}");

        return uniquePapers.Values.ToList();
    }


    public static object GroupArticlesByKeyword(IReadOnlyList<ArticlePaperMiner> papers, IEnumerable<string> keywords, int startYear, int endYear) =>
        new
        {
            keyword = keywords,
            start_year = startYear,
            end_year = endYear,
            articles = papers
        };

    public async Task<IReadOnlyList<ArticlePaperMiner>> FetchAllPapersAsync(
        IEnumerable<string> keywords,
        int startYear,
        int endYear,
        int securityLimit,
        CancellationToken cancellationToken = default)
    {
        var semanticPapers = new List<ArticlePaperMiner>();
        var openAlexPapers = new List<ArticlePaperMiner>();

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



        var combinedPapers = CombineResults(semanticPapers, openAlexPapers);

        Console.WriteLine("All results have been saved.");

        return combinedPapers.Take(securityLimit).ToList();
    }
}
