using System.Text;
using Breezy.Core.IO.Definitions.Class;
using Breezy.Core.IO.Definitions.Extensions;
using Breezy.Core.IO.Definitions.Properties;
using Breezy.Enums;
using Breezy.Extensions;

namespace Breezy.Core.Builder.Functions;

public class FunctionBuilder
{
    protected readonly List<int> _classBuilds = new ();

    protected readonly ClassDbDefinition _classDbDefinition;
    
    protected StringBuilder _stringBuilder = new ();

    protected FunctionBuilder(in ClassDbDefinition classDbDefinition) => _classDbDefinition = classDbDefinition;

    public virtual FunctionBuilder GenerateSummaries(bool withParam = false, bool withTransaction = false, bool withCache = false)
    {
        if (withParam)
            _stringBuilder.AppendLine(""" 
             /// <param name="param">The parameters to pass, if any.</param>
            """);
        
        if (withCache)
            _stringBuilder.AppendLine(""" 
             /// <param name="cacheableQuery">The cache that you need to impl, if you want to be faster.</param>
            """);

        if (withTransaction)
            _stringBuilder.AppendLine(""" 
            /// <param name="transaction">The transaction to use for this query.</param>
            """);
        
        return this;
    }

    public virtual FunctionBuilder GenerateConstructor(bool withParam = false, bool withTransaction = false,
        bool withCache = false)
    {
        return this;
    }

    public virtual FunctionBuilder GenerateBody(bool withParam = false, bool withTransaction = false, bool withCache = false)
    {
        _stringBuilder.Append(@"bool wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed)
            await connection.OpenAsync(cancellationToken);");

        if (withTransaction)
        {
            if (withParam)
            {
                _stringBuilder.Append(@" var commands = new DbCommand[sql.Length];
                    for (var i = 0; i < sql.Length; i++)
                    {
                        await using var command = connection.CreateCommand();
                        command.CommandText = sql[i];
                        command.Transaction = transaction;
                  
                        var paramt = param[i];
                        
                        foreach (var property in paramt.GetType().GetProperties())
                        {
                            var parameter = command.CreateParameter();
                            parameter.ParameterName = ""@"" + property.Name;
                            parameter.Value = property.GetValue(paramt);
                            command.Parameters.Add(parameter);
                        }

                        commands[i] = command;
                    }       ");
                return this;
            }

            _stringBuilder.Append(@"
            
            var commands = new DbCommand[sql.Length];
            for (var i = 0; i < sql.Length; i++)
            {
                await using var command = connection.CreateCommand();
                command.CommandText = sql[i];
                command.Transaction = transaction;
                commands[i] = command;
            }");

            return this;

        }
        
        _stringBuilder.Append(@"
        await using var command = connection.CreateCommand();
        command.CommandText = sql;");

        if (!withParam) return this;
        
        _stringBuilder.Append(@"          
                foreach (var property in param.GetType().GetProperties())
                {
                   var parameter = command.CreateParameter();
                   parameter.ParameterName = ""@"" + property.Name;
                   parameter.Value = property.GetValue(param);
                   command.Parameters.Add(parameter);
                 }                                               
            ");

        return this;
        
    }

    public virtual string Build() => _stringBuilder.ToString();
    
    protected string GenerateCacheProperties(StringBuilder sb, in ClassDbDefinition classDbDefinition, ICollection<(string, string)> cacheTypesExist)
    {
        foreach (var property in classDbDefinition.PropertyDbDefinitions.Where(property => property.IsForeignKey 
                     && cacheTypesExist.All(x => x.Item2 != property.Type)))
        {
            sb.Append($"var {property.Type.ToCamelCase()}s = new Dictionary<{property.ClassDbType?.PropertyDbDefinitions.ElementAt(0).Type}, {property.Type}>();");
                
            cacheTypesExist.Add((property.ClassDbType?.PropertyDbDefinitions.ElementAt(0).Type!, property.Type));
                
            GenerateCacheProperties(sb, property.ClassDbType!, cacheTypesExist);
        }

        return sb.ToString();
    }

    protected string GenerateReaderProperties(StringBuilder sb, ClassDbDefinition classDbDefinition,
        IEnumerable<(string, string)> cacheTypesExist, Tuple<ClassDbDefinition, PropertyDbDefinition>? parentDbDefinition = null)
    {
        var (type, name) = cacheTypesExist.FirstOrDefault(x => string.Equals(x.Item2, classDbDefinition.Type));

        if (!string.IsNullOrEmpty(name))
        {
            var property = classDbDefinition.PropertyDbDefinitions.ElementAt(0);
            
            var reader = property.TryGetDbReader(out var dbReader) ? $"reader.{dbReader}" : $"({type})reader.{dbReader}";
            
            sb.Append($"var {classDbDefinition.UniqueName}Id = reader.IsDBNull({property.Ordinal}) ? default : {reader}({property.Ordinal});");
            
            sb.Append(
                $"if (!{name.ToCamelCase()}s.TryGetValue({classDbDefinition.UniqueName}Id, out {classDbDefinition.Type.ToCamelCase()})) {{");

            sb.Append($"{classDbDefinition.Type.ToCamelCase()} = new {classDbDefinition.Type}() {{");
            
            sb.Append(ReadProperties(new StringBuilder(), classDbDefinition, cacheTypesExist, true));

            if (!_classBuilds.Contains(classDbDefinition.Id))
            {
                _classBuilds.Add(classDbDefinition.Id);
                sb.Append("};");
                
                sb.Append(GeneratePropertiesUnReadable(new StringBuilder(), classDbDefinition));
                
                sb.Append(@$"{name.ToCamelCase()}s.Add({classDbDefinition.UniqueName}Id, {classDbDefinition.Type.ToCamelCase()});");
                
                if (parentDbDefinition != null && parentDbDefinition.Item1.HasManyRelations(out var relationType) &&
                    relationType == RelationType.ONE_TO_MANY && parentDbDefinition.Item2.IsCollection)
                {
                    var (parentClassDbDef, childrenPropertyDbDef) = parentDbDefinition;

                    sb.Append($"{parentClassDbDef.Type.ToCamelCase()}.{childrenPropertyDbDef.Name}.Add({classDbDefinition.Type.ToCamelCase()}); }}");
                }
                else
                {
                    sb.Append('}');
                }
            }

        }
        else
        {
            sb.Append($"var {classDbDefinition.Name.ToCamelCase()} = new {classDbDefinition.Type} {{");
            
            sb.Append(ReadProperties(new StringBuilder(), classDbDefinition, cacheTypesExist));

            if (_classBuilds.Contains(classDbDefinition.Id)) return sb.ToString();
            
            _classBuilds.Add(classDbDefinition.Id);
            
            sb.Append("};");
        }
        
        return sb.ToString();
    }
    
    private string ReadProperties(StringBuilder sb, in ClassDbDefinition classDbDefinition, IEnumerable<(string, string)> cacheTypesExist, 
        bool fromDictionary = false)
    {
        for (var i = 0; i < classDbDefinition.PropertyDbDefinitions.Count(); i++)
        {
            var property = classDbDefinition.PropertyDbDefinitions.ElementAt(i);

            if (property is { IsForeignKey: false, IsReference: false })
            {
                var reader = property.TryGetDbReader(out var dbReader) ? $"reader.{dbReader}" : $"({property.Type})reader.{dbReader}";
                
                sb.Append(i == 0 && fromDictionary ?
                    $"{property.Name} = {classDbDefinition.UniqueName}Id,"
                    : $"{property.Name} = reader.IsDBNull({property.Ordinal}) ? default : {reader}({property.Ordinal}),");
                continue;
            }

            if (property.IsForeignKey)
            {
                if (property.CannotBeRead)
                {
                    if (!property.IsCollection)
                        sb.Append($"{property.Name} = {property.Type.ToCamelCase()},");
                    continue;
                }

                if (!_classBuilds.Contains(classDbDefinition.Id))
                {
                    _classBuilds.Add(classDbDefinition.Id);
                    sb.Append("};");
                    
                    sb.Append(GeneratePropertiesUnReadable(new StringBuilder(), classDbDefinition));

                    if (fromDictionary)
                    {
                        var name = classDbDefinition.Type.ToCamelCase();
                        sb.Append(@$"{name}s.Add({classDbDefinition.UniqueName}Id, {name}); }}");
                    }
                }
                
                GenerateReaderProperties(sb, property.ClassDbType!, cacheTypesExist, 
                    new Tuple<ClassDbDefinition, PropertyDbDefinition>(classDbDefinition, property));
                
                if (classDbDefinition.Id != _classDbDefinition.Id)
                {
                    sb.Append(property.IsCollection
                        ? $"{classDbDefinition.Type.ToCamelCase()}.{property.Name}.Add({property.Type.ToCamelCase()});"
                        : $"{classDbDefinition.Type.ToCamelCase()}.{property.Name} = {property.Type.ToCamelCase()};");
                }

                continue;
            }

            if (property is not { IsReference: true, IsCollection: false }) continue;
            
            sb.Append($"{property.Name} = new {property.Type} {{");
            
            sb.Append(ReadProperties(new StringBuilder(), property.ClassDbType!, cacheTypesExist) + "},");
        }
        
        return sb.ToString();
    }
    
    private string GeneratePropertiesUnReadable(StringBuilder sb, ClassDbDefinition classDbDefinition)
    {
        foreach (var propertyDbDefinition in classDbDefinition.PropertyDbDefinitions.Where(x => x is { CannotBeRead: true, IsCollection: true }))
            sb.Append($"{classDbDefinition.Type.ToCamelCase()}.{propertyDbDefinition.Name}.Add({propertyDbDefinition.Type.ToCamelCase()});");
        
        return sb.ToString();
    }
}