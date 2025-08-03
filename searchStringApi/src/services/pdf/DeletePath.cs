using System;
using System.IO;

namespace MyApp.Utilities
{
    /// Fornece utilitário para deletar todos os arquivos de uma pasta.
    public static class DeletePath
    {
        public static bool DeleteAllFiles(string folderPath, bool deleteSubfolders = true)
        {
            try
            {
                // Verifica se a pasta existe
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine($"A pasta '{folderPath}' não existe.");
                    return false;
                }

                // Deleta arquivos na pasta principal
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    try
                    {
                        File.Delete(file);
                        Console.WriteLine($"Arquivo deletado: {Path.GetFileName(file)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao deletar '{file}': {ex.Message}");
                    }
                }

                // Se solicitado, deleta subpastas e seus conteúdos
                if (deleteSubfolders)
                {
                    foreach (var dir in Directory.GetDirectories(folderPath))
                    {
                        try
                        {
                            Directory.Delete(dir, true); // true = deleta recursivamente
                            Console.WriteLine($"Subpasta deletada: {dir}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao deletar subpasta '{dir}': {ex.Message}");
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao processar a pasta '{folderPath}': {e.Message}");
                return false;
            }
        }
    }
}
