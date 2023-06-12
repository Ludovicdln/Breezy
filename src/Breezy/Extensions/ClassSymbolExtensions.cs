using Breezy.Core.IO.Attributes;
using Breezy.Core.IO.Definitions.Class;
using Microsoft.CodeAnalysis;

namespace Breezy.Extensions;

public static class ClassSymbolExtensions
{
    public static ClassDefinition ToClassDefinition(this INamedTypeSymbol classSymbol)
    {
        var className = classSymbol.Name;
        
        var isSealed = classSymbol.IsSealed;

        var namespaceName = classSymbol.ContainingNamespace.ToString();
        
        var properties = classSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(x => !x.ContainingAssembly
                .ToDisplayString()
                .Equals(BreezyGenerator.SourceGeneratorAssemblyName, StringComparison.OrdinalIgnoreCase)).ToPropertiesDefinition();

        var dependencies = new List<string>();

        foreach (var propertyDefinition in properties)
        {
            if (propertyDefinition.Namespace != string.Empty && !dependencies.Contains($"using {propertyDefinition.Namespace};", StringComparer.OrdinalIgnoreCase))
                dependencies.Add($"using {propertyDefinition.Namespace};");
        }

        var splitOnAttribute = classSymbol.GetAttributes().FirstOrDefault(x =>
            x.AttributeClass!.Name.Equals(SplitOnAttribute.Name, StringComparison.OrdinalIgnoreCase));
        
        return new ClassDefinition(dependencies, namespaceName, className, isSealed, properties, splitOnAttribute != null 
            ?  splitOnAttribute.ConstructorArguments[0].Values.Select(x => (int)x.Value!).ToArray() : Array.Empty<int>());
    }
}