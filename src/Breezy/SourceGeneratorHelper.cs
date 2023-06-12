using System.Text;
using Breezy.Core.Builder;
using Breezy.Core.IO.Converter;
using Breezy.Core.IO.Definitions.Class;
using Breezy.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Breezy;

public static class SourceGeneratorHelper
{
    public static string GenerateExtensionClass(IConverter<ClassDefinition, ClassDbDefinition> classConverter, in ClassDefinition classDefinition)
    {
        var dbTemplateBuilder = new DbTemplateBuilder(classConverter.Convert(classDefinition));

        return dbTemplateBuilder
            .GenerateQueryAsyncFunction()
            .GenerateQueryFirstAsyncFunction()
            .Build();
    }
    
    public static string GenerateExtensionDbConnectionClass(IConverter<ClassDefinition, ClassDbDefinition> classConverter, in ClassDefinition classDefinition)
    {
        var dbTemplateBuilder = new DbTemplateBuilder(classConverter.Convert(classDefinition));

        return dbTemplateBuilder
            .GenerateExecuteAsyncFunction()
            .Build();
    }

}