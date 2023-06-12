﻿namespace Breezy.Benchmarks.Domain.Models.Constraints;

[Table("car_users")]
public class CarUser
{
    public int Id { get; set; }
    public string Model { get; set; }
    public double Price { get; set; }
}