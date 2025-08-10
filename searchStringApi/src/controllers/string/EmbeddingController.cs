using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs;
using MyApp.Services.String;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmbeddingController : ControllerBase
{
    private readonly Embedding _embeddingService;
    // A API key poderia vir de appsettings.json ou variável de ambiente
    public EmbeddingController(IConfiguration config)
    {
        // ajustar depois config
        _embeddingService = new Embedding("nk-S_CvPLdfWOzJyL0HFgXFj1dDEBK0gxft-Fw1HsAoKKU");
    }


    /// Gera embeddings para uma lista de textos usando o modelo Nomic.
    /// <param name="request">Lista de textos</param>
    /// <returns>Lista de embeddings (um vetor de floats por texto)</returns>
    [HttpPost("embedding")]
    [ProducesResponseType(typeof(NomicEmbeddingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateEmbeddings([FromBody] NomicEmbeddingRequest request)
    {
        Console.WriteLine(request);
        if (request.Texts == null || request.Texts.Count == 0)
            return BadRequest("A lista de textos não pode estar vazia.");
        try
        {
            Console.WriteLine("Iniciando embedding com o texto:");
            Console.WriteLine(request.Texts);
            var embeddings = await _embeddingService.GenerateAsync(request.Texts);

            var response = new NomicEmbeddingResponse
            {
                Embeddings = embeddings.Select(e => e.ToList()).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao gerar embeddings", error = ex.Message });
        }
    }
}
