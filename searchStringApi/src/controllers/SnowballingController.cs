using Microsoft.AspNetCore.Mvc;
using MyApp.Services;

[ApiController]
[Route("api/[controller]")]
public class SnowballingController : ControllerBase
{
    private readonly SnowballingService _snowballingService;

    public SnowballingController(SnowballingService snowballingService)
    {
        _snowballingService = snowballingService;
    }

    /// Retorna as citações de um artigo a partir do DOI
    [HttpGet("citations")]
    public async Task<ActionResult<List<SnowballingService.ArticleInfo>>> GetCitations(
        [FromQuery] string doi)
    {
        if (string.IsNullOrWhiteSpace(doi))
            return BadRequest("DOI é obrigatório.");

        var results = await _snowballingService.GetCitationsByDoiAsync(doi);
        return Ok(results);
    }

    /// Retorna as referências de um artigo a partir do DOI
    [HttpGet("references")]
    public async Task<ActionResult<List<SnowballingService.ArticleInfo>>> GetReferences(
        [FromQuery] string doi)
    {
        if (string.IsNullOrWhiteSpace(doi))
            return BadRequest("DOI é obrigatório.");

        var results = await _snowballingService.GetReferencesByDoiAsync(doi);
        return Ok(results);
    }

    /// Executa o snowballing (referências + citaçõGenerateSnowballingAsynces) a partir de uma lista de artigos iniciais
    [HttpPost("snowballing")]
    public async Task<ActionResult<List<SnowballingService.ArticleInfo>>> GenerateSnowballing(
        [FromBody] List<string> DOIs)
    {
        if (DOIs == null || DOIs.Count == 0)
            return BadRequest("Lista de DOI's não pode ser vazia.");

        var results = await _snowballingService.GenerateSnowballingAsync(DOIs);
        return Ok(results);
    }
}
