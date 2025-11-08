using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sample.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp), Shared]
public class SwitchExpressionAnalyzer : DiagnosticSuppressor
{
    private const string SuppressorId = "SP8509";
    private const string DiagnosticId = "CS8509";

    private static readonly LocalizableString Justification =
        new LocalizableResourceString(nameof(Resources.SP8509SuppressorJustification),
            Resources.ResourceManager,
            typeof(Resources));

    private readonly SuppressionDescriptor _rule = new (SuppressorId, DiagnosticId, Justification);
    
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => [_rule];

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        var reportedDiagnostic = context.ReportedDiagnostics.First(x => x.Id == DiagnosticId);
        var syntaxTree = reportedDiagnostic.Location.SourceTree;
        if (syntaxTree is null)
            return;

        var semanticModel = context.GetSemanticModel(syntaxTree);

        var root = syntaxTree.GetRoot();

        if (root.FindNode(reportedDiagnostic.Location.SourceSpan) is not SwitchExpressionSyntax switchExpressionNode)
            return;

        var switchExpressionArms = switchExpressionNode.Arms;
        if (switchExpressionArms.Count == 0)
            return;

        if (root.FindNode(switchExpressionNode.GoverningExpression.Span) is not IdentifierNameSyntax identifier)
            return;

        context.CancellationToken.ThrowIfCancellationRequested();

        if (semanticModel.GetSymbolInfo(identifier).Symbol is not IParameterSymbol identifierSemanticModel)
            return;

        if (identifierSemanticModel.Type is { IsAbstract: false })
            return;

        var derivedTypes = context.Compilation.SyntaxTrees
            .SelectMany(tree =>
            {
                var records = tree.GetRoot()
                        .DescendantNodes()
                        .OfType<RecordDeclarationSyntax>()
                        .ToImmutableArray()
                    ;

                if (!records.Any())
                    return [];

                var model = context.GetSemanticModel(tree);
                return records.Select(record => model.GetDeclaredSymbol(record));
            })
            .Where(type => type?.BaseType != null)
            .Where(type => type!.BaseType!.Equals(identifierSemanticModel.Type, SymbolEqualityComparer.Default))
            .OrderBy(type => type!.Name)
            .ToList();

        var handledTypes = switchExpressionArms
            .Select(arm => semanticModel.GetTypeInfo(arm.Pattern).ConvertedType)
            .Where(type => type is not null)
            .OrderBy(type => type!.Name)
            .ToList();

        if (!derivedTypes.SequenceEqual(handledTypes, SymbolEqualityComparer.Default))
            return;
        
        var suppression = Suppression.Create(_rule, reportedDiagnostic);
        context.ReportSuppression(suppression);
    }
}