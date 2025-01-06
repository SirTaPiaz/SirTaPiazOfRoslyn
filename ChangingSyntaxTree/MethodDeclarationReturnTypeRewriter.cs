using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChangingSyntaxTree;

internal sealed class MethodDeclarationReturnTypeRewriter
    : CSharpSyntaxRewriter
{
    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax original)
    {
        var returnType = original.ReturnType;

        if (returnType.GetFirstToken().Text != SyntaxFactory.Token(SyntaxKind.VoidKeyword).Text)
            return original;

        var identifierToken = SyntaxFactory.Identifier("Task")
            .WithTriviaFrom(returnType.GetFirstToken());

        var identifierNameSyntaxNode = SyntaxFactory.IdentifierName(identifierToken);

        return original.WithReturnType(identifierNameSyntaxNode);
    }
}
