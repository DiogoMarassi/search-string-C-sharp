namespace MyApp.Services.String;

/// <summary>
/// Gera n-grams e calcula top termos com pseudo TF-IDF para um documento.
/// </summary>
public class NGrams
{
    /// <summary>
    /// Retorna os top N n-grams de um documento, ponderando frequência (TF) e "raridade" local.
    /// </summary>
    public List<string> GetTopNgrams(List<string> tokens, int topN)
    {
        // 1. Gera bigrams e trigrams
        var ngrams = GenerateNgrams(tokens);

        if (ngrams.Count == 0)
            return new List<string>();

        // 2. Conta frequências (TF)
        var tf = ngrams
            .GroupBy(n => n)
            .ToDictionary(g => g.Key, g => g.Count());

        int totalNgrams = ngrams.Count;

        // 3. Calcula "IDF local" penalizando termos muito comuns
        // Aqui, como temos 1 documento, simulamos IDF = log(total / freq)
        var tfidf = tf.ToDictionary(
            kv => kv.Key,
            kv => kv.Value * Math.Log((double)totalNgrams / kv.Value + 1.0)
        );

        // 4. Ordena por TF-IDF decrescente e retorna os topN termos
        return tfidf
            .OrderByDescending(kv => kv.Value)
            .Take(topN)
            .Select(kv => kv.Key)
            .ToList();
    }

    /// <summary>
    /// Gera bigrams e trigrams a partir da lista de tokens.
    /// </summary>
    private List<string> GenerateNgrams(List<string> tokens)
    {
        var ngrams = new List<string>();

        // Palavras isoladas
        ngrams.AddRange(tokens);

        // Bigrams
        for (int i = 0; i < tokens.Count - 1; i++)
            ngrams.Add($"{tokens[i]} {tokens[i + 1]}");

        // Trigrams
        for (int i = 0; i < tokens.Count - 2; i++)
            ngrams.Add($"{tokens[i]} {tokens[i + 1]} {tokens[i + 2]}");

        return ngrams;
    }
}
