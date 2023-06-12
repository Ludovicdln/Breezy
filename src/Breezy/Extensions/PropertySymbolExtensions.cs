using Breezy.Core.IO.Definitions.Properties;
using Microsoft.CodeAnalysis;

namespace Breezy.Extensions;

public static class PropertySymbolExtensions
{
    public static PropertyDefinition ToPropertyDefinition(this IPropertySymbol propertySymbol)
    {
        var isCollection = propertySymbol.Type.IsCollection();
        
        var baseType = isCollection && propertySymbol.Type is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.TypeArguments[0] : propertySymbol.Type;
        
        return new PropertyDefinition(baseType.TryGetNamespace(out var nameSpace) ? nameSpace : string.Empty, 
            baseType.GetTypeName(), propertySymbol.Name,
            isCollection, baseType.IsReferenceType(), baseType.IsForeignKey(), baseType);
    }
    
    public static IList<PropertyDefinition> ToPropertiesDefinition(this IEnumerable<IPropertySymbol> propertySymbols)
        => propertySymbols.Select(propertySymbol => propertySymbol.ToPropertyDefinition()).ToList();
}