namespace Breezy.Sample.Domain.Entities;

[Table("user")]
public class User
{
    public int Id { get; set; }
    public DateTime Birthday { get; set; }
    public bool Sex { get; set; }
    public Credential Credential { get; set; }
}