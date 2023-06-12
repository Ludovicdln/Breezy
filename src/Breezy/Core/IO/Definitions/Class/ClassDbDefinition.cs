using Breezy.Core.IO.Converter;
using Breezy.Core.IO.Definitions.Properties;
using Breezy.Enums;
using Breezy.Extensions;

namespace Breezy.Core.IO.Definitions.Class;

public sealed class ClassDbDefinition : IConverted
{
    private List<RelationType>? _relationTypes;
    private string? _uniqueName;
    
    public ClassDbDefinition(int id, string type, string name, IEnumerable<PropertyDbDefinition> propertyDbDefinitions) 
        => (Id, Dependencies, Namespace, Type, Name, PropertyDbDefinitions) = (id, new List<string>(), string.Empty, type, name, propertyDbDefinitions);
    
    public ClassDbDefinition(int id, IList<string> dependencies, string nameSpace, string name, IEnumerable<PropertyDbDefinition> propertyDbDefinitions)
        => (Id, Dependencies, Namespace, Type, Name, PropertyDbDefinitions) = (id, dependencies, nameSpace, name, name, propertyDbDefinitions);
    
    public int Id { get; }
    
    public IList<string> Dependencies { get; }

    public string Namespace { get; }
    
    public string Type { get; }
    
    public string Name { get; }
    
    public string UniqueName => !string.IsNullOrEmpty(_uniqueName) ? _uniqueName : _uniqueName = $"{Type.ToCamelCase()}{Id}";
    
    public IEnumerable<PropertyDbDefinition> PropertyDbDefinitions { get; }
    
    public IEnumerable<RelationType> RelationTypes
    {
        get
        {
            if (_relationTypes != null) return _relationTypes;

            _relationTypes = new List<RelationType>();
            
            foreach (var propertyDbDefinition in PropertyDbDefinitions)
            {
                if (!propertyDbDefinition.IsForeignKey && !propertyDbDefinition.IsCollection) continue;

                var propertyLoop =
                    propertyDbDefinition.ClassDbType?.PropertyDbDefinitions.FirstOrDefault(x =>
                        x.Type.Equals(this.Type, StringComparison.OrdinalIgnoreCase));

                if (propertyLoop != null)
                {
                    _relationTypes.Add(propertyLoop.IsCollection && propertyDbDefinition.IsCollection
                        ? RelationType.MANY_TO_MANY
                        : RelationType.ONE_TO_MANY);
                    
                    continue;
                }
                
                _relationTypes.Add(propertyDbDefinition.IsCollection ? 
                    RelationType.ONE_TO_MANY : RelationType.ONE_TO_ONE);
            }

            var results = _relationTypes.Distinct();

            return results;
        }
    }
    
    public bool HasManyRelations(out RelationType? relationType)
    {
        relationType = RelationTypes.OrderByDescending(x => (int)x)
            .FirstOrDefault(x => x != RelationType.ONE_TO_ONE);
        
        return relationType != null;
    }

    public bool IsConvertible() => PropertyDbDefinitions.Any();
}