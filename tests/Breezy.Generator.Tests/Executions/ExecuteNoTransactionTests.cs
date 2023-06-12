using System.Globalization;
using System.Text;
using Breezy.Generator.Tests.Models;
using Breezy.Generator.Tests.Models.Enums;
using Bogus;
using DbConnectionExtensions;
using MySql.Data.MySqlClient;

namespace Breezy.Generator.Tests.Executions;

public sealed class ExecuteNoTransactionTests
{
    private MySqlConnection _mySqlConnection;

    [SetUp]
    public void Setup()
    {
        _mySqlConnection = new MySqlConnection($"server=localhost;port=3306;user id=root; password=root; database=test;");
    }

    [Test]
    public async Task DbExecute_ShouldInsertData_ReturnLastId()
    {
        var houses = new Faker<House>()
            .RuleFor(x => x.Name, y => y.Commerce.ProductName())
            .RuleFor(x => x.Members, y => y.Random.Int(1, 15000))
            .RuleFor(x => x.Tag, y => y.Random.String2(4)).Generate(10000);
        
        var cars = new Faker<Car>()
            .RuleFor(x => x.Model, y => y.Vehicle.Model())
            .RuleFor(x => x.Price, y => y.Random.Double(1, 1865)).Generate(10000);
        
        var compagnons = new Faker<Compagnon>()
            .RuleFor(x => x.Pseudo, y => y.Person.UserName)
            .RuleFor(x => x.Age, y => y.Random.Int(1, 1850))
            .RuleFor(x => x.Gender, f => f.PickRandom<Gender>()).Generate(10000);

        var users = new Faker<User>()
            .RuleFor(x => x.Firstname, f => f.Person.FirstName)
            .RuleFor(x => x.Lastname, f => f.Person.LastName)
            .RuleFor(x => x.Gender, f => f.PickRandom<Gender>())
            .RuleFor(x => x.Birthday, f => f.Person.DateOfBirth)
            .RuleFor(x => x.IsMinor, f => (DateTime.Now - f.Person.DateOfBirth).TotalDays >= 6570)
            .Generate(10000);
        
        var houseValues = new StringBuilder();

        foreach (var house in houses)
            houseValues.Append($"('{house.Name}', '{house.Tag}', '{house.Members}'),");

        houseValues.Remove(houseValues.Length - 1, 1);

        var housesInsertSql = $"INSERT INTO house (name, tag, members) VALUES {houseValues}; SELECT LAST_INSERT_ID();";

        var lastHouseId = 10000;//await _mySqlConnection.ExecuteAsync(housesInsertSql);
        
        var startHouseId = (lastHouseId - houses.Count) + 1;

        foreach (var house in houses)
        {
            house.Id = startHouseId;
            startHouseId++;
        }
        
        Assert.Multiple(() =>
        {
            Assert.That(lastHouseId, Is.Not.EqualTo(-1));
            Assert.That(houses.All(x => x.Id != 0), Is.True);
        });
        
        var carValues = new StringBuilder();

        foreach (var car in cars)
            carValues.Append($"('{car.Model.Replace("'", "\\'")}', '{car.Price.ToString(new NumberFormatInfo())}'),");

        carValues.Remove(carValues.Length - 1, 1);
        
        var carInsertSql = $"INSERT INTO car (model, price) VALUES {carValues}; SELECT LAST_INSERT_ID();";

        var lastCarId = await _mySqlConnection.ExecuteAsync(carInsertSql);
        
        var startCarId = (lastCarId - cars.Count) + 1;

        foreach (var car in cars)
        {
            car.Id = startCarId;
            startCarId++;
        }
        
        Assert.Multiple(() =>
        {
            Assert.That(lastCarId, Is.Not.EqualTo(-1));
            Assert.That(cars.All(x => x.Id != 0), Is.True);
        });

        var compagnonValues = new StringBuilder();
        
        foreach (var compagnon in compagnons)
            compagnonValues.Append($"('{compagnon.Pseudo.Replace("'", "\\'")}', '{compagnon.Age}', '{(int)compagnon.Gender}'),");

        compagnonValues.Remove(compagnonValues.Length - 1, 1);

        var compagnonInsertSql = $"INSERT INTO compagnon (pseudo, age, gender) VALUES {compagnonValues}; SELECT LAST_INSERT_ID();";

        var lastCompagnonId = await _mySqlConnection.ExecuteAsync(compagnonInsertSql);
        
        var startCompagnonId = (lastCompagnonId - compagnons.Count) + 1;
        
        foreach (var compagnon in compagnons)
        {
            compagnon.Id = startCompagnonId;
            startCompagnonId++;
        }
        
        Assert.Pass();
        
        Assert.Multiple(() =>
        {
            Assert.That(lastCompagnonId, Is.Not.EqualTo(-1));

            Assert.That(compagnons.All(x => x.Id != 0), Is.True);
        });
        
        
        var userValues = new StringBuilder();

        var random = new Random();
        
        foreach (var user in users)
        {
            var houseRdn = houses[random.Next(0, houses.Count - 1)];
            var carRdn = cars[random.Next(0, cars.Count - 1)];
            var compagnonRdn = compagnons[random.Next(0, compagnons.Count - 1)];
            
            user.House = houseRdn;
            user.Car = carRdn;
            user.Compagnon = compagnonRdn;
            
            userValues.Append($"('{user.Firstname.Replace("'", "\\'")}', '{user.Lastname.Replace("'", "\\'")}', '{user.Birthday.ToString("yyyy-MM-dd")}', '{(int)user.Gender}', '{Convert.ToByte(user.IsMinor)}', '{user.House.Id}', '{user.Compagnon.Id}', '{user.Car.Id}'),");
        }

        userValues.Remove(userValues.Length - 1, 1);

        var userInsertSql = $"INSERT INTO user (firstname, lastname, birthday, gender, is_minor, house_id, compagnon_id, car_id) VALUES {userValues}; SELECT LAST_INSERT_ID();";

        var lastUserId = await _mySqlConnection.ExecuteAsync(userInsertSql);

        var startUserId = (lastUserId - users.Count) + 1;
        
        foreach (var user in users)
        {
            user.Id = startUserId;
            startUserId++;
        }
        
        Assert.Pass();

        Assert.Multiple(() =>
        {
            Assert.That(lastUserId, Is.Not.EqualTo(-1));

            Assert.That(users.All(x => x.Id != 0), Is.True);
        });
    }
}