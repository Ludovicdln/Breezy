using Breezy.Core.IO.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Breezy;

public sealed class BreezySyntaxReceiver : ISyntaxReceiver
{
    public IList<ClassDeclarationSyntax> ClassDeclarationSyntaxes { get; }
	
    public BreezySyntaxReceiver() =>
        ClassDeclarationSyntaxes = new List<ClassDeclarationSyntax>();
    
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclarationSyntax)
            return;

        if (classDeclarationSyntax.AttributeLists.Count is 0)
            return;
		
        if (!classDeclarationSyntax.AttributeLists
                .Any(x => x.Attributes.Any(y => 
                    y.Name.ToString().Equals(TableAttribute.Name, StringComparison.OrdinalIgnoreCase) ||
                    y.Name.ToString().Equals(TableAttribute.ShortName, StringComparison.OrdinalIgnoreCase) || 
                    y.Name.ToString().Equals(TableAttribute.FullName, StringComparison.OrdinalIgnoreCase))))
            return;
		
        ClassDeclarationSyntaxes.Add(classDeclarationSyntax);
    }
}