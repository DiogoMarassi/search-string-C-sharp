using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs;
using MyApp.Services;
using MyApp.Models;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StringGeneratorController : ControllerBase
{
    private readonly StringGeneratorService _stringGeneratorService;

    public StringGeneratorController(StringGeneratorService stringGeneratorService)
    {
        _stringGeneratorService = stringGeneratorService;
    }

    /// <summary>
    /// Processa PDFs de uma pasta e retorna clusters baseados em embeddings do Nomic.
    /// </summary>
    /// <param name="request">Caminho da pasta e número de clusters desejado.</param>
    /// <returns>Clusters gerados a partir dos embeddings dos abstracts.</returns>
    [HttpPost("generatestring")]
    [ProducesResponseType(typeof(KMeansResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessPdfs([FromBody] StringGeneratorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FolderPath))
            return BadRequest("O caminho da pasta é obrigatório.");

        if (!Directory.Exists(request.FolderPath))
            return BadRequest($"A pasta '{request.FolderPath}' não existe.");

        try
        {
            var clusters = await _stringGeneratorService.ProcessPdfFolderAsync(request.FolderPath, request.K);
            return Ok(clusters);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao processar PDFs", error = ex.Message });
        }
    }
}
