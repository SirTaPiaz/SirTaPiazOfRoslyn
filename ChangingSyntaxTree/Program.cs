using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// lang=c#
const string code = """
                    public class Sample
                    {
                        public void Method();
                        internal void Method2();
                        
                        public class Nested
                        {
                            public void Method();
                            public int Method2();
                        }
                    }
                    """;

var syntaxTree = CSharpSyntaxTree.ParseText(code);
// syntaxTree = SyntaxFactory.ParseSyntaxTree(code);

var root = syntaxTree.GetRoot();

var methods = root
    .DescendantNodes(_ => true)
    .OfType<MethodDeclarationSyntax>();

var newRoot = root
    .ReplaceNodes(methods, (original, _) =>
    {
        var returnType = original.ReturnType;

        if (returnType.GetFirstToken().Text != SyntaxFactory.Token(SyntaxKind.VoidKeyword).Text)
            return original;

        var identifierToken = SyntaxFactory.Identifier("Task")
            .WithTriviaFrom(returnType.GetFirstToken());
        // .WithTrailingTrivia(returnType.GetFirstToken().TrailingTrivia)
        // .WithLeadingTrivia(returnType.GetFirstToken().LeadingTrivia);
        var identifierNameSyntaxNode = SyntaxFactory.IdentifierName(identifierToken);

        return original.WithReturnType(identifierNameSyntaxNode);
    });

var newSyntaxTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);

Console.WriteLine(newSyntaxTree);
