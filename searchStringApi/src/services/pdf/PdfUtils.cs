using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace MyApp.Services.pdf;
public static class PdfUtils
{

    /// Reduz um PDF (ou todos os PDFs em uma pasta) para apenas as primeiras páginas.
    /// Se for pasta, processa todos os PDFs dentro dela.
    /// O(s) arquivo(s) original(is) são modificados permanentemente.

    /// <param name="path">Caminho do PDF ou da pasta com PDFs</param>
    /// <param name="pageCount">Número de páginas a manter</param>
    public static void ReduceToFirstPages(string path, int pageCount = 1)
    {
        if (File.Exists(path))
        {
            // Se o caminho é um arquivo PDF, processa só ele
            ReduceSinglePdf(path, pageCount);
        }
        else if (Directory.Exists(path))
        {
            // Se o caminho é uma pasta, processa todos os PDFs dentro dela
            var pdfFiles = Directory.GetFiles(path, "*.pdf");
            foreach (var pdf in pdfFiles)
            {
                ReduceSinglePdf(pdf, pageCount);
            }
        }
        else
        {
            Console.WriteLine($"O caminho '{path}' não existe.");
        }
    }

    // Reduz um único PDF para apenas as primeiras páginas, sobrescrevendo o original.
    private static void ReduceSinglePdf(string pdfPath, int pageCount)
    {
        try
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

            using (var inputDocument = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import))
            using (var outputDocument = new PdfDocument())
            {
                int pagesToCopy = Math.Min(pageCount, inputDocument.PageCount);

                for (int i = 0; i < pagesToCopy; i++)
                {
                    outputDocument.AddPage(inputDocument.Pages[i]);
                }

                outputDocument.Save(tempFile);
            }

            // Substitui o original pelo reduzido
            File.Copy(tempFile, pdfPath, overwrite: true);
            File.Delete(tempFile);

            Console.WriteLine($"PDF reduzido: {Path.GetFileName(pdfPath)} ({pageCount} páginas)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao reduzir PDF '{pdfPath}': {ex.Message}");
        }
    }
}
