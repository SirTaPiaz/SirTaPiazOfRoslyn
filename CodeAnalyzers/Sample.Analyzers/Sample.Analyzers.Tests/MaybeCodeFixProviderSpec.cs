using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Sample.Fx;
using Xunit;

namespace Sample.Analyzers.Tests;

public sealed class MaybeCodeFixProviderSpec
{
    [Fact]
    public async Task When_MethodWithReturnTypeMaybe_ContainsThrow_Then_ReplaceThrowWithReturnNone()
    {
        //lang=c#
        const string code = """
                            using System;
                            
                                                        
                            public class Program
                            {
                                public Sample.Fx.Maybe<int> GetValue(string number)
                                {
                                    throw new InvalidOperationException("Could not parse the number");
                                }
                            }
                            """;

        //lang=c#
        const string fixedCode = """
                                 using System;
                                 
                                                             
                                 public class Program
                                 {
                                     public Sample.Fx.Maybe<int> GetValue(string number)
                                     {
                                         return Sample.Fx.Maybe.None;
                                     }
                                 }
                                 """;

        var expectedDiagnostic = CSharpCodeFixVerifier<MaybeSemanticAnalyzer, MaybeCodeFixProvider, DefaultVerifier>
            .Diagnostic()
            .WithLocation(8, 9)
            .WithMessage("Use Maybe.None instead of throw exception");

        var codeFixTest = new CSharpCodeFixTest<MaybeSemanticAnalyzer, MaybeCodeFixProvider, DefaultVerifier>
        {
            TestState =
            {
                Sources = { code },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(None).Assembly.Location),
                }
            },
            ExpectedDiagnostics = { expectedDiagnostic },
            FixedCode = fixedCode
        };

        await codeFixTest.RunAsync();
    }


    [Fact]
    public async Task When_Suggesting_CodeFix_Skip_The_Namespace_If_It_Is_Already_Present()
    {
        //lang=c#
        const string code = """
                            using System;
                            using Sample.Fx;
                                                        
                            public class Program
                            {
                                public Maybe<int> GetValue(string number)
                                {
                                    throw new InvalidOperationException("Could not parse the number");
                                }
                            }
                            """;

        //lang=c#
        const string fixedCode = """
                                 using System;
                                 using Sample.Fx;
                                                             
                                 public class Program
                                 {
                                     public Maybe<int> GetValue(string number)
                                     {
                                         return Maybe.None;
                                     }
                                 }
                                 """;

        var expectedDiagnostic = CSharpCodeFixVerifier<MaybeSemanticAnalyzer, MaybeCodeFixProvider, DefaultVerifier>
            .Diagnostic()
            .WithLocation(8, 9)
            .WithMessage("Use Maybe.None instead of throw exception");

        var codeFixTest = new CSharpCodeFixTest<MaybeSemanticAnalyzer, MaybeCodeFixProvider, DefaultVerifier>
        {
            TestState =
            {
                Sources = { code },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(None).Assembly.Location),
                }
            },
            ExpectedDiagnostics = { expectedDiagnostic },
            FixedCode = fixedCode
        };

        await codeFixTest.RunAsync();
    }
}
