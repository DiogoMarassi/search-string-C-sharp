using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticleFilterController : ControllerBase
{
    /// Filtra artigos com acesso aberto e URL válida
    [HttpPost("filtrar-aberto-com-url")]
    [ProducesResponseType(typeof(List<ArticlePaperMiner>), StatusCodes.Status200OK)]
    public IActionResult FiltrarAbertosComUrl([FromBody] List<ArticlePaperMiner> artigos)
    {
        if (artigos == null || artigos.Count == 0)
            return BadRequest("A lista de artigos não pode estar vazia.");

        var resultado = ArticleFilterService.FiltrarAcessoAbertoComUrl(artigos);
        return Ok(resultado);
    }

    /// Ordena os artigos por métrica de relevância (decrescente)
    [HttpPost("ordenar-por-relevancia")]
    [ProducesResponseType(typeof(List<ArticlePaperMiner>), StatusCodes.Status200OK)]
    public IActionResult OrdenarPorRelevancia([FromBody] List<ArticlePaperMiner> artigos)
    {
        if (artigos == null || artigos.Count == 0)
            return BadRequest("A lista de artigos não pode estar vazia.");

        var resultado = ArticleFilterService.OrdenarPorRelevancia(artigos);
        return Ok(resultado);
    }

    /// Filtra E ordena os artigos
    [HttpPost("filtrar-e-ordenar")]
    [ProducesResponseType(typeof(List<ArticlePaperMiner>), StatusCodes.Status200OK)]
    public IActionResult FiltrarEOrdenar([FromBody] List<ArticlePaperMiner> artigos)
    {
        if (artigos == null || artigos.Count == 0)
            return BadRequest("A lista de artigos não pode estar vazia.");

        var resultado = ArticleFilterService.FiltrarEAgrupar(artigos);
        return Ok(resultado);
    }


}
