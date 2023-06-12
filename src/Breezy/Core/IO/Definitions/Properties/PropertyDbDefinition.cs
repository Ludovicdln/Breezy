using Breezy.Core.IO.Converter;
using Breezy.Core.IO.Definitions.Class;

namespace Breezy.Core.IO.Definitions.Properties;

public sealed class PropertyDbDefinition : IConverted
{
    public PropertyDbDefinition(ClassDbDefinition? classDbType, string type, string name, int ordinal, bool isForeignKey = false, bool isCollection = false, bool cannotBeRead = false)
        => (ClassDbType, Type, Name, Ordinal, IsForeignKey, IsCollection, CannotBeRead) = (classDbType, type, name, ordinal, isForeignKey, isCollection, cannotBeRead);
    
    public bool IsCollection { get; }
    
    public bool IsForeignKey { get; }
    
    public bool IsReference => !IsForeignKey && ClassDbType != null;
    
    public ClassDbDefinition? ClassDbType { get; }
    
    public string Type { get; }
    
    public string Name { get; }
    
    public int Ordinal { get; }
    
    public bool CannotBeRead { get; }

    public bool IsConvertible() => true;
}