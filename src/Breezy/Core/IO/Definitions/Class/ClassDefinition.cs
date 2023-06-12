using System.Text;
using Breezy.Core.IO.Converter;
using Breezy.Core.IO.Definitions.Properties;

namespace Breezy.Core.IO.Definitions.Class;

public sealed class ClassDefinition : IConverted
{
    public ClassDefinition(IList<string> dependencies, string nameSpace, string name, bool isSealed, IList<PropertyDefinition> properties, int[] splitOn)
        => (Dependencies, Namespace, Name, IsSealed, Properties, SplitOn) = (dependencies, nameSpace, name, isSealed, properties, splitOn);
    
    public ClassDefinition(IList<string> dependencies, string nameSpace, string name, bool isSealed)
        => (Dependencies, Namespace, Name, IsSealed, Properties, SplitOn, IsForced) = (dependencies, nameSpace, name, isSealed, new List<PropertyDefinition>(), Array.Empty<int>(), true);
    
    public IList<string> Dependencies { get; }

    public string Namespace { get; }
	
    public string Name { get; }
        
    public bool IsSealed { get; }
	
    public IList<PropertyDefinition> Properties { get; }
    
    public int[] SplitOn { get; }
    
    public bool IsForced { get; }

    public bool IsConvertible() => Properties.Any() || IsForced;

}