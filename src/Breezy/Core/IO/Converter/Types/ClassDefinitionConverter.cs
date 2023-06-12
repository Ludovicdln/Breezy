using Breezy.Core.IO.Definitions.Class;
using Breezy.Core.IO.Definitions.Properties;
using Breezy.Core.Pool;

namespace Breezy.Core.IO.Converter.Types;

public class ClassDefinitionConverter : IConverter<ClassDefinition, ClassDbDefinition>
{
    private readonly IConverter<PropertyDefinition, PropertyDbDefinition> _propertyConverter;
    private readonly UniqueIdProvider _uniqueIdProvider;

    public ClassDefinitionConverter(IConverter<PropertyDefinition, PropertyDbDefinition> propertyConverter, UniqueIdProvider uniqueIdProvider) =>
        (_propertyConverter, _uniqueIdProvider) = (propertyConverter, uniqueIdProvider);
    
    public ClassDbDefinition Convert(ClassDefinition input, object? param = null)
    {
        if (!input.IsConvertible()) throw new ArgumentException($"{input.Name} is not convertible", paramName: nameof(input));

        var propertiesDbDefinitions = _propertyConverter.Convert(input.Properties, input);

        return new ClassDbDefinition(_uniqueIdProvider.Pop(), input.Dependencies, input.Namespace, input.Name, propertiesDbDefinitions);
    }

    public IEnumerable<ClassDbDefinition> Convert(IEnumerable<ClassDefinition> inputs, object? param) 
        => inputs.Select(x => Convert(x, param));
}