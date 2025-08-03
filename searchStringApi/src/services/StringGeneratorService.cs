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

    public StringGeneratorService(
        PdfReaderService pdfReader,
        TextPreprocessing textPreprocessor,
        NGrams ngramService,
        Embedding embeddingService
    )
    {
        _pdfReader = pdfReader;
        _textPreprocessor = textPreprocessor;
        _ngramService = ngramService;
        _embeddingService = embeddingService;
    }

    // Processa a pasta de PDFs e retorna clusters de termos usando embeddings do Nomic.
    public async Task<List<object>> ProcessPdfFolderAsync(string folderPath, int k = 3)
    {
        var articles = _pdfReader.ExtractData(folderPath);
        var article = articles.First();
        var tokens = _textPreprocessor.Preprocess(article.Title, article.AbstractText, article.keywords);
        var topNgrams = _ngramService.GetTopNgrams(tokens, 40);

        // 1️⃣ Remove n-grams com stopwords
        topNgrams = topNgrams
            .Where(ngram =>
                !ngram.Split(' ')
                    .Any(word => _textPreprocessor.IsStopWord(word)))
            .ToList();

        // 2️⃣ Remove simples que aparecem em compostos considerando stem

        var stemmer = new Porter2Stemmer.EnglishPorter2Stemmer();
        
        var composedNgrams = topNgrams.Where(t => t.Contains(' ')).ToList();
        var simpleNgrams = topNgrams.Where(t => !t.Contains(' ')).ToList();

        var wordsToRemove = new HashSet<string>();

        foreach (var simple in simpleNgrams)
        {
            var simpleStem = stemmer.Stem(simple).Value;

            foreach (var composed in composedNgrams)
            {
                // Gera os stems do composto
                var composedStems = composed
                    .Split(' ')
                    .Select(word => stemmer.Stem(word).Value)
                    .ToList();

                // Se qualquer palavra do composto tem o mesmo stem do simples → remover
                if (composedStems.Any(stem => stem.Equals(simpleStem, StringComparison.OrdinalIgnoreCase)))
                {
                    wordsToRemove.Add(simple);
                    break;
                }
            }
        }

        // Remove palavras simples que são absorvidas por compostos semanticamente
        topNgrams = topNgrams
            .Where(t => !wordsToRemove.Contains(t))
            .ToList();


        // 3️⃣ Remove palavras excessivamente semelhantes usando stemming para deduplicação
        var seenRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var filteredNgrams = new List<string>();

        foreach (var ngram in topNgrams)
        {
            // Gera uma chave de stem para cada n-gram
            var stemKey = string.Join(" ",
                ngram.Split(' ')
                    .Select(word => stemmer.Stem(word).Value)
            );

            // Só adiciona se ainda não vimos esse stem
            if (!seenRoots.Contains(stemKey))
            {
                filteredNgrams.Add(ngram);   // Mantém original
                seenRoots.Add(stemKey);      // Marca pelo stem
            }
        }

        // Atualiza lista final
        topNgrams = filteredNgrams.Take(10).ToList();

        var embeddings = await _embeddingService.GenerateAsync(topNgrams);
        var points = embeddings.Select(e => new Point { Vector = e }).ToList();

        var clusters = Clustering.Cluster(points, k);

        // Clusters externos (primeira clusterização)
        var resultado = clusters.Labels
            .Select((label, index) => new { label, term = topNgrams[index] })
            .GroupBy(x => x.label)
            .Select(g => new
            {
                ClusterId = g.Key,
                Terms = g.Select(x => x.term).ToList()
            })
            .ToList();

        // 🔹 Para cada cluster externo, clusterizar internamente
        var resultadoComInternos = new List<object>();

        foreach (var cluster in resultado)
        {
            // Embeddings para os termos deste cluster
            var newEmbeddings = await _embeddingService.GenerateAsync(cluster.Terms);
            var newPoints = newEmbeddings.Select(e => new Point { Vector = e }).ToList();

            // Nova clusterização interna
            var innerClusters = Clustering.Cluster(newPoints, 2);

            // Mapeia termos para clusters internos
            var innerResultado = innerClusters.Labels
                .Select((innerLabel, index) => new { innerLabel, term = cluster.Terms[index] })
                .GroupBy(x => x.innerLabel)
                .Select(g => g.Select(x => x.term).ToList())
                .ToList();

            // Cria objeto final simples
            resultadoComInternos.Add(new
            {
                ClusterId = cluster.ClusterId,
                Terms = cluster.Terms,
                InnerClusters = innerResultado
            });
        }

        return resultadoComInternos;
    }


}
