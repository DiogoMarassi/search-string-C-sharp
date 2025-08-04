using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs;
using MyApp.Services.String;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClusteringController : ControllerBase
{
    private readonly Clustering _clusteringService;
    private readonly Embedding _embeddingService;
    // A API key poderia vir de appsettings.json ou variável de ambiente
    public ClusteringController()
    {
        _embeddingService = new Embedding("nk-S_CvPLdfWOzJyL0HFgXFj1dDEBK0gxft-Fw1HsAoKKU");

        _clusteringService = new Clustering(_embeddingService);
    }


    /// Executa clusterização K-Means em uma lista de pontos 2D.
    [HttpPost("kmeans")]
    public IActionResult RunKMeans([FromBody] KMeansRequest request)
    {
        if (request.Points == null || request.Points.Count == 0)
            return BadRequest("A lista de pontos não pode estar vazia.");
        if (request.K <= 0)
            return BadRequest("K deve ser maior que zero.");

        var result = _clusteringService.Cluster(request.Points, request.K, request.MaxIterations);
        return Ok(result); // Usa o mesmo modelo como resposta
    }
}
