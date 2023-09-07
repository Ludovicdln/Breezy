﻿using System.Text;
using Breezy.Core.Builder.Functions;
using Breezy.Core.IO.Definitions.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Breezy.Core.Builder;

public class DbTemplateBuilder : IDbTemplateBuilder
{
    protected readonly ClassDbDefinition _classDbDefinition;
    
    private readonly StringBuilder _stringBuilder = new();

    public DbTemplateBuilder(ClassDbDefinition classDbDefinition) => _classDbDefinition = classDbDefinition;
    
    public string Build(CancellationToken cancellationToken = default)
    {
        _stringBuilder.Insert(0, @$"{GenerateUsing()} 
                public static class {_classDbDefinition.Name}Extensions 
                                {{");

        _stringBuilder.Append($"}} }}");
        
        return _stringBuilder.ToString();
    }

    public IDbTemplateBuilder GenerateQueryAsyncFunction()
    {
        var queryAsyncFunctionBuilder = 
            new QueryAsyncFunctionBuilder(in _classDbDefinition);

        _stringBuilder.Append(queryAsyncFunctionBuilder
            .GenerateSummaries()    
            .GenerateConstructor()
            .GenerateBody()
            .Build());
        
        _stringBuilder.Append(queryAsyncFunctionBuilder
            .GenerateSummaries(withParam: true) 
            .GenerateConstructor(withParam: true)
            .GenerateBody(withParam: true)
            .Build());
        
        _stringBuilder.Append(queryAsyncFunctionBuilder
            .GenerateSummaries(withCache: true)
            .GenerateConstructor(withCache: true)
            .GenerateBody(withCache: true)
            .Build());
        
        _stringBuilder.Append(queryAsyncFunctionBuilder
            .GenerateSummaries(withParam: true, withCache: true)
            .GenerateConstructor(withParam: true, withCache: true)
            .GenerateBody(withParam: true, withCache: true)
            .Build());
        
        return this;
    }
    
    public IDbTemplateBuilder GenerateQueryFirstAsyncFunction()
    {
        var queryFirstAsyncFunctionBuilder = 
            new QueryFirstAsyncFunctionBuilder(in _classDbDefinition);
        
        _stringBuilder.Append(queryFirstAsyncFunctionBuilder
            .GenerateSummaries()
            .GenerateConstructor()
            .GenerateBody()
            .Build());
        
        _stringBuilder.Append(queryFirstAsyncFunctionBuilder
            .GenerateSummaries(withParam: true)
            .GenerateConstructor(withParam: true)
            .GenerateBody(withParam: true)
            .Build());
        
        _stringBuilder.Append(queryFirstAsyncFunctionBuilder
            .GenerateSummaries(withCache: true)
            .GenerateConstructor(withCache: true)
            .GenerateBody(withCache: true)
            .Build());
        
        _stringBuilder.Append(queryFirstAsyncFunctionBuilder
            .GenerateSummaries(withParam: true, withCache: true)
            .GenerateConstructor(withParam: true, withCache: true)
            .GenerateBody(withParam: true, withCache: true)
            .Build());

        return this;
    }
    
    public IDbTemplateBuilder GenerateExecuteAsyncFunction()
    {
        var executeAsyncFunctionBuilder =
            new ExecuteAsyncFunctionBuilder(in _classDbDefinition);

        _stringBuilder.Append(executeAsyncFunctionBuilder
            .GenerateSummaries()
            .GenerateConstructor()
            .GenerateBody()
            .Build());

        _stringBuilder.Append(executeAsyncFunctionBuilder
            .GenerateSummaries(withParam: true)
            .GenerateConstructor(withParam: true)
            .GenerateBody(withParam: true)
            .Build());
        
        _stringBuilder.Append(executeAsyncFunctionBuilder
            .GenerateSummaries(withParam: false, withTransaction: true)
            .GenerateConstructor(withParam: false, withTransaction: true)
            .GenerateBody(withParam: false, withTransaction: true)
            .Build());
        
        _stringBuilder.Append(executeAsyncFunctionBuilder
            .GenerateSummaries(withParam: true, withTransaction: true)
            .GenerateConstructor(withParam: true, withTransaction: true)
            .GenerateBody(withParam: true, withTransaction: true)
            .Build());
        
        return this;
    }
    
    private string GenerateUsing()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append("// <auto-generated /> \n");
        stringBuilder.Append(@$"using System.Collections.Generic;
        using System.Data;
        using System.Data.Common;
        using System.Threading.Tasks;
        using System.Collections.Generic;
        using Breezy; 
        {string.Join("\n", _classDbDefinition.Dependencies)}

        namespace {_classDbDefinition.Namespace}
        {{");

        return stringBuilder.ToString();
    }
}