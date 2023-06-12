using Breezy.Generator.Tests.Models;
using Breezy.Generator.Tests.Models.Enums;
using Bogus.DataSets;
using MySql.Data.MySqlClient;

namespace Breezy.Generator.Tests.Mappings;

public sealed class OneToOneTests
{
    private MySqlConnection _mySqlConnection;

    [SetUp]
    public void Setup()
    {
        _mySqlConnection = new MySqlConnection($"server=localhost;port=3306;user id=root; password=root; database=test;");
    }
    
    [Test]
    public async Task MapOneToOne_ShouldReturnIEnumerable()
    {
        var users = await _mySqlConnection.QueryAsync<User>(
            @"SELECT * FROM test.user u 
                        INNER JOIN test.house h ON u.house_id = h.id 
                        INNER JOIN test.compagnon cp ON u.compagnon_id = cp.id
                        INNER JOIN test.car c ON u.car_id = c.id");
        
        var usersRef = await _mySqlConnection.QueryAsync<UserReference>(
            @"SELECT * FROM test.user_ref u 
                        INNER JOIN test.house h ON u.house_id = h.id 
                        INNER JOIN test.compagnon cp ON u.compagnon_id = cp.id
                        INNER JOIN test.car c ON u.car_id = c.id");
        
        Assert.Multiple(() =>
        {
            Assert.That(users.Any(), Is.True);
            Assert.That(users.All(x => x.IsValid()), Is.True);
            Assert.That(usersRef.Any(), Is.True);
            Assert.That(usersRef.All(x => x.IsValid()), Is.True);
        });
    }

    [Test]
    public async Task MapOneToOne_ShouldReturnFirstElement()
    {
        var user = await _mySqlConnection.QueryFirstOrDefaultAsync<User>(
            @"SELECT * FROM test.user u 
                        INNER JOIN test.house h ON u.house_id = h.id 
                        INNER JOIN test.compagnon cp ON u.compagnon_id = cp.id
                        INNER JOIN test.car c ON u.car_id = c.id WHERE u.is_minor AND u.gender");
        
        var userRef = await _mySqlConnection.QueryFirstOrDefaultAsync<UserReference>(
            @"SELECT * FROM test.user_ref u 
                        INNER JOIN test.house h ON u.house_id = h.id 
                        INNER JOIN test.compagnon cp ON u.compagnon_id = cp.id
                        INNER JOIN test.car c ON u.car_id = c.id WHERE u.is_minor AND u.gender");
        Assert.Multiple(() =>
        {
            Assert.That(user, Is.Not.Null);
            Assert.That(userRef, Is.Not.Null);
        });
        
        Assert.Multiple(() =>
        {
            Assert.That(user is { IsMinor: true }, Is.True);
            Assert.That(user is { Gender: Gender.FEMALE }, Is.True);
            Assert.That(userRef is { IsMinor: true }, Is.True);
            Assert.That(userRef is { Gender: Gender.FEMALE }, Is.True);
        });
    }
    
    [Test]
    public async Task MapOneToOne_ShouldReturnFirstElementWithAnonymousType()
    {
        var user = await _mySqlConnection.QueryFirstOrDefaultAsync<User>(
            @"SELECT * FROM test.user u 
                        INNER JOIN test.house h ON u.house_id = h.id 
                        INNER JOIN test.compagnon cp ON u.compagnon_id = cp.id
                        INNER JOIN test.car c ON u.car_id = c.id WHERE u.is_minor AND u.gender = @GenderT", new { GenderT = 1 });
        
        var userRef = await _mySqlConnection.QueryFirstOrDefaultAsync<UserReference>(
            @"SELECT * FROM test.user_ref u 
                        INNER JOIN test.house h ON u.house_id = h.id 
                        INNER JOIN test.compagnon cp ON u.compagnon_id = cp.id
                        INNER JOIN test.car c ON u.car_id = c.id WHERE u.is_minor AND u.gender = @GenderT", new {GenderT = 1});
        Assert.Multiple(() =>
        {
            Assert.That(user, Is.Not.Null);
            Assert.That(userRef, Is.Not.Null);
        });
        
        Assert.Multiple(() =>
        {
            Assert.That(user is { IsMinor: true }, Is.True);
            Assert.That(user is { Gender: Gender.FEMALE }, Is.True);
            Assert.That(userRef is { IsMinor: true }, Is.True);
            Assert.That(userRef is { Gender: Gender.FEMALE }, Is.True);
        });
    }
}