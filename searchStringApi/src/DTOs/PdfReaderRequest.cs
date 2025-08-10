using System.ComponentModel;
namespace MyApp.DTOs;

public class ProcessFolderRequest
{
    /// Caminho completo da pasta onde estão os PDFs.
    [DefaultValue("C:\\Users\\DiogoMarassi\\Desktop\\search-string-C-sharp\\searchStringApi\\src\\external\\temp")]
    public string FolderPath { get; set; } = "src/external";
}
