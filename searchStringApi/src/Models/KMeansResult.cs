namespace MyApp.Models;

/// <summary>
/// Resultado do K-Means, usado internamente e como resposta da API.
/// </summary>
public class KMeansResult
{
    public List<Point> Centroids { get; set; } = new();
    public List<int> Labels { get; set; } = new();
}
