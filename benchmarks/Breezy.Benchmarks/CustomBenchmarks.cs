using System.Data;
using BenchmarkDotNet.Attributes;
using Breezy.Benchmarks.Domain.Models;
using Breezy.Benchmarks.Domain.Models.Constraints;
using Breezy.Benchmarks.Extensions;
using Dapper;
using MySql.Data.MySqlClient;

namespace Breezy.Benchmarks;

[MemoryDiagnoser()]
public class CustomBenchmarks
{
    private MySqlConnection _mySqlConnection;
    private MemoryCacheableQuery<House> _memoryCacheableQuery;
    private MemoryCacheableQuery<UserCopy> _memoryCacheableQueryBis;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _mySqlConnection = new MySqlConnection($"server=localhost;port=3306;user id=root; password=root; database=test;");
        _memoryCacheableQuery = new MemoryCacheableQuery<House>();
        _memoryCacheableQueryBis = new MemoryCacheableQuery<UserCopy>();
    }

    [Benchmark]
    public async Task QueryAsyncSimpleRows_WithBreezy()
    {
        var houses = await _mySqlConnection.QueryAsync<House>("SELECT * FROM house");
    }
    
    [Benchmark]
    public async Task QueryAsyncSimpleRows_WithBreezyWithCache()
    {
        var houses = await _mySqlConnection.QueryAsync<House>("SELECT * FROM house", _memoryCacheableQuery);
    }

    [Benchmark]
    public async Task QueryAsyncSimpleRows_WithDapper()
    {
        var houses = await _mySqlConnection.QueryAsync<House>("SELECT * FROM house", commandType:CommandType.Text);
    }
    
    [Benchmark]
    public async Task QueryFirstAsyncSimpleRows_WithBreezy()
    {
        var house = await _mySqlConnection.QueryFirstOrDefaultAsync<House>("SELECT * FROM house WHERE id = 5000");
    }
    
    [Benchmark]
    public async Task QueryFirstAsyncSimpleRows_WithBreezyWithCache()
    {
        var house = await _mySqlConnection.QueryFirstOrDefaultAsync<House>("SELECT * FROM house WHERE id = 5000", _memoryCacheableQuery);
    }

    [Benchmark]
    public async Task QueryFirstAsyncSimpleRows_WithDapper()
    {
        var houses = await _mySqlConnection.QueryFirstAsync<House>("SELECT * FROM house WHERE Id = 5000", commandType:CommandType.Text);
    }
    
    [Benchmark]
    public async Task QueryAsyncOneToManyRows_WithBreezy()
    {
        var users = await _mySqlConnection.QueryAsync<UserCopy>("SELECT * FROM test.user_copy uc INNER JOIN car_users cu on uc.id = cu.user_id INNER JOIN house_users hu ON hu.user_id = uc.id  ORDER BY uc.id ASC");
    }
    
    [Benchmark]
    public async Task QueryAsyncOneToManyRows_WithBreezyWithCache()
    {
        var users = await _mySqlConnection.QueryAsync<UserCopy>("SELECT * FROM test.user_copy uc INNER JOIN car_users cu on uc.id = cu.user_id INNER JOIN house_users hu ON hu.user_id = uc.id  ORDER BY uc.id ASC", _memoryCacheableQueryBis);
    }

    [Benchmark]
    public async Task QueryAsyncOneToManyRows_WithDapper()
    {
        var users = new Dictionary<int, UserCopy>();
        var cars = new Dictionary<int, CarUser>();
        var houses = new Dictionary<int, HouseUser>();

        var user = await _mySqlConnection.QueryAsync<UserCopy, CarUser, HouseUser, UserCopy>(
            @"SELECT * FROM test.user_copy uc INNER JOIN car_users cu on uc.id = cu.user_id INNER JOIN house_users hu ON hu.user_id = uc.id ORDER BY uc.id ASC",
            (user, car, house) =>
            {
                UserCopy userEntry;
                CarUser carEntry;
                HouseUser houseEntry;

                if (!users.TryGetValue(user.Id, out userEntry))
                {
                    userEntry = user;
                    users.Add(user.Id, userEntry);
                }

                if (!cars.TryGetValue(car.Id, out carEntry))
                {
                    carEntry = car;
                    cars.Add(car.Id, carEntry);
                    userEntry.Cars.Add(car);
                }

                if (!houses.TryGetValue(house.Id, out houseEntry))
                {
                    houseEntry = house;
                    houses.Add(house.Id, houseEntry);
                    userEntry.Houses.Add(house);
                }
                
                return userEntry;
            });
    }
    
    [Benchmark]
    public async Task QueryFirstAsyncOneToManyRows_WithBreezy()
    {
        var users = await _mySqlConnection.QueryFirstOrDefaultAsync<UserCopy>("SELECT * FROM test.user_copy uc INNER JOIN car_users cu on uc.id = cu.user_id INNER JOIN house_users hu ON hu.user_id = uc.id  WHERE uc.id = 5000");
    }
    
    [Benchmark]
    public async Task QueryFirstAsyncOneToManyRows_WithBreezyWithCache()
    {
        var users = await _mySqlConnection.QueryFirstOrDefaultAsync<UserCopy>("SELECT * FROM test.user_copy uc INNER JOIN car_users cu on uc.id = cu.user_id INNER JOIN house_users hu ON hu.user_id = uc.id  WHERE uc.id = 5000", _memoryCacheableQueryBis);
    }
    
    [Benchmark]
    public async Task QueryFirstAsyncOneToManyRows_WithDapper()
    {
        var users = new Dictionary<int, UserCopy>();
        var cars = new Dictionary<int, CarUser>();
        var houses = new Dictionary<int, HouseUser>();

        var user = (await _mySqlConnection.QueryAsync<UserCopy, CarUser, HouseUser, UserCopy>(
            @"SELECT * FROM test.user_copy uc INNER JOIN car_users cu on uc.id = cu.user_id INNER JOIN house_users hu ON hu.user_id = uc.id WHERE uc.id = 5000",
            (user, car, house) =>
            {
                UserCopy userEntry;
                CarUser carEntry;
                HouseUser houseEntry;

                if (!users.TryGetValue(user.Id, out userEntry))
                {
                    userEntry = user;
                    users.Add(user.Id, userEntry);
                }

                if (!cars.TryGetValue(car.Id, out carEntry))
                {
                    carEntry = car;
                    cars.Add(car.Id, carEntry);
                    userEntry.Cars.Add(car);
                }

                if (!houses.TryGetValue(house.Id, out houseEntry))
                {
                    houseEntry = house;
                    houses.Add(house.Id, houseEntry);
                    userEntry.Houses.Add(house);
                }
                
                return userEntry;
            })).FirstOrDefault();
    }
}