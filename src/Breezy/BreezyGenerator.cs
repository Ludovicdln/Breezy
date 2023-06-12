using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Breezy.Core.Builder;
using Breezy.Core.Caching;
using Breezy.Core.IO.Attributes;
using Breezy.Core.IO.Converter.Types;
using Breezy.Core.IO.Definitions.Class;
using Breezy.Core.IO.Definitions.Properties;
using Breezy.Core.Pool;
using Breezy.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Breezy
{
    [Generator]
    internal sealed class BreezyGenerator : IIncrementalGenerator
    {
        public const string SourceGeneratorAssemblyName = "Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.SourceAssemblySymbol";
        public const string Namespace = "Breezy";
        
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource($"{TableAttribute.Name}.g.cs", TableAttribute.Content);
                ctx.AddSource($"{SplitOnAttribute.Name}.g.cs", SplitOnAttribute.Content);
                ctx.AddSource($"{CacheableInterface.Name}.g.cs", CacheableInterface.Content);
                ctx.AddSource($"{IdentityQueryClass.Name}.g.cs", IdentityQueryClass.Content);
            });
            
            var provider = context.SyntaxProvider.CreateSyntaxProvider(predicate: (c, _) => c is ClassDeclarationSyntax
                    {
                        AttributeLists.Count: > 0
                    },
                transform: (n, _) => (ClassDeclarationSyntax)n.Node)
                .Where(m => m is not null && m.AttributeLists.Any(x => x.Attributes.Any(y => 
                    y.Name.ToString().Equals(TableAttribute.Name, StringComparison.OrdinalIgnoreCase) ||
                    y.Name.ToString().Equals(TableAttribute.ShortName, StringComparison.OrdinalIgnoreCase) || 
                    y.Name.ToString().Equals(TableAttribute.FullName, StringComparison.OrdinalIgnoreCase))));
            
            var compilation = context.CompilationProvider.Combine(provider.Collect());
            
            context.RegisterSourceOutput(compilation, (spc, source) => Execute(spc, source.Left, source.Right));
            
        }

        private void Execute(SourceProductionContext context, Compilation compilation,
            ImmutableArray<ClassDeclarationSyntax> typeList)
        {

            // https://stackoverflow.com/questions/681500/c-sharp-to-format-indent-align-c-sharp-properly
            string ArrangeUsingRoslyn(string csCode) {
                var tree = CSharpSyntaxTree.ParseText(csCode, cancellationToken: context.CancellationToken);
                var root = tree.GetRoot(context.CancellationToken).NormalizeWhitespace();
                return root.ToFullString();
            }
            
            var classDefinitions = new List<ClassDefinition>();
            
            foreach (var classDeclarationSyntax in typeList)
            {
                var model = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
			
                if (ModelExtensions.GetDeclaredSymbol(model, classDeclarationSyntax, context.CancellationToken) is not INamedTypeSymbol classSymbol)
                    continue;
                
                classDefinitions.Add(classSymbol.ToClassDefinition());
            }
            
            if (classDefinitions.Count is 0) return;

            UniqueIdProvider uniqueIdProvider;
            
            var propertyConverter = new PropertyDefinitionConverter(uniqueIdProvider = new UniqueIdProvider());
            
            var classConverter = new ClassDefinitionConverter(propertyConverter, uniqueIdProvider);

            foreach (var classDefinition in from classDefinition in classDefinitions 
                     let sb = new StringBuilder() select classDefinition)
            {
                context.AddSource($"{classDefinition.Name}Extensions.g.cs", 
                    ArrangeUsingRoslyn(SourceGeneratorHelper.GenerateExtensionClass(classConverter, in classDefinition)));
            }
            
            var dbConnectionClassDefinition = new ClassDefinition(new List<string>(),
                "DbConnectionExtensions", "DbConnection", false);
            
            context.AddSource($"{dbConnectionClassDefinition.Name}Extensions.g.cs", 
                ArrangeUsingRoslyn(SourceGeneratorHelper.GenerateExtensionDbConnectionClass(classConverter, in dbConnectionClassDefinition)));
        }
    }
}