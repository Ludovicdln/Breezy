namespace Breezy.Tests.Models.Constraints;

//[Table("post")]
//[SplitOn(3, 4)]
public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public List<Tag> Tags { get; set; } = new();
    
    public bool IsValid()
    {
        return Id != 0 && !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Title) && Tags != null;
    }
}