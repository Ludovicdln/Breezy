using Breezy.Core.IO.Converter;
using Microsoft.CodeAnalysis;

namespace Breezy.Core.IO.Definitions.Properties;

public sealed class PropertyDefinition : IConverted
{
    public PropertyDefinition(string nameSpace, string type, string name, bool isCollection, bool isReferenceType, bool isForeignKey, ITypeSymbol typeSymbol, bool cannotBeRead = false)
        => (Namespace, Type, Name, IsCollection, IsReferenceType, IsForeignKey, TypeSymbol, CannotBeRead) 
            = (nameSpace, type, name, isCollection, isReferenceType, isForeignKey, typeSymbol, cannotBeRead);
    
    public string Namespace { get; } 
    
    public string Type { get; }
        
    public string Name { get; }
    
    public bool IsCollection { get; }
        
    public bool IsReferenceType { get; }
        
    public bool IsForeignKey { get; }
    
    public ITypeSymbol TypeSymbol { get; }

    public bool CannotBeRead { get; }
    
    public bool IsConvertible() => true;
}