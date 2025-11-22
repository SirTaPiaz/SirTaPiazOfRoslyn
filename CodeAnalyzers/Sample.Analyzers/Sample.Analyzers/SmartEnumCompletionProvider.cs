using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Tags;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Analyzers;

[ExportCompletionProvider(nameof(SmartEnumCompletionProvider), LanguageNames.CSharp)]
[Shared]
public class SmartEnumCompletionProvider : CompletionProvider
{
    public override Task ProvideCompletionsAsync(CompletionContext context)
    {
        ImmutableDictionary<string, string> properties = ImmutableDictionary.Create<string, string>();
        ImmutableArray<string> tags = [WellKnownTags.Module, WellKnownTags.Class, WellKnownTags.EnumMember];

        ImmutableArray<CharacterSetModificationRule> filterCharacterRules = default;
        ImmutableArray<CharacterSetModificationRule> commitCharacterRules = default;
        CompletionItemRules rules = CompletionItemRules.Create(
            filterCharacterRules,
            commitCharacterRules,
            EnterKeyRule.Always,
            false,
            1,
            CompletionItemSelectionBehavior.HardSelection);

        var completionItem = CompletionItem.Create(
            "Aaa", "Aaa", "Aaa", properties, tags, rules);
        context.AddItem(completionItem);

        context.SuggestionModeItem = completionItem;

        return Task.CompletedTask;
    }

    public override bool ShouldTriggerCompletion(SourceText text, int caretPosition, CompletionTrigger trigger,
        OptionSet options)
    {
        return false;
    }

    public override Task<CompletionDescription?> GetDescriptionAsync(Document document, CompletionItem item, CancellationToken cancellationToken)
    {
        var description = CompletionDescription.FromText("This is our beautiful description.");

        return Task.FromResult(description);
    }

    public override Task<CompletionChange> GetChangeAsync(Document document, CompletionItem item, char? commitKey, CancellationToken cancellationToken)
    {
        return base.GetChangeAsync(document, item, commitKey, cancellationToken);
    }
}
