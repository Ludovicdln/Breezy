using System.Data;
using System.Text;
using BenchmarkDotNet.Attributes;
using Breezy.Benchmarks.Domain.Models;
using Breezy.Benchmarks.Domain.Models.Constraints;
using Dapper;
using DbConnectionExtensions;
using MySql.Data.MySqlClient;

namespace Breezy.Benchmarks;

[MemoryDiagnoser]
public class Benchmarks
{
    private MySqlConnection _mySqlConnection;
    
    [GlobalSetup]
    public void GlobalSetup()
    {
        _mySqlConnection = new MySqlConnection($"server=localhost;port=3306;user id=root; password=root; database=test;");
    }

    [Benchmark(Description = "Select And Map 10 000 simple rows ~ Breezy")]
    public async Task QueryAsyncSimpleRows_WithBreezy()
    {
        var houses = await _mySqlConnection.QueryAsync<House>("SELECT * FROM house h WHERE h.id = @Id", new {Id = 1});
    }
    
    [Benchmark(Description = "Select And Map 10 000 simple rows ~ Dapper")]
    public async Task QueryAsyncSimpleRows_WithDapper()
    {
        var houses = await _mySqlConnection.QueryAsync<House>("SELECT * FROM house", commandType:CommandType.Text);
    }
    
    [Benchmark(Description = "Select And Map 10 000 reference rows ~ Breezy")]
    public async Task QueryAsyncReferenceRows_WithBreezy()
    {
        var userReferences = await _mySqlConnection.QueryAsync<UserReference>(@"SELECT * FROM test.user_ref u 
                        INNER JOIN test.house h ON u.house_id = h.id 
                        INNER JOIN test.compagnon cp ON u.compagnon_id = cp.id
                        INNER JOIN test.car c ON u.car_id = c.id");
    }
    
    [Benchmark(Description = "Select And Map 10 000 reference rows ~ Dapper => Refenence No SET")]
    public async Task QueryAsyncReferenceRows_WithDapper()
    {
        var userReferences = await _mySqlConnection.QueryAsync<UserReference, House, Compagnon, Car, UserReference>(@"SELECT * FROM test.user_ref u 
                        INNER JOIN test.house h ON u.house_id = h.id 
                        INNER JOIN test.compagnon cp ON u.compagnon_id = cp.id
                        INNER JOIN test.car c ON u.car_id = c.id", (user, house, compagnon, car) =>
        {
            user.House = house;
            user.Compagnon = compagnon;
            user.Car = car;
            return user;
        });
    }
    
    [Benchmark(Description = "Select And Map 10 000 one to one rows ~ Breezy")]
    public async Task QueryAsyncOneToOneRows_WithBreezy()
    {
        var users = await _mySqlConnection.QueryAsync<User>(
            @"SELECT * FROM test.user u 
                        INNER JOIN test.house h ON u.house_id = h.id 
                        INNER JOIN test.compagnon cp ON u.compagnon_id = cp.id
                        INNER JOIN test.car c ON u.car_id = c.id");
    }
    
    [Benchmark(Description = "Select And Map 10 000 one to one rows ~ Dapper")]
    public async Task QueryAsyncOneToOneRows_WithDapper()
    {
        var users = await _mySqlConnection.QueryAsync<User, House, Compagnon, Car, User>(@"SELECT * FROM test.user u 
                        INNER JOIN test.house h ON u.house_id = h.id 
                        INNER JOIN test.compagnon cp ON u.compagnon_id = cp.id
                        INNER JOIN test.car c ON u.car_id = c.id", (user, house, compagnon, car) =>
        {
            user.House = house;
            user.Compagnon = compagnon;
            user.Car = car;
            return user;
        });
    }
    
    [Benchmark(Description = "Select And Map 10 000 one to many rows ~ Breezy")]
    public async Task QueryAsyncOneToManyRows_WithBreezy()
    {
        var users = await _mySqlConnection.QueryAsync<UserCopy>(
            @"SELECT * FROM test.user_copy u INNER JOIN test.car_users c ON c.user_id = u.id 
                    INNER JOIN test.house_users h ON h.user_id = u.id");
    }
    
    [Benchmark(Description = "Select And Map 10 000 one to many rows ~ Dapper")]
    public async Task QueryAsyncOneToManyRows_WithDapper()
    {
        var houses = new Dictionary<int, HouseUser>();
        var cars = new Dictionary<int, CarUser>();
        
        var users = await _mySqlConnection.QueryAsync<UserCopy, CarUser, HouseUser, UserCopy>(
            @"SELECT * FROM test.user_copy u INNER JOIN test.car_users c ON c.user_id = u.id 
                    INNER JOIN test.house_users h ON h.user_id = u.id", (user, car, house) =>
        {
            HouseUser houseEntry;
            CarUser carEntry;

            if (!houses.TryGetValue(house.Id, out houseEntry))
            {
                houseEntry = house;
                houses.Add(house.Id, houseEntry);
            }

            user.Houses.Add(houseEntry);
            
            if (!cars.TryGetValue(car.Id, out carEntry))
            {
                carEntry = car;
                cars.Add(car.Id, carEntry);
            }

            user.Houses.Add(houseEntry);
            user.Cars.Add(carEntry);
            
            return user;
        });
    }
    
    [Benchmark(Description = "Select And Map 10 000 many to many rows ~ Breezy")]
    public async Task QueryAsyncManyToManyRows_WithBreezy()
    {
        var posts = await _mySqlConnection.QueryAsync<Post>(
            @"SELECT * FROM test.post p INNER JOIN posts_tags pt ON p.id = pt.post_id INNER JOIN tag t ON t.id = pt.tag_id");
    }
    
    [Benchmark(Description = "Select And Map 10 000 many to many rows ~ Dapper")]
    public async Task QueryAsyncManyToManyRows_WithDapper()
    {
        var tags = new Dictionary<int, Tag>();
        
        var posts = await _mySqlConnection.QueryAsync<Post, Tag, Post>(
            @"SELECT * FROM test.post p INNER JOIN posts_tags pt ON p.id = pt.post_id INNER JOIN tag t ON t.id = pt.tag_id",
            (post, tag) =>
            {
                Tag tagEntry;

                if (!tags.TryGetValue(tag.Id, out tagEntry))
                {
                    tagEntry = tag;
                    tags.Add(tag.Id, tagEntry);
                }
                
                tag.Posts.Add(post);
                post.Tags.Add(tagEntry);
                return post;
            });
    }
    
}