using System.ComponentModel.DataAnnotations.Schema;
using Breezy.Generator.Tests.Models.Enums;

namespace Breezy.Generator.Tests.Models;

[Table("user_ref")]
public class UserReference
{
    public int Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public DateTime Birthday { get; set; }
    public Gender Gender { get; set; }
    public bool IsMinor { get; set; }
    public Position Position { get; set; }
    public House House { get; set; }
    public Compagnon Compagnon { get; set; }
    public Car Car { get; set; }
    
    public override bool Equals(object? obj)
    {
        var userCompare = obj as UserReference;

        if (userCompare == null) return false;

        return
            Id == userCompare.Id && string.Equals(Firstname, userCompare.Firstname)
                                 && string.Equals(Lastname, userCompare.Lastname)
                                 && DateTime.Equals(Birthday, userCompare.Birthday)
                                 && Gender == userCompare.Gender && IsMinor == userCompare.IsMinor 
                                 && Position == userCompare.Position
                                 && House.Equals(userCompare.House) && Compagnon.Equals(userCompare.Compagnon)
                                 && Car.Equals(userCompare.Car);
    }

    public bool IsValid()
    {
        return Id != 0 && !string.IsNullOrEmpty(Firstname) && !string.IsNullOrEmpty(Lastname) && Position != null && House != null &&
               Compagnon != null && Car != null;
    }
}

public sealed class Position
{
    public string ZipCode { get; set; }
    public string City { get; set; }
    public string Address { get; set; }
    
    public override bool Equals(object? obj)
    {
        var positionCompare = obj as Position;

        if (positionCompare == null) return false;

        return ZipCode == positionCompare.ZipCode && string.Equals(City, positionCompare.City) && string.Equals(Address, positionCompare.Address);
    }
}

