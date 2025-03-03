using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Sample.Fx;
using Xunit;

namespace Sample.Analyzers.Tests;

public sealed class MaybeSemanticAnalyzerSpecs
{
    [Fact]
    public async Task When_MethodWithReturnTypeMaybe_ContainsThrow_Then_ReportDiagnostic()
    {
        const string code = """
                            using System;
                            using Sample.Fx;
                                                        
                            public class Program
                            {
                                public Maybe<int> GetValue(string number)
                                {
                                    {|#0:throw new InvalidOperationException("Could not parse the number");|}
                                }
                            }
                            """;

        var expectedDiagnostic = CSharpAnalyzerVerifier<MaybeSemanticAnalyzer, DefaultVerifier>
            .Diagnostic(MaybeSemanticAnalyzer.DiagnosticId)
            .WithLocation(0);

        var testAnalyzer = new CSharpAnalyzerTest<MaybeSemanticAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { code },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences = { MetadataReference.CreateFromFile(typeof(None).Assembly.Location) },
            },
            ExpectedDiagnostics = { expectedDiagnostic }
        };

        await testAnalyzer.RunAsync();
    }

    [Fact]
    public async Task When_MethodWithReturnTypeNotMaybe_ContainsThrow_Then_ReportNoDiagnostic()
    {
        //lang=c#
        const string code = """
                            using System;
                            using Sample.Fx;
                                                        
                            public class Program
                            {
                                public int GetValue(string number)
                                {
                                    throw new InvalidOperationException("Could not parse the number");
                                }
                            }
                            """;

        var testAnalyzer = new CSharpAnalyzerTest<MaybeSemanticAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { code },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences = { MetadataReference.CreateFromFile(typeof(None).Assembly.Location) },
            },
        };

        await testAnalyzer.RunAsync();
    }
}
