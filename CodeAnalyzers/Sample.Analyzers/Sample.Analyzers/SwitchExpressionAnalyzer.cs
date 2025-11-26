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

    private readonly SuppressionDescriptor _rule = new(SuppressorId, DiagnosticId, Justification);

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(_rule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var reportedDiagnostics = context.ReportedDiagnostics;
        foreach (var reportedDiagnostic in reportedDiagnostics)
        {
            if (CanSkipReportedDiagnostic(context, reportedDiagnostic)) 
                continue;

            SuppressReportedDiagnostic(context, reportedDiagnostic);
        }
    }

    private void SuppressReportedDiagnostic(SuppressionAnalysisContext context, Diagnostic reportedDiagnostic)
    {
        var suppression = Suppression.Create(_rule, reportedDiagnostic);
        context.ReportSuppression(suppression);
    }

    private static bool CanSkipReportedDiagnostic(SuppressionAnalysisContext context, Diagnostic reportedDiagnostic)
    {
        var syntaxTree = reportedDiagnostic.Location.SourceTree;
        if (syntaxTree is null)
            return true;

        var semanticModel = context.GetSemanticModel(syntaxTree);

        var root = syntaxTree.GetRoot();

        if (root.FindNode(reportedDiagnostic.Location.SourceSpan) is not SwitchExpressionSyntax
            switchExpressionNode)
            return true;

        var switchExpressionArms = switchExpressionNode.Arms;
        if (switchExpressionArms.Count == 0)
            return true;

        if (root.FindNode(switchExpressionNode.GoverningExpression.Span) is not IdentifierNameSyntax identifier)
            return true;

        if (semanticModel.GetSymbolInfo(identifier).Symbol is not IParameterSymbol identifierSemanticModel)
            return true;

        if (identifierSemanticModel.Type is { IsAbstract: false })
            return true;

        if (ValidateSwitchExpressionsArms(context, identifierSemanticModel, switchExpressionArms, semanticModel))
            return true;
        return false;
    }

    private static bool ValidateSwitchExpressionsArms(SuppressionAnalysisContext context,
        IParameterSymbol identifierSemanticModel, SeparatedSyntaxList<SwitchExpressionArmSyntax> switchExpressionArms,
        SemanticModel semanticModel)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var derivedTypes = context.Compilation.SyntaxTrees
            .SelectMany(tree =>
            {
                var records = tree.GetRoot()
                        .DescendantNodes()
                        .OfType<RecordDeclarationSyntax>()
                        .Where(syntax => syntax.BaseList is not null && syntax.BaseList.Types.Count == 1
                                                                     && syntax.BaseList.Types.First().Type
                                                                         .ToFullString()
                                                                         .Contains(identifierSemanticModel.Type.Name)
                        )
                        .ToImmutableArray()
                    ;

                if (!records.Any())
                    return [];

                var model = context.GetSemanticModel(tree);
                return records.Select(record => model.GetDeclaredSymbol(record));
            })
            .Where(type => type!.BaseType!.Equals(identifierSemanticModel.Type, SymbolEqualityComparer.Default))
            .OrderBy(type => type!.Name)
            .ToImmutableArray();

        var handledTypes = switchExpressionArms
            .Select(arm => semanticModel.GetTypeInfo(arm.Pattern).ConvertedType)
            .OrderBy(type => type!.Name)
            .ToImmutableArray();

        return !derivedTypes.SequenceEqual(handledTypes, SymbolEqualityComparer.Default);
    }
}