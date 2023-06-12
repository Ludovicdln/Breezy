using Breezy.Tests.Models.Enums;
using Breezy.Tests.Models.Sample;

namespace Breezy.Tests.Models;

//[Table("user")]
public class User
{
    public int Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public DateTime Birthday { get; set; }
    public Gender Gender { get; set; }
    public bool IsMinor { get; set; }
    public House House { get; set; }
    public Compagnon Compagnon { get; set; }
    public Car Car { get; set; }

    public override bool Equals(object? obj)
    {
        var userCompare = obj as User;

        if (userCompare == null) return false;

        return
            Id == userCompare.Id && string.Equals(Firstname, userCompare.Firstname)
                                 && string.Equals(Lastname, userCompare.Lastname)
                                 && DateTime.Equals(Birthday, userCompare.Birthday)
                                 && Gender == userCompare.Gender && IsMinor == userCompare.IsMinor 
                                 && House.Equals(userCompare.House) && Compagnon.Equals(userCompare.Compagnon)
                                 && Car.Equals(userCompare.Car);
    }

    public bool IsValid()
    {
        return Id != 0 && !string.IsNullOrEmpty(Firstname) && !string.IsNullOrEmpty(Lastname) && House != null &&
               Compagnon != null && Car != null;
    }
}