#nullable enable
using Microsoft.AspNetCore.Mvc;
using MyApp.Services.Search;
using MyApp.DTOs;

namespace YourApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OpenAlexController : ControllerBase
{
    private readonly OpenAlex _service;

    public OpenAlexController(OpenAlex service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpPost("search")]
    [ProducesResponseType(typeof(IReadOnlyList<ArticlePaperMiner>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ArticlePaperMiner>>> Search(
        [FromBody] ClusteredSearchRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (request!.StartYear > request.EndYear)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Intervalo de anos inválido",
                Detail = $"StartYear ({request.StartYear}) não pode ser maior que EndYear ({request.EndYear})."
            });
        }

        if (request.Clusters is null || request.Clusters.Count == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Clusters ausentes",
                Detail = "Informe ao menos um cluster de termos."
            });
        }

        var result = await _service.FetchOpenAlexAsync(
            request.Clusters!,
            request.StartYear,
            request.EndYear,
            request.SecurityLimit,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping() => Ok(new { status = "ok" });
}
