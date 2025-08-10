#nullable enable
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using MyApp.Services;

namespace YourApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CombinePapersController : ControllerBase
{
    private readonly CombinePapersService _service;

    public CombinePapersController(CombinePapersService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }



    [HttpPost("search")]
    [ProducesResponseType(typeof(IReadOnlyList<ApiPaperMiner>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ApiPaperMiner>>> Search(
        [FromBody] CombinePapersRequest request,
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

        if (request.Keywords is null || !request.Keywords.Any())
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Keywords ausentes",
                Detail = "Informe ao menos uma keyword."
            });
        }

        var result = await _service.FetchAllPapersAsync(
            request.Keywords,
            request.StartYear,
            request.EndYear,
            request.WithPdf,
            request.WithIssn,
            request.SecurityLimit,
            request.WithPreprint,
            cancellationToken
        );

        return Ok(result);
    }

    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping() => Ok(new { status = "ok" });
}
