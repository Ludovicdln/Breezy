namespace Breezy.Tests.Models.Sample;

//[Table("house")]
public class House
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Tag { get; set; }
    public int Members { get; set; }
    
    public override bool Equals(object? obj)
    {
        var houseCompare = obj as House;

        if (houseCompare == null) return false;

        return
            Id == houseCompare.Id && string.Equals(Name, houseCompare.Name)
                                  && string.Equals(Tag, houseCompare.Tag)
                                  && Members == houseCompare.Members;
    }
}