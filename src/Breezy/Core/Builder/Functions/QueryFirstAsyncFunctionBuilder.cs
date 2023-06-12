using System.Text;
using Breezy.Core.IO.Definitions.Class;

namespace Breezy.Core.Builder.Functions;

public sealed class QueryFirstAsyncFunctionBuilder : FunctionBuilder
{
    public QueryFirstAsyncFunctionBuilder(in ClassDbDefinition classDbDefinition) 
        : base(in classDbDefinition)
    {
    }

    public override FunctionBuilder GenerateSummaries(bool withParam = false, bool withTransaction = false, bool withCache = false)
    {
        _classBuilds.Clear();
        _stringBuilder.Clear();
        
        _stringBuilder.AppendLine("""
        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="sql">The SQL to execute for the query.</param>
        """);
        
        base.GenerateSummaries(withParam, withTransaction, withCache);
        
        _stringBuilder.AppendLine("""
        /// <param name="cancellationToken">The cancellation token for this command.</param>
        /// <returns>
        /// A first sequence of data of <typeparamref name="T"/>;
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
        
        _stringBuilder.Append($@"public static async Task<{_classDbDefinition.Name}?> QueryFirstOrDefaultAsync<T>(this DbConnection connection, string sql, {optionsBuilder}
                CancellationToken cancellationToken = default)  where T : {_classDbDefinition.Name} {{");

        return this;
    }
    
    public override FunctionBuilder GenerateBody(bool withParam = false, bool withTransaction = false, bool withCache = false)
    {
        var optionsBuilder = new StringBuilder();

        if (withParam)
            optionsBuilder.Append("param,");

        if (withCache)
            optionsBuilder.Append($"cacheableQuery,");
        
        _stringBuilder.Append($"return (await connection.QueryAsync<{_classDbDefinition.Name}>(sql, {optionsBuilder} cancellationToken)).FirstOrDefault(); }}");

        return this;
    }
}