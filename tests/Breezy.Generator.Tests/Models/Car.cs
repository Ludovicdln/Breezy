using System.ComponentModel.DataAnnotations.Schema;

namespace Breezy.Generator.Tests.Models;

[Table("car")]
public class Car
{
    public int Id { get; set; }
    public string Model { get; set; }
    public double Price { get; set; }
    
    public override bool Equals(object? obj)
    {
        var carCompare = obj as Car;

        if (carCompare == null) return false;

        return Id == carCompare.Id && string.Equals(Model, carCompare.Model)
                                   && string.Equals(Price.ToString(), carCompare.Price.ToString());
    }
}