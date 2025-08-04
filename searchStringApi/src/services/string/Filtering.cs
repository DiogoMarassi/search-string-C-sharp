
namespace MyApp.Services.String;

public class Filtering
{
    private readonly TextPreprocessing _textPreprocessor;

    public Filtering(
        TextPreprocessing textPreprocessor
    )
    {
        _textPreprocessor = textPreprocessor;
    }

    public List<string> FilterNgrams(List<string> topNgrams)
    {
        // 1️. Remove n-grams com stopwords
        topNgrams = topNgrams
            .Where(ngram =>
                !ngram.Split(' ')
                    .Any(word => _textPreprocessor.IsStopWord(word)))
            .ToList();

        // 2️. Remove simples que aparecem em compostos considerando stem

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


        // 3️. Remove palavras excessivamente semelhantes usando stemming para deduplicação
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
        return topNgrams;
    }
}