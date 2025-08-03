namespace MyApp.DTOs;
public class NomicEmbeddingResponse
{
    public List<List<float>> Embeddings { get; set; } = new();
}