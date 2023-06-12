namespace Breezy.Sample.Domain.Entities;

[Table("credential")]
public class Credential
{
    public int Id { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
}