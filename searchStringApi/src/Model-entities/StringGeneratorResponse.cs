public class StringGeneratorResponse
{
    public int ClusterId { get; set; }
    public List<string> Terms { get; set; } = new();
}