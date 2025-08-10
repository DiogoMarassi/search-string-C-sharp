#nullable enable
using Microsoft.AspNetCore.Mvc;
using MyApp.Services.Search;

[ApiController]
[Route("api/[controller]")]
public sealed class SemanticController : ControllerBase
{
    private readonly Semantic _service;

    public SemanticController(Semantic service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        // nameof retorna o nome do identificador da variável, útil para validação de nullability
    }


    [HttpPost("semantic-search")]
    [ProducesResponseType(typeof(IReadOnlyList<PaperMinerArticle>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PaperMinerArticle>>> Search(
        [FromBody] ClusteredSearchRequest request,
        CancellationToken cancellationToken)
    {
        // validações objetivas
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (request!.StartYear > request.EndYear)
            return BadRequest(new ProblemDetails
            {
                Title = "Intervalo de anos inválido",
                Detail = $"StartYear ({request.StartYear}) não pode ser maior que EndYear ({request.EndYear})."
            });

        if (request.Clusters is null || request.Clusters.Count == 0)
            return BadRequest(new ProblemDetails
            {
                Title = "Clusters ausentes",
                Detail = "Informe ao menos um cluster de termos."
            });

        var result = await _service.FetchSemanticAsync(
            request.Clusters!,
            request.StartYear,
            request.EndYear,
            request.SecurityLimit,
            cancellationToken);

        return Ok(result);
    }

    // Healthcheck simples
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping() => Ok(new { status = "ok" });
}
