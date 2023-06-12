using System.Globalization;
using Breezy.Core.IO.Definitions.Properties;
using Microsoft.CodeAnalysis;

namespace Breezy.Extensions;

public static class TypeSymbolExtensions
{
    private const string INT_32 = "Int32";
    private const string INT_64 = "Int64";
    private const string INT_128 = "Int128";

    private const string STRING = "String";
    private const string BOOLEAN = "Boolean";
    private const string DOUBLE = "Double";
    
    public static bool IsCollection(this ITypeSymbol propertyType) => propertyType.Name.Contains("List");
    
    public static bool IsReferenceType(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.IsForeignKey() || (typeSymbol.IsReferenceType && !typeSymbol.IsNativeReference());
    }

    public static bool IsForeignKey(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes().FirstOrDefault(x 
            => string.Equals(x.AttributeClass?.Name.ToString(CultureInfo.InvariantCulture), "TableAttribute", StringComparison.OrdinalIgnoreCase)) != null;
    }

    public static bool TryGetNamespace(this ITypeSymbol typeSymbol, out string nameSpace)
    {
        nameSpace = string.Empty;

        if (typeSymbol.ContainingNamespace.ToDisplayString().Contains("System")) return false;

        nameSpace = typeSymbol.ContainingNamespace.ToDisplayString();
        
        return true;
    }
    
    public static string GetTypeName(this ITypeSymbol typeSymbol)
    {
        var typeName = typeSymbol.GetName();

        return typeName switch
        {
            INT_32 => "int",
            INT_64 => "int",
            INT_128 => "int",
            STRING => "string",
            BOOLEAN => "bool",
            DOUBLE => "double",
            _ => typeName
        };
    }
    
    private static bool IsNativeReference(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_String => true,
            _ => false
        };
    }
    
    public static IList<PropertyDefinition> ToPropertiesDefinition(this ITypeSymbol typeSymbol)
    {
        var members = typeSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(x => !x.ContainingAssembly
                .ToDisplayString()
                .Equals(BreezyGenerator.SourceGeneratorAssemblyName, StringComparison.OrdinalIgnoreCase));

        return members.ToPropertiesDefinition();
    }

    private static string GetName(this ISymbol typeSymbol) => typeSymbol.Name.Replace("System.", "");
}