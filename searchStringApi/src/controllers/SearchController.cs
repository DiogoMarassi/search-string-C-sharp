using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs;

using MyApp.Services;


[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly SearchService _searchService;

    public SearchController(SearchService searchService)
    {
        _searchService = searchService;
    }

    /// Processa todos os PDFs da pasta indicada e retorna dados extraídos.
    [HttpPost("search")]
    [ProducesResponseType(typeof(Task<List<JsonElement>>), StatusCodes.Status200OK)]
    public async Task<List<JsonElement>> Process()
    {
        var result = await _searchService.Search("\"machine learning\"", 2022, 2024, "JournalArticle,Review");
        return result;
    }
}
