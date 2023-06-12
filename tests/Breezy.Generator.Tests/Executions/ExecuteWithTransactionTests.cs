using System.Text;
using Breezy.Generator.Tests.Models;
using Breezy.Generator.Tests.Models.Enums;
using Bogus;
using DbConnectionExtensions;
using MySql.Data.MySqlClient;

namespace Breezy.Generator.Tests.Executions;

public sealed class ExecuteWithTransactionTests
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
        var userReferences = new Faker<UserReference>()
            .RuleFor(x => x.Firstname, f => f.Person.FirstName)
            .RuleFor(x => x.Lastname, f => f.Person.LastName)
            .RuleFor(x => x.Gender, f => f.PickRandom<Gender>())
            .RuleFor(x => x.Birthday, f => f.Person.DateOfBirth)
            .RuleFor(x => x.IsMinor, f => (DateTime.Now - f.Person.DateOfBirth).TotalDays >= 6570)
            .RuleFor(x => x.Position, f => new Position() { Address = f.Address.FullAddress(), City = f.Address.City(), ZipCode = f.Address.ZipCode()})
            .Generate(10000);
        
        var houses = (await _mySqlConnection.QueryAsync<House>("SELECT * FROM house")).ToList();

        var cars = (await _mySqlConnection.QueryAsync<Car>("SELECT * FROM car")).ToList();

        var compagnons = (await _mySqlConnection.QueryAsync<Compagnon>("SELECT * FROM compagnon")).ToList();
        
        var userValues = new StringBuilder();
        
        var random = new Random();
        
        foreach (var userReference in userReferences)
        {
            var houseRdn = houses[random.Next(0, houses.Count - 1)];
            var carRdn = cars[random.Next(0, cars.Count - 1)];
            var compagnonRdn = compagnons[random.Next(0, compagnons.Count - 1)];
            
            userReference.House = houseRdn;
            userReference.Car = carRdn;
            userReference.Compagnon = compagnonRdn;
            
            userValues.Append($"('{userReference.Firstname.Replace("'", "\\'")}', '{userReference.Lastname.Replace("'", "\\'")}', '{userReference.Birthday.ToString("yyyy-MM-dd")}', '{(int)userReference.Gender}', '{Convert.ToByte(userReference.IsMinor)}', '{userReference.Position.ZipCode.Replace("'", "\\'")}', '{userReference.Position.City.Replace("'", "\\'")}', '{userReference.Position.Address.Replace("'", "\\'")}', '{userReference.House.Id}', '{userReference.Compagnon.Id}', '{userReference.Car.Id}'),");

        }
        
        userValues.Remove(userValues.Length - 1, 1);

        var userInsertSql = $"INSERT INTO user_ref (firstname, lastname, birthday, gender, is_minor, zip_code, city, address, house_id, compagnon_id, car_id) VALUES {userValues}; SELECT LAST_INSERT_ID();";

        await _mySqlConnection.OpenAsync();
        var dbTransaction = await _mySqlConnection.BeginTransactionAsync();

        var results = await _mySqlConnection.ExecuteAsync(new []{userInsertSql}, dbTransaction);

        var lastUserId = results[0];

        var startUserId = (lastUserId - userReferences.Count) + 1;
        
        foreach (var user in userReferences)
        {
            user.Id = startUserId;
            startUserId++;
        }

        Assert.Multiple(() =>
        {
            Assert.That(lastUserId, Is.Not.EqualTo(-1));

            Assert.That(userReferences.All(x => x.Id != 0), Is.True);
        });
    }
}