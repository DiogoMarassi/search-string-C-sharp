using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs;
using MyApp.Services.String;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClusteringController : ControllerBase
{
    /// <summary>
    /// Executa clusterização K-Means em uma lista de pontos 2D.
    /// </summary>
    [HttpPost("kmeans")]
    public IActionResult RunKMeans([FromBody] KMeansRequest request)
    {
        if (request.Points == null || request.Points.Count == 0)
            return BadRequest("A lista de pontos não pode estar vazia.");
        if (request.K <= 0)
            return BadRequest("K deve ser maior que zero.");

        var result = Clustering.Cluster(request.Points, request.K, request.MaxIterations);
        return Ok(result); // Usa o mesmo modelo como resposta
    }
}
