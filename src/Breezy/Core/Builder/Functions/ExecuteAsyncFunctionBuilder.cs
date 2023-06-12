using Breezy.Core.IO.Definitions.Class;

namespace Breezy.Core.Builder.Functions;

public sealed class ExecuteAsyncFunctionBuilder : FunctionBuilder
{
    public ExecuteAsyncFunctionBuilder(in ClassDbDefinition classDbDefinition) 
        : base(in classDbDefinition)
    {
    }

    public override FunctionBuilder GenerateSummaries(bool withParam = false, bool withTransaction = false, bool withCache = false)
    {
        _classBuilds.Clear();
        _stringBuilder.Clear();
        
        _stringBuilder.AppendLine("""
        /// <summary>
        /// Execute a command asynchronously using Task.
        /// </summary>       
        /// <param name="sql">The SQL to execute for the query.</param>
        """);
        
        base.GenerateSummaries(withParam, withTransaction, withCache);
        
        _stringBuilder.AppendLine("""
        /// <returns>The number of rows affected.</returns>
        """);

        return this;
    }

    public override FunctionBuilder GenerateConstructor(bool withParam = false, bool withTransaction = false, bool withCache = false)
    {
        _stringBuilder.Append(withTransaction ? "public static async Task<int[]> ExecuteAsync(this DbConnection connection, string[] sql," : 
            @"public static async Task<int> ExecuteAsync(this DbConnection connection, string sql,");

        if (withParam)
            _stringBuilder.Append(withTransaction ? "object[] param," : "object param,");

        if (withTransaction)
            _stringBuilder.Append("DbTransaction transaction,");
        
        _stringBuilder.Append("CancellationToken cancellationToken = default) {");

        return this;
    }

    public override FunctionBuilder GenerateBody(bool withParam = false, bool withTransaction = false, bool withCache = false)
    {
        base.GenerateBody(withParam, withTransaction, withCache);

        _stringBuilder.Append("try {");

        _stringBuilder.Append(withTransaction
            ? @"                          
                var results = new int[sql.Length];
                
                for (var i = 0; i < commands.Length; i++)             
                    results[i] = await commands[i].ExecuteNonQueryAsync(cancellationToken);
                
                await transaction.CommitAsync();          
                return results;
               }

            catch(DbException e) 
             {
                await transaction.RollbackAsync();
                return Array.Empty<int>();
             }"
            : "return await command.ExecuteNonQueryAsync(cancellationToken); }");
        
        _stringBuilder.Append(withTransaction ? "finally { transaction.Dispose(); if (wasClosed)connection.Close(); } }" : "finally { if (wasClosed)connection.Close(); } }");
        
        return this;
    }
}