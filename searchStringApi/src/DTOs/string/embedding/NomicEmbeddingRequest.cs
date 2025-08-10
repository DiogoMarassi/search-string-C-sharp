namespace MyApp.DTOs;
public class NomicEmbeddingRequest
{
    public List<string> Texts { get; set; } = new();
    public string Task_Type { get; set; } = "search_document";
    public int Max_Tokens_Per_Text { get; set; } = 8192;
    public int Dimensionality { get; set; } = 768;
}


