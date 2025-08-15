namespace MyApp.DTOs;

/// Request para processar PDFs e gerar clusters.
public class StringGeneratorRequest
{
    /// Caminho da pasta com PDFs
    public string FolderPath { get; set; } = "C:\\Users\\DiogoMarassi\\Desktop\\search-string-C-sharp\\searchStringApi\\src\\external\\temp";

    ///Número de clusters (k) para o KMeans
    public int K { get; set; } = 3;
}
