using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Sample.Analyzers;

/// <summary>
/// A sample code fix provider that renames classes with the company name in their definition.
/// All code fixes must  be linked to specific analyzers.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MaybeCodeFixProvider)), Shared]
public class MaybeCodeFixProvider : CodeFixProvider
{
    // Specify the diagnostic IDs of analyzers that are expected to be linked.
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
        [MaybeSemanticAnalyzer.DiagnosticId];

    // If you don't need the 'fix all' behaviour, return null.
    public override FixAllProvider? GetFixAllProvider() => null;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // We link only one diagnostic and assume there is only one diagnostic in the context.
        var diagnostic = context.Diagnostics.Single();

        // 'SourceSpan' of 'Location' is the highlighted area. We're going to use this area to find the 'SyntaxNode' to replace.
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Get the root of Syntax Tree that contains the highlighted diagnostic.
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        // Find SyntaxNode corresponding to the diagnostic.
        var diagnosticNode = root?.FindNode(diagnosticSpan);

        // To get the required metadata, we should match the Node to the specific type: 'ThrowStatementSyntax'.
        if (diagnosticNode is not ThrowStatementSyntax throwStatementSyntax)
            return;

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: string.Format(Resources.SHG001CodeFixTitle, "Maybe.None", "throw"),
                token => ReplaceThrowWithReturnStatement(context.Document, throwStatementSyntax, token),
                equivalenceKey: nameof(Resources.SHG001CodeFixTitle)),
            diagnostic
        );
    }

    private static async Task<Document> ReplaceThrowWithReturnStatement(
        Document document, CSharpSyntaxNode throwSyntaxNode, CancellationToken cancellationToken)
    {
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);
        var usingDirectives = syntaxTree!.GetCompilationUnitRoot().DescendantNodes().OfType<UsingDirectiveSyntax>();
        var namespaceAlreadyImported = usingDirectives.Any(u => u.ToFullString().Contains("using Sample.Fx;"));

        var classWithNamespace =
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Sample"),
                        IdentifierName("Fx")),
                    IdentifierName("Maybe")), IdentifierName("None"));

        var justClass = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression, IdentifierName("Maybe"), IdentifierName("None"));

        var returnStatement = ReturnStatement(
            namespaceAlreadyImported
                ? justClass
                : classWithNamespace
                    .WithTriviaFrom(throwSyntaxNode)
                    .NormalizeWhitespace());

        var root = await document.GetSyntaxRootAsync(cancellationToken);
        var newRoot = root?.ReplaceNode(throwSyntaxNode, returnStatement);

        var formattedRoot = Formatter.Format(newRoot!, Formatter.Annotation, document.Project.Solution.Workspace);
        return document.WithSyntaxRoot(formattedRoot);
    }
}
