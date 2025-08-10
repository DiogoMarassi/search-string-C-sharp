using MyApp.Models;

namespace MyApp.DTOs;

/// <summary>
/// Requisição para clusterização K-Means.
/// </summary>
public class KMeansRequest
{
    /// <summary>Lista de pontos para clusterizar.</summary>
    public List<Point> Points { get; set; } = new();

    /// <summary>Número de clusters (K).</summary>
    public int K { get; set; }

    /// <summary>Máximo de iterações (default = 100).</summary>
    public int MaxIterations { get; set; } = 100;
}
