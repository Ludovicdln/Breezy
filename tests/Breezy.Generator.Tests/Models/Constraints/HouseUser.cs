﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Breezy.Generator.Tests.Models.Constraints;

[Table("house_users")]
public class HouseUser
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Tag { get; set; }
    public int Members { get; set; }
}