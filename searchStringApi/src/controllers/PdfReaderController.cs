using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs;
using MyApp.Services;
using MyApp.Models;

namespace MyApp.Controllers;
[ApiController]
[Route("api/[controller]")]
public class PdfReaderController : ControllerBase
{
    private readonly PdfReaderService _pdfReader;

    public PdfReaderController(PdfReaderService pdfReader)
    {
        _pdfReader = pdfReader ?? throw new ArgumentNullException(nameof(pdfReader));
    }

    /// Processa todos os PDFs da pasta indicada e retorna dados extraídos.
    [HttpPost("read-pdf")]
    [ProducesResponseType(typeof(List<ArticleData>), StatusCodes.Status200OK)]
    public ActionResult<List<ArticleData>> Process([FromBody] ProcessFolderRequest request)
    {
        var result = _pdfReader.ExtractData(request.FolderPath);
        return Ok(result);
    }
}
