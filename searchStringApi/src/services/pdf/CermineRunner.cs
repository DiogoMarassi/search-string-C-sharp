using System.Diagnostics;
namespace MyApp.Services.pdf;

/// Responsável por executar o JAR do CERMINE e processar PDFs.
public class CermineRunner
{
    private readonly string _cermineVersion;

    public CermineRunner(string cermineVersion = "1.13")
    {
        _cermineVersion = cermineVersion;
    }

    /// Executa o CERMINE apontando para uma pasta de PDFs.s
    public bool Run(string inputPath)
    {
        Console.WriteLine("Starting PDF processing with CERMINE...");

        string jarPath = $"src/external/cermine-impl-{_cermineVersion}-jar-with-dependencies.jar";
        string arguments = $"-cp \"{jarPath}\" pl.edu.icm.cermine.ContentExtractor -path \"{inputPath}\"";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "java",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                Console.WriteLine("Falha ao iniciar o processo Java.");
                return false;
            }

            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("CERMINE output:");
                Console.WriteLine(output);
                return true;
            }
            else
            {
                Console.WriteLine("Erro ao executar o CERMINE:");
                Console.WriteLine(errors);
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao iniciar o processo: {ex.Message}");
            return false;
        }
    }
}
