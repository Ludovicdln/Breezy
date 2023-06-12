using Breezy.Core.IO.Definitions.Class;
using Breezy.Core.IO.Definitions.Properties;
using Breezy.Core.Pool;
using Breezy.Extensions;

namespace Breezy.Core.IO.Converter.Types;

public sealed class PropertyDefinitionConverter : IConverter<PropertyDefinition, PropertyDbDefinition>
{
    private readonly UniqueIdProvider _uniqueIdProvider;
    private ClassDefinition? _currentDefinition;

    private bool _skipForeignKey; 
    private int _lastOrdinal;

    public PropertyDefinitionConverter(UniqueIdProvider uniqueIdProvider) => _uniqueIdProvider = uniqueIdProvider;
    
    public PropertyDbDefinition Convert(PropertyDefinition input, object? param)
#pragma warning disable MA0025
        => throw new NotImplementedException();
#pragma warning restore MA0025
    
    public IEnumerable<PropertyDbDefinition> Convert(IEnumerable<PropertyDefinition> inputs, object? param = null)
    {
        var propertyDbDefinitions = new List<PropertyDbDefinition>();

        _skipForeignKey = false; _lastOrdinal = 0;

        _currentDefinition = (ClassDefinition)param! ?? throw new ArgumentException("param for currentDefinition isn't set !");

        if (!TryParse(inputs.ToList(), _currentDefinition.Properties.Count(x => x.IsForeignKey), propertyDbDefinitions) && !_currentDefinition.IsForced)
            throw new Exception($"can't convert properties of {_currentDefinition.Name}");

        return propertyDbDefinitions;
    }
    
    #region Treatments
    private bool TryParse(in IList<PropertyDefinition> properties, int foreignKeysCount, ICollection<PropertyDbDefinition> propertyDbDefinitions, 
        int index = 0, int ordinal = 0)
    {
        if (properties.Count <= 0) return false;
        
        if (index >= properties.Count)
            return true;
        
        if (_currentDefinition!.SplitOn.Any(x => x == ordinal))
            return TryParse(properties, foreignKeysCount, propertyDbDefinitions, index, ordinal + 1);

        var property = properties[index];

        if (string.Equals(property.Type, _currentDefinition.Name))
        {
            propertyDbDefinitions.Add(new PropertyDbDefinition(null, property.Type, property.Name,
                ordinal, property.IsForeignKey, property.IsCollection, true));
            
            _lastOrdinal = ordinal;
            
            return TryParse(properties, foreignKeysCount, propertyDbDefinitions, index + 1, ordinal + 1);
        }
        
        if (property is { IsForeignKey: true, IsCollection: false })
        {
            if (!_skipForeignKey && _currentDefinition.SplitOn.Length <= 0)
            {
                ordinal += foreignKeysCount;
                _skipForeignKey = true;
            }
            else
                ordinal += _lastOrdinal > ordinal ? ((_lastOrdinal - ordinal) + 1) : +1;

            ParseForeignKeyType(property, propertyDbDefinitions, foreignKeysCount, ordinal);
        }
        else
        {
            if (property.IsReferenceType || property.IsCollection)
            {
                ParseReferenceType(property, propertyDbDefinitions, foreignKeysCount, ordinal);
                
                ordinal += _lastOrdinal > ordinal ? (_lastOrdinal - ordinal) : +1;
            }
            else
            {
                _lastOrdinal = ordinal;

                propertyDbDefinitions.Add(new PropertyDbDefinition(null, property.Type, property.Name, ordinal));
            }
        }

        return TryParse(properties, foreignKeysCount, propertyDbDefinitions, index + 1, ordinal + 1);
    }

    private void ParseForeignKeyType(PropertyDefinition property, ICollection<PropertyDbDefinition> propertyDbDefinitions, int foreignKeysCount, int ordinal)
    {
        var foreignKeyProperties = property.TypeSymbol.ToPropertiesDefinition();

        var propertyForeignKeysCount = foreignKeyProperties.Count(x => x.IsForeignKey);

        if (propertyForeignKeysCount > 0)
            _skipForeignKey = false;

        var foreignProperties = new List<PropertyDbDefinition>();
            
        TryParse(foreignKeyProperties,
            propertyForeignKeysCount > 0 ? propertyForeignKeysCount : foreignKeysCount,
            foreignProperties, 0, ordinal);

        var foreignClassDbDef = new ClassDbDefinition(_uniqueIdProvider.Pop(), property.Type, property.Name, foreignProperties);
            
        propertyDbDefinitions.Add(new PropertyDbDefinition(foreignClassDbDef, property.Type, property.Name,
            -1, true));
    }

    private void ParseReferenceType(PropertyDefinition property, ICollection<PropertyDbDefinition> propertyDbDefinitions, int foreignKeysCount, int ordinal)
    {
        var referenceProperties = property.TypeSymbol.ToPropertiesDefinition();

        var refProperties = new List<PropertyDbDefinition>();
                
        TryParse(referenceProperties, foreignKeysCount, refProperties, 0, ordinal);

        var refClassDbDef = new ClassDbDefinition(_uniqueIdProvider.Pop(), property.Type, property.Name, refProperties);
                
        propertyDbDefinitions.Add(new PropertyDbDefinition(refClassDbDef, property.Type, property.Name,
            -1, property.IsForeignKey, property.IsCollection));
    }
    #endregion
}
