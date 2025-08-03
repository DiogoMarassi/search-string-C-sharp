using System.Xml.Linq;
using MyApp.Models;

namespace MyApp.Services.pdf;

// Responsável por ler arquivos XML gerados pelo CERMINE e extrair título, resumo e keywords.
public class XmlArticleExtractor
{
    // Extrai título, resumo e keywords de um arquivo XML individual.
    public ArticleData ExtractFromXml(string pathToArticle)
    {
        try
        {
            XDocument doc = XDocument.Load(pathToArticle);

            // 🔹 Título
            var titleElement = doc.Descendants("article-title").FirstOrDefault();
            string titleText = titleElement != null
                ? titleElement.Value.Trim()
                : "Título não encontrado";

            // 🔹 Resumo
            var abstractElement = doc.Descendants("abstract").FirstOrDefault();
            string abstractText = "Resumo não encontrado";

            if (abstractElement != null)
            {
                abstractText = string.Concat(
                    abstractElement.Elements("p")
                                   .Select(p => (p.Value ?? ""))
                ).Trim();

                if (string.IsNullOrEmpty(abstractText))
                    abstractText = abstractElement.Value.Trim();
            }

            // 🔹 Keywords
            var keywords = doc.Descendants("kwd")
                              .Select(k => k.Value.Trim())
                              .Where(k => !string.IsNullOrEmpty(k))
                              .ToList();

            string keywordsText = keywords.Any() 
                ? string.Join(", ", keywords) 
                : "Keywords não encontradas";

            return new ArticleData(
                Path.GetFileName(pathToArticle),
                titleText,
                abstractText,
                keywordsText
            );
        }
        catch (Exception e)
        {
            Console.WriteLine($"Erro ao processar '{Path.GetFileName(pathToArticle)}': {e.Message}");
            return new ArticleData(
                Path.GetFileName(pathToArticle),
                "Erro ao processar título",
                "Erro ao processar resumo",
                "Erro ao processar keywords"
            );
        }
    }

    // Processa todos os XMLs em uma pasta e retorna uma lista de artigos.
    public List<ArticleData> ExtractAllFromFolder(string inputFolder)
    {
        if (!Directory.Exists(inputFolder))
        {
            Console.WriteLine($"A pasta '{inputFolder}' não existe.");
            return new List<ArticleData>();
        }

        var allData = new List<ArticleData>();

        foreach (var filePath in Directory.GetFiles(inputFolder, "*.cermxml"))
        {
            var article = ExtractFromXml(filePath);
            allData.Add(article);
        }

        return allData;
    }
}
