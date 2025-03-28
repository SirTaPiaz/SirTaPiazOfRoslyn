using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Sample.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MaybeSemanticAnalyzer : DiagnosticAnalyzer
{
    // Preferred format of DiagnosticId is Your Prefix + Number, e.g. CA1234.
    public const string DiagnosticId = "SHG001";

    // Feel free to use raw strings if you don't need localization.
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.SHG001Title),
        Resources.ResourceManager, typeof(Resources));

    // The message that will be displayed to the user.
    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Resources.SHG001MessageFormat), Resources.ResourceManager,
            typeof(Resources));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Resources.SHG001Description), Resources.ResourceManager,
            typeof(Resources));

    // The category of the diagnostic (Design, Naming etc.).
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description
        , customTags:
        [
            WellKnownDiagnosticTags.NotConfigurable,
            WellKnownDiagnosticTags.Compiler
        ]
    );

    // Keep in mind: you have to list your rules here.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // You must call this method to avoid analyzing generated code.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // You must call this method to enable the Concurrent Execution.
        context.EnableConcurrentExecution();

        // Subscribe to semantic (compile time) action invocation, e.g. throw .
        context.RegisterOperationAction(AnalyzeThrowStatements, OperationKind.Throw);
    }

    private static void AnalyzeThrowStatements(OperationAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        if (context.Operation is not IThrowOperation || context.Operation.Syntax is not ThrowStatementSyntax)
            return;

        if (context.Operation.SemanticModel is null)
            return;

        var containingMethodSyntax = GetContainingMethod(context.Operation.Syntax);
        var containingMethodSymbol =
            context.Operation.SemanticModel.GetDeclaredSymbol(containingMethodSyntax) as IMethodSymbol;

        var returnType = containingMethodSymbol?.ReturnType;
        var maybeType = context.Compilation.GetTypeByMetadataName("Sample.Fx.Maybe`1");

        if (!returnType!.OriginalDefinition.Equals(maybeType, SymbolEqualityComparer.Default))
            return;

        var diagnostic = Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static MethodDeclarationSyntax GetContainingMethod(SyntaxNode syntax)
    {
        while (true)
        {
            if (syntax.Parent is MethodDeclarationSyntax methodDeclarationSyntax)
                return methodDeclarationSyntax;
            syntax = syntax.Parent!;
        }
    }
}
