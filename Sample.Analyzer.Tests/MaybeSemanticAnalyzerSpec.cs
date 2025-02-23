using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Sample.Analyzers;
using Sample.Fx;

namespace Sample.Analyzer.Tests;

public class MaybeSemanticAnalyzerSpec
{
    [Fact]
    public async Task When_MethodReturnsMaybe_Then_ReportDiagnostic()
    {
        const string code = """
                            using System;
                            using Sample.Fx;
                            
                            public class Program
                            {
                                public Maybe<string> Method()
                                {
                                    throw new InvalidOperationException();
                                }
                            }
                            """;

        var expectedDiagnostic = CSharpAnalyzerVerifier<MaybeSemanticAnalyzer, DefaultVerifier>
            .Diagnostic(MaybeSemanticAnalyzer.DiagnosticId)
            .WithLocation(8, 9);
        
        var anaylzerTest = new CSharpAnalyzerTest<MaybeSemanticAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { code },
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(None).Assembly.Location)
                },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90
            },
            ExpectedDiagnostics =
            {
                expectedDiagnostic
            }
        };

        await anaylzerTest.RunAsync();
        
    }
    [Fact]
    public async Task When_MethodReturnsMaybe_And_ThereIsNoThrow_Then_NoDiagnostic()
    {
        const string code = """
                            using System;
                            using Sample.Fx;
                            
                            public class Program
                            {
                                public Maybe<string> Method()
                                {
                                    return Maybe.None;
                                }
                            }
                            """;
        
        var anaylzerTest = new CSharpAnalyzerTest<MaybeSemanticAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { code },
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(None).Assembly.Location)
                },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90
            }
        };

        await anaylzerTest.RunAsync();
        
    }
}
