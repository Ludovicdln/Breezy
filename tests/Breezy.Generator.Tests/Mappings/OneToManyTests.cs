using Breezy.Generator.Tests.Models.Constraints;
using MySql.Data.MySqlClient;

namespace Breezy.Generator.Tests.Mappings;

public sealed class OneToManyTests
{
    private MySqlConnection _mySqlConnection;

    [SetUp]
    public void Setup()
    {
        _mySqlConnection = new MySqlConnection($"server=localhost;port=3306;user id=root; password=root; database=test;");
    }
    
    [Test]
    public async Task MapOneToMany_ShouldReturnIEnumerable()
    {
        var users = await _mySqlConnection.QueryAsync<UserCopy>(
            @"SELECT * FROM test.user_copy u INNER JOIN test.car_users c ON c.user_id = u.id 
                    INNER JOIN test.house_users h ON h.user_id = u.id");
        
        Assert.Multiple(() =>
        {
            Assert.That(users.Any(), Is.True);
            Assert.That(users.All(x => x.IsValid()), Is.True);
            Assert.That(users.Any(x => x.Houses.Count > 0));
            Assert.That(users.Any(x => x.Cars.Count > 0));
        });
    }
    
    [Test]
    public async Task MapOneToMany_ShouldReturnFirstElement()
    {
        var user = await _mySqlConnection.QueryFirstOrDefaultAsync<UserCopy>(
            @"SELECT * FROM test.user_copy u INNER JOIN test.car_users c ON c.user_id = u.id 
                    INNER JOIN test.house_users h ON h.user_id = u.id AND u.is_minor = 1");
        
        Assert.Multiple(() =>
        {
            Assert.That(user, Is.Not.Null);
            Assert.That(user.IsMinor, Is.True);
            Assert.That(user.IsValid, Is.True);
        });
    }
    
    [Test]
    public async Task MapManyToMany_ShouldReturnFirstElement_WithAnonymousType()
    {
        var user = await _mySqlConnection.QueryFirstOrDefaultAsync<UserCopy>(
            @"SELECT * FROM test.user_copy u INNER JOIN test.car_users c ON c.user_id = u.id 
                    INNER JOIN test.house_users h ON h.user_id = u.id AND u.is_minor = @IsMinor", new { IsMinor = 1});
        
        Assert.Multiple(() =>
        {
            Assert.That(user, Is.Not.Null);
            Assert.That(user.IsMinor, Is.True);
            Assert.That(user.IsValid, Is.True);
        });
    }
}