using System.Collections;
using System.Data;
using System.Data.Common;
using Breezy.Benchmarks.Domain.Models;
using Breezy.Benchmarks.Domain.Models.Constraints;
using Breezy.Benchmarks.Domain.Models.Enums;

namespace Breezy.Benchmarks.Extensions;

public static class CustomSampleReaderExtensions
{
    public static async Task<IEnumerable<House>> CustomQueryAsync<T>(this DbConnection connection, string sql, CancellationToken cancellationToken = default)
        where T : House
    {
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed)
            await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var results = new List<House>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        try
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var house = new House
                {
                    Id = reader[0] != DBNull.Value ? reader.GetFieldValue<int>(0) : default,
                    Name = reader[1] != DBNull.Value ? reader.GetFieldValue<string>(1): default,
                    Tag = reader[2] != DBNull.Value ? reader.GetFieldValue<string>(2): default,
                    Members = reader[3] != DBNull.Value ?  reader.GetFieldValue<int>(3): default,
                };
                results.Add(house);
            }

            return results;
        }
        finally
        {
            await reader.DisposeAsync();
        }
    }
    
    public static async Task<IEnumerable<UserCopy>> CustomQueryAsync2<T>(this DbConnection connection, string sql, CancellationToken cancellationToken = default)
            where T : UserCopy
        {
            bool wasClosed = connection.State == ConnectionState.Closed;
            if (wasClosed)
                await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken: cancellationToken);
            var userCopys = new Dictionary<int, UserCopy>();
            var carUsers = new Dictionary<int, CarUser>();
            var houseUsers = new Dictionary<int, HouseUser>();
            try
            {
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    UserCopy userCopy = null;
                    CarUser carUser = null;
                    HouseUser houseUser = null;
                    var userCopy9Id = reader.IsDBNull(0) ? default : reader.GetInt32(0);
                    if (!userCopys.TryGetValue(userCopy9Id, out userCopy))
                    {
                        userCopy = new UserCopy()
                        {
                            Id = userCopy9Id, 
                            Firstname = reader.IsDBNull(1) ? default : reader.GetString(1), 
                            Lastname = reader.IsDBNull(2) ? default : reader.GetString(2), 
                            Birthday = reader.IsDBNull(3) ? default : reader.GetDateTime(3), 
                            Gender = reader.IsDBNull(4) ? default : (Gender)reader.GetValue(4), 
                            IsMinor = reader.IsDBNull(5) ? default : reader.GetBoolean(5), 
                        };
                        userCopys.Add(userCopy9Id, userCopy);
                    }

                    var carUser7Id = reader.IsDBNull(6) ? default : reader.GetInt32(6);
                    if (!carUsers.TryGetValue(carUser7Id, out carUser))
                    {
                        carUser = new CarUser()
                        {
                            Id = carUser7Id, 
                            Model = reader.IsDBNull(7) ? default : reader.GetString(7), 
                            Price = reader.IsDBNull(8) ? default : reader.GetDouble(8),
                        };
                        carUsers.Add(carUser7Id, carUser);
                        userCopy.Cars.Add(carUser);
                    }

                    var houseUser8Id = reader.IsDBNull(10) ? default : reader.GetInt32(10);
                    if (!houseUsers.TryGetValue(houseUser8Id, out houseUser))
                    {
                        houseUser = new HouseUser()
                        {
                            Id = houseUser8Id, 
                            Name = reader.IsDBNull(11) ? default : reader.GetString(11), 
                            Tag = reader.IsDBNull(12) ? default : reader.GetString(12), 
                            Members = reader.IsDBNull(13) ? default : reader.GetInt32(13), 
                        };
                        houseUsers.Add(houseUser8Id, houseUser);
                        userCopy.Houses.Add(houseUser);
                    }
                }
                
                return userCopys.Values;
            }
            finally
            {
                reader.Close();
                if (wasClosed)
                    connection.Close();
            }
        }
    
    
    /*public class IdentityQuery : IEquatable<IdentityQuery>
    {
        private readonly int _hashCodeSql;
        private readonly int? _hashCodeParam;

        public IdentityQuery(string sql, object? param = null) 
            => (_hashCodeSql, _hashCodeParam) = (sql.GetHashCode(), param?.GetHashCode());

        public bool Equals(IdentityQuery? other)
        {
            if (ReferenceEquals(other, this)) return true;

            return this.GetHashCode() == other?.GetHashCode();
        }

        public override string ToString() 
            => $"{_hashCodeSql.ToString()}-{_hashCodeParam?.ToString()}";

        public override bool Equals(object? obj)
        {
            return Equals(obj as IdentityQuery);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_hashCodeSql, _hashCodeParam);
        }
    }


    public interface ICacheableQuery<T> where T : class
    {
        public Task<IEnumerable<T>> GetCacheableResultsAsync(IdentityQuery identityQuery);

        public Task SetCacheableResultsAsync(IdentityQuery identityQuery, IEnumerable<T> results);
    }*/
}