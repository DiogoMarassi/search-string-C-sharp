using MyApp.Models;
using MyApp.Services.String;

namespace MyApp.Services;

/// Orquestra toda a pipeline: PDF → Texto → N-Grams → Embeddings → Clusters.
public class StringGeneratorService
{
    private readonly PdfReaderService _pdfReader;
    private readonly TextPreprocessing _textPreprocessor;
    private readonly NGrams _ngramService;
    private readonly Embedding _embeddingService;
    private readonly Filtering _filterNgrams;
    private readonly Clustering _clustering;
    public StringGeneratorService(
        PdfReaderService pdfReader,
        TextPreprocessing textPreprocessor,
        NGrams ngramService,
        Embedding embeddingService,
        Filtering filterNgrams,
        Clustering clustering
    )
    {
        _pdfReader = pdfReader;
        _textPreprocessor = textPreprocessor;
        _ngramService = ngramService;
        _embeddingService = embeddingService;
        _filterNgrams = filterNgrams;
        _clustering = clustering;
    }

    // Processa a pasta de PDFs e retorna clusters de termos usando embeddings do Nomic.
    public async Task<List<object>> ProcessPdfFolderAsync(string folderPath, int k = 3)
    {
        var articles = _pdfReader.ExtractData(folderPath);
        var article = articles.First();
        var tokens = _textPreprocessor.Preprocess(article.Title, article.AbstractText, article.keywords);
        var topNgrams = _filterNgrams.FilterNgrams(_ngramService.GetTopNgrams(tokens, 40));

        var embeddings = await _embeddingService.GenerateAsync(topNgrams);
        var points = embeddings.Select(e => new Point { Vector = e }).ToList();

        // Clusters externos (primeira clusterização)
        var clusters = _clustering.Cluster(points, k);

        var result = await _clustering.ClusterInners(clusters, topNgrams);

        return result;
    }
}
