namespace Breezy.Benchmarks.Domain.Models.Constraints;

[Table("tag")]
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Post> Posts { get; set; } = new();
}