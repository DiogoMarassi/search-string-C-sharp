// /DTOs/StringGeneratorRequest.cs
namespace MyApp.DTOs;

/// <summary>
/// Request para processar PDFs e gerar clusters.
/// </summary>
public class StringGeneratorRequest
{
    /// <summary>Caminho da pasta com PDFs.</summary>
    public string FolderPath { get; set; } = "C:\\Users\\DiogoMarassi\\Desktop\\search-string-C-sharp\\searchStringApi\\src\\external\\temp";

    /// <summary>Número de clusters (k) para o KMeans.</summary>
    public int K { get; set; } = 3;
}
