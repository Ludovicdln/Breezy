// See https://aka.ms/new-console-template for more information

using Breezy.Sample.Domain.Entities;
using Breezy.Sample.Infrastructure.Caching;
using Npgsql;

var connection = new NpgsqlConnection("your connection");

var memoryCache = new MemoryCacheableQuery<User>();

var users = await connection.QueryAsync<User>("SELECT * FROM user", memoryCache);

var user = await connection.QueryFirstOrDefaultAsync<User>("SELECT * FROM user WHERE id = @Id", new {Id = 1}, memoryCache);