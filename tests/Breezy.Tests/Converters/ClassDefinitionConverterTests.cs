using System.Reflection;
using Breezy.Core.IO.Converter;
using Breezy.Core.IO.Converter.Types;
using Breezy.Core.IO.Definitions.Class;
using Breezy.Core.IO.Definitions.Properties;
using Breezy.Core.Pool;
using Breezy.Enums;
using Breezy.Extensions;
using Breezy.Tests.Models.Constraints;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Breezy.Tests.Converters;

public sealed class ClassDefinitionConverterTests
{
    private CSharpCompilation _cSharpCompilation;

    private IConverter<ClassDefinition, ClassDbDefinition> _classDefConverter;
    
    [SetUp]
    public void SetUp()
    {
        _classDefConverter = new ClassDefinitionConverter(new PropertyDefinitionConverter(new UniqueIdProvider()),
            new UniqueIdProvider());
        
        _cSharpCompilation = CSharpCompilation.Create("BreezyCompilation")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
    
    [Test]
    public void Convert_ShouldReturnClassDbDefinition()
    {
        #region Analyzer
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace Breezy.Tests.Models.Constraints;

            public class UserCopy
            {
                public int Id { get; set; }
                public string Firstname { get; set; }
                public string Lastname { get; set; }
                public DateTime Birthday { get; set; }
                public Gender Gender { get; set; }
                public bool IsMinor { get; set; }
                public List<CarUser> Cars { get; set; } = new ();
                public List<HouseUser> Houses { get; set; } = new ();
                
                public bool IsValid()
                {
                    return Id != 0 && !string.IsNullOrEmpty(Firstname) && !string.IsNullOrEmpty(Lastname) && Cars != null && Houses != null;
                }
            }
            ");
        
        var compilation = _cSharpCompilation.AddSyntaxTrees(syntaxTree);

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        #endregion
        
        var userTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName(typeof(UserCopy).FullName!);

        var classDefinition = userTypeSymbol.ToClassDefinition();

        var classDbDefinition = _classDefConverter.Convert(classDefinition);

        Assert.IsNotNull(classDbDefinition);
        
        Assert.AreEqual("UserCopy", classDbDefinition.Name);
        Assert.AreEqual("UserCopy", classDbDefinition.Type);
        Assert.IsTrue(classDbDefinition.PropertyDbDefinitions.Any());
    }

    [Test]
    public void Convert_ShouldIncludeManyToManyRelationType()
    {
        var houseUserDbDefinition = new ClassDbDefinition(1, "HouseUser", "HouseUser", new List<PropertyDbDefinition>()
        {
            new(null, "int", "Id", 0),
            new(null, "string", "Tag", 1),
        });
        
        var carUserDbDefinition = new ClassDbDefinition(1, "CarUser", "CarUser", new List<PropertyDbDefinition>()
        {
            new(null, "int", "Id", 0),
            new(null, "string", "Model", 1),
        });

        var userCopyDbDefinition = new ClassDbDefinition(1, "UserCopy", "UserCopy", new List<PropertyDbDefinition>()
        {
            new(null, "int", "Id", 0),
            new(null, "string", "FullName", 1),
            new(houseUserDbDefinition, "HouseUser", "Houses", 2, false, true),
            new(carUserDbDefinition, "CarUser", "Cars", 3, false, true),
        });
        
        var postDbDefinition = new ClassDbDefinition(1, "Post", "Post", new List<PropertyDbDefinition>()
        {
            new(null, "int", "Id", 0),
            new(null, "string", "Title", 1),
            
        });
        
        var tagDbDefinition = new ClassDbDefinition(2, "Tag", "Tag", new List<PropertyDbDefinition>()
        {
            new(null, "int", "Id", 0),
            new(null, "string", "Name", 1),
            new (postDbDefinition, "Post", "Posts", 2, false, true, true)
        });
        
        postDbDefinition = new ClassDbDefinition(3, "Post", "Post", 
                postDbDefinition.PropertyDbDefinitions.Append(new PropertyDbDefinition(tagDbDefinition, "Tag", "Tags", 2, false,
                true, true)));
        
        tagDbDefinition = new ClassDbDefinition(2, "Tag", "Tag", 
            tagDbDefinition.PropertyDbDefinitions.Append(new PropertyDbDefinition(postDbDefinition, "Post", "Posts", 2, false,
            true, true)));
        
        Assert.IsFalse(houseUserDbDefinition.RelationTypes.Any());
        Assert.IsFalse(carUserDbDefinition.RelationTypes.Any());
        Assert.IsTrue(userCopyDbDefinition.RelationTypes.Any(x => x == RelationType.ONE_TO_MANY));
        Assert.IsTrue(postDbDefinition.RelationTypes.Any(x => x == RelationType.MANY_TO_MANY));
        Assert.IsTrue(tagDbDefinition.RelationTypes.Any(x => x == RelationType.MANY_TO_MANY));

    }
}