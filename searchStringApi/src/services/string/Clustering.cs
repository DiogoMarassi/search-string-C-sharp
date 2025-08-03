using MyApp.Models;

namespace MyApp.Services.String;

/// <summary>
/// Serviço estático que executa o algoritmo K-Means em 2D.
/// </summary>
public static class Clustering
{
    public static KMeansResult Cluster(List<Point> data, int k, int maxIterations = 50)
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
