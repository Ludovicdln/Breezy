using Breezy.Tests.Models.Enums;

namespace Breezy.Tests.Models;

//[Table("compagnon")]
public class Compagnon
{
    public int Id { get; set; }
    public string Pseudo { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    
    public override bool Equals(object? obj)
    {
        var compagnonCompare = obj as Compagnon;

        if (compagnonCompare == null) return false;

        return Id == compagnonCompare.Id && string.Equals(Pseudo, compagnonCompare.Pseudo)
                                         && Age == compagnonCompare.Age
                                         && Gender == compagnonCompare.Gender;
    }
}