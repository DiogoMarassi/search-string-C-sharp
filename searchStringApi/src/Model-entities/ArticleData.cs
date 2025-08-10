namespace MyApp.Models;

/// <summary>
/// Representa os dados extraídos de um artigo processado pelo CERMINE.
/// É imutável e serializável para JSON automaticamente.
/// </summary>
public record ArticleData(string FileName, string Title, string AbstractText, string keywords);
