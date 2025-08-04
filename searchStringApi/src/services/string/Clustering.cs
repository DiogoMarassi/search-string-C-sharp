using MyApp.Models;

namespace MyApp.Services.String;

/// Serviço estático que executa o algoritmo K-Means em 2D.
public class Clustering
{
    private readonly Embedding _embeddingService;

    public Clustering(
        Embedding embeddingService
    )
    {
        _embeddingService = embeddingService;
    }

    public KMeansResult Cluster(List<Point> data, int k, int maxIterations = 50)
    {
        var rand = new Random();
        var centroids = new List<Point>();
        var labels = new List<int>(new int[data.Count]);

        // Inicializa centróides aleatórios
        for (int i = 0; i < k; i++)
            centroids.Add(data[rand.Next(data.Count)]);

        // Iterações do K-Means
        for (int iter = 0; iter < maxIterations; iter++)
        {
            // Atribui cada ponto ao centróide mais próximo
            for (int i = 0; i < data.Count; i++)
                labels[i] = ClosestCentroid(data[i], centroids);

            // Atualiza centróides como a média dos pontos do cluster
            for (int j = 0; j < k; j++)
            {
                var clusterPoints = data.Where((_, idx) => labels[idx] == j).ToList();
                if (clusterPoints.Count > 0)
                    centroids[j] = Mean(clusterPoints);
            }
        }

        return new KMeansResult { Centroids = centroids, Labels = labels };
    }

    public async Task<List<object>> ClusterInners(KMeansResult externalClusters, List<string> topNgrams, int innerClustersQntd = 2)
    {
        var resultado = externalClusters.Labels
            .Select((label, index) => new { label, term = topNgrams[index] })
            .GroupBy(x => x.label)
            .Select(g => new
            {
                ClusterId = g.Key,
                Terms = g.Select(x => x.term).ToList()
            })
            .ToList();

        // Para cada cluster externo, clusterizar internamente
        var resultadoComInternos = new List<object>();

        foreach (var cluster in resultado)
        {
            // Embeddings para os termos deste cluster
            var newEmbeddings = await _embeddingService.GenerateAsync(cluster.Terms);
            var newPoints = newEmbeddings.Select(e => new Point { Vector = e }).ToList();

            // Nova clusterização interna
            var innerClusters = Cluster(newPoints, innerClustersQntd);

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

    private static int ClosestCentroid(Point point, List<Point> centroids)
    {
        double minDistance = double.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < centroids.Count; i++)
        {
            double distance = EuclideanDistance(point, centroids[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    private static Point Mean(List<Point> points)
    {
        int dims = points[0].Vector.Length;
        var centroid = new float[dims];

        foreach (var p in points)
            for (int i = 0; i < dims; i++)
                centroid[i] += p.Vector[i];

        for (int i = 0; i < dims; i++)
            centroid[i] /= points.Count;

        return new Point { Vector = centroid };
    }

    private static double EuclideanDistance(Point a, Point b)
    {
        double sum = 0;
        for (int i = 0; i < a.Vector.Length; i++)
        {
            double diff = a.Vector[i] - b.Vector[i];
            sum += diff * diff;
        }
        return Math.Sqrt(sum);
    }
}
