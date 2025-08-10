using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs;
using MyApp.Services.String;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClusteringController : ControllerBase
{
<<<<<<< HEAD:searchStringApi/src/controllers/string/ClusteringController.cs
=======
    private readonly Clustering _clusteringService;
    private readonly Embedding _embeddingService;
    // A API key poderia vir de appsettings.json ou variável de ambiente
    public ClusteringController()
    {
        _embeddingService = new Embedding("nk-S_CvPLdfWOzJyL0HFgXFj1dDEBK0gxft-Fw1HsAoKKU");

        _clusteringService = new Clustering(_embeddingService);
    }


>>>>>>> 2a85aa4cdea0e2a43f8ca3c98ab6139f9b5b7abc:searchStringApi/src/controllers/ClusteringController.cs
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
