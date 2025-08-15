using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
namespace MyApp.DTOs;
public sealed class CombinePapersRequest
{
    [Required]
    public IEnumerable<string>? Keywords { get; init; }

    [Range(1800, 2100)]
    public int StartYear { get; init; }

    [Range(1800, 2100)]
    public int EndYear { get; init; }

    [Range(1, 10_000)]
    public int SecurityLimit { get; init; } = 500;
}

public sealed class ClusteredSearchRequest
{
    [Required] public Dictionary<string, List<string>>? Clusters { get; init; }
    [Range(1800, 2100)] public int StartYear { get; init; }
    [Range(1800, 2100)] public int EndYear { get; init; }
    [Range(1, 10_000)] public int SecurityLimit { get; init; } = 500;
}