using MyApp.Models;
using System.Text.Json;
using MyApp.Utilities;
using MyApp.Services.pdf;

namespace MyApp.Services;

// Serviço que orquestra a execução do CERMINE e a leitura dos XMLs.
public class PdfReaderService
{
    private readonly CermineRunner _runner;
    private readonly XmlArticleExtractor _extractor;

    public PdfReaderService(CermineRunner runner, XmlArticleExtractor extractor)
    {
        _runner = runner;
        _extractor = extractor;
    }

    // Executa o processamento completo e retorna a lista de artigos extraídos.
    public List<ArticleData> ExtractData(string path)
    {
        PdfUtils.ReduceToFirstPages(path, 3);
      
        bool success = _runner.Run(path);

        if (!success)
        {
            Console.WriteLine("Falha ao processar PDFs com CERMINE.");
            return new List<ArticleData>();
        }

        var output = _extractor.ExtractAllFromFolder(path);
        DeletePath.DeleteAllFiles(path);

        return output;
    }

    // Executa e salva o resultado em JSON formatado na mesma pasta.
    public List<ArticleData> ExtractAndSaveJson(string path)
    {
        var data = ExtractData(path);
        string jsonOutput = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(path, "resultado.json"), jsonOutput);
        Console.WriteLine("Resultado salvo em resultado.json");
        return data;
    }
}
