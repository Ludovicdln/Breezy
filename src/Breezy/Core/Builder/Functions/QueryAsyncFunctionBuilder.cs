using System.Text;
using Breezy.Core.IO.Definitions.Class;
using Breezy.Core.IO.Definitions.Properties;
using Breezy.Enums;
using Breezy.Extensions;

namespace Breezy.Core.Builder.Functions;

public sealed class QueryAsyncFunctionBuilder : FunctionBuilder
{
    public QueryAsyncFunctionBuilder(in ClassDbDefinition classDbDefinition)
        : base(in classDbDefinition)
    {
    }

    public override FunctionBuilder GenerateSummaries(bool withParam = false, bool withTransaction = false, bool withCache = false)
    {
        _classBuilds.Clear();
        _stringBuilder.Clear();
        
        _stringBuilder.AppendLine("""
        /// <summary>
        /// Execute a query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of results to return.</typeparam>
        /// <param name="sql">The SQL to execute for the query.</param>
        """);
        
        base.GenerateSummaries(withParam, withTransaction, withCache);
        
        _stringBuilder.AppendLine("""
        /// <param name="cancellationToken">The cancellation token for this command.</param>
        /// <returns>
        /// A sequence of data of <typeparamref name="T"/>;
        /// </returns>
        """);
        
        return this;
    }

    public override FunctionBuilder GenerateConstructor(bool withParam = false, bool withTransaction = false, bool withCache = false)
    {
        var optionsBuilder = new StringBuilder();
        
        if (withParam)
            optionsBuilder.Append("object param,");

        if (withCache)
            optionsBuilder.Append($"ICacheableQuery<{_classDbDefinition.Name}> cacheableQuery,");

        _stringBuilder.Append($@"public static async Task<IEnumerable<{_classDbDefinition.Name}>> QueryAsync<T>(this DbConnection connection, string sql, {optionsBuilder} 
                CancellationToken cancellationToken = default)  where T : {_classDbDefinition.Name} {{");

        return this;
    }

    public override FunctionBuilder GenerateBody(bool withParam = false, bool withTransaction = false, bool withCache = false)
    {
        base.GenerateBody(withParam, withTransaction, withCache);
        
        if (withCache)
        {
            _stringBuilder.Append("""               
                var identityQuery = new IdentityQuery(sql);
                var cacheableResults = await cacheableQuery.GetCacheableResultsAsync(identityQuery);

                if (cacheableResults.Any()) return cacheableResults;                
            """);
        }
        
        _stringBuilder.Append("await using var reader = await command.ExecuteReaderAsync(cancellationToken: cancellationToken);");

        var hasManyRelations = _classDbDefinition.HasManyRelations(out var relationType);
        
        _stringBuilder.Append(hasManyRelations
            ? $"var {_classDbDefinition.Name.ToCamelCase()}s = new Dictionary<{_classDbDefinition.PropertyDbDefinitions.ElementAt(0).Type}, {_classDbDefinition.Name}>();"
            : $"var results = new List<{_classDbDefinition.Name}>();");

        var cacheTypesExist = hasManyRelations ? new List<(string, string)>() { (_classDbDefinition.PropertyDbDefinitions.ElementAt(0).Type, _classDbDefinition.Name) } : new List<(string, string)>();
        
        _stringBuilder.Append(GenerateCacheProperties(new StringBuilder(), _classDbDefinition, cacheTypesExist));
        
        _stringBuilder.Append(@"
            try {
                  while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                  {");
        
        foreach (var (_, name) in cacheTypesExist)
            _stringBuilder.Append($"{name} {name.ToCamelCase()} = null;");

        _stringBuilder.Append(GenerateReaderProperties(new StringBuilder(), _classDbDefinition, cacheTypesExist));
        
        var result = hasManyRelations ? $"{_classDbDefinition.Name.ToCamelCase()}s.Values" : "results";

        switch (hasManyRelations)
        {
            case true when relationType != RelationType.ONE_TO_MANY:
            {
                foreach (var propertyDbDefinition in _classDbDefinition.PropertyDbDefinitions
                             .Where(x => x is { IsForeignKey: true, CannotBeRead: false }))
                {
                    _stringBuilder.Append(propertyDbDefinition.IsCollection
                        ? $"{_classDbDefinition.Type.ToCamelCase()}.{propertyDbDefinition.Name}.Add({propertyDbDefinition.Type.ToCamelCase()});"
                        : $"{_classDbDefinition.Type.ToCamelCase()}.{propertyDbDefinition.Name} = {propertyDbDefinition.Type.ToCamelCase()};");
                }

                break;
            }
            case false:
                _stringBuilder.Append($"results.Add({_classDbDefinition.Name.ToCamelCase()});");
                break;
        }

        _stringBuilder.Append(withCache ? $@"}} await cacheableQuery.SetCacheableResultsAsync(identityQuery, {result}); return {result}; }} finally {{ reader.Close(); if (wasClosed)connection.Close(); }} }}" 
            : $@"}}   return {result}; }} finally {{ reader.Close(); if (wasClosed)connection.Close(); }} }}");
        
        return this;
    }
}