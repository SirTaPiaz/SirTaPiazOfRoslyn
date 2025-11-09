using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Sample.Fx;
using Xunit;

namespace Sample.Analyzers.Tests;

public sealed class SwitchExpressionAnalyzerSpecs
{
    [Fact]
    public async Task When_all_cases_are_covered_Then_Dont_ReportDiagnostic()
    {
        const string code = """
                            #nullable enable

                            namespace Sample.Console;

                            internal sealed record Game
                            {
                                internal Game? Apply(GamesEvents @event)
                                {
                                    return @event {|#0:switch|}
                                    {
                                        GamesEvents.GameCreated created => this,
                                        GamesEvents.PlayerJoined playerJoined => this,
                                        GamesEvents.GameStarted => this,
                                        GamesEvents.GameEnded => this,
                                    };
                                }
                            }

                            internal abstract record GamesEvents
                            {
                                private GamesEvents()
                                {
                                }

                                internal record GameCreated() : GamesEvents;

                                internal record PlayerJoined() : GamesEvents;

                                internal record GameStarted() : GamesEvents;

                                internal record GameEnded() : GamesEvents;
                            }
                            """;

        var expectedDiagnostic = new DiagnosticResult("CS8509", DiagnosticSeverity.Warning)
                .WithLocation(0)
                .WithIsSuppressed(true)
            ;

        var testAnalyzer = new CSharpAnalyzerTest<SwitchExpressionAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { code },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences = { },
            },
            CompilerDiagnostics = CompilerDiagnostics.All,
            ExpectedDiagnostics = { expectedDiagnostic },
        };

        await testAnalyzer.RunAsync();
    }

    [Fact]
    public async Task When_none_of_cases_are_covered_Then_ReportDiagnostic()
    {
        const string code = """
                            using System;

                            #nullable enable

                            namespace Sample.Console;

                            internal sealed record Game
                            {
                                public Guid Id { get; init; }    
                                public Guid PlayerId { get; init; }    
                                internal Game? Apply(GamesEvents @event)
                                {
                                    return @event {|#0:switch|}
                                    {
                                    };
                                }
                            }

                            internal abstract record GamesEvents
                            {
                                private GamesEvents()
                                {
                                }

                                internal record GameCreated(Guid GameId) : GamesEvents;

                                internal record PlayerJoined(Guid GameId, Guid PlayerId) : GamesEvents;

                                internal record GameStarted(Guid GameId) : GamesEvents;

                                internal record GameEnded(Guid GameId) : GamesEvents;
                            }
                            """;


        var expectedDiagnostic = new DiagnosticResult("CS8509", DiagnosticSeverity.Warning)
            .WithLocation(0);

        var testAnalyzer = new CSharpAnalyzerTest<SwitchExpressionAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { code },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences = { },
                ExpectedDiagnostics = { },
            },
            CompilerDiagnostics = CompilerDiagnostics.All,
            ExpectedDiagnostics = { expectedDiagnostic }
        };

        await testAnalyzer.RunAsync();
    }

    [Fact]
    public async Task When_some_of_cases_are_covered_Then_ReportDiagnostic()
    {
        const string code = """
                            using System;

                            #nullable enable

                            namespace Sample.Console;

                            internal sealed record Game
                            {
                                public Guid Id { get; init; }    
                                public Guid PlayerId { get; init; }    
                                internal Game? Apply(GamesEvents @event)
                                {
                                    return @event {|#0:switch|}
                                    {
                                        GamesEvents.GameCreated created => new Game{ Id = created.GameId },
                                    };
                                }
                            }

                            internal abstract record GamesEvents
                            {
                                private GamesEvents()
                                {
                                }

                                internal record GameCreated(Guid GameId) : GamesEvents;

                                internal record PlayerJoined(Guid GameId, Guid PlayerId) : GamesEvents;

                                internal record GameStarted(Guid GameId) : GamesEvents;

                                internal record GameEnded(Guid GameId) : GamesEvents;
                            }
                            """;


        var expectedDiagnostic = new DiagnosticResult("CS8509", DiagnosticSeverity.Warning)
            .WithLocation(0);

        var testAnalyzer = new CSharpAnalyzerTest<SwitchExpressionAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { code },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences = { },
                ExpectedDiagnostics = { },
            },
            CompilerDiagnostics = CompilerDiagnostics.All,
            ExpectedDiagnostics = { expectedDiagnostic }
        };

        await testAnalyzer.RunAsync();
    }
    
    
    [Fact]
    public async Task When_two_different_expressions_exists_and_all_cases_are_covered_Then_Dont_ReportDiagnostic()
    {
        const string code = """
                            #nullable enable
                            
                            using System;

                            namespace Sample.Console;

                            internal sealed record Game
                            {
                                internal Game? Apply(GamesEvents @event)
                                {
                                    return @event {|#0:switch|}
                                    {
                                        GamesEvents.GameCreated created => this,
                                        GamesEvents.PlayerJoined playerJoined => this,
                                        GamesEvents.GameStarted => this,
                                        GamesEvents.GameEnded => this,
                                    };
                                }
                                
                                internal void Apply(CustomerEvents @event)
                                {
                                    var message = @event {|#1:switch|}
                                    {
                                        CustomerEvents.CustomerActivated activated => "Activated",
                                        CustomerEvents.CustomerRegistered customerRegistered => "Registered",
                                    };
                                
                                    System.Console.WriteLine(message);
                                }
                                
                            }

                            internal abstract record CustomerEvents
                            {
                                private CustomerEvents()
                                {
                                }
                            
                                internal record CustomerRegistered : CustomerEvents;
                            
                                internal record CustomerActivated : CustomerEvents;
                            }
                            
                            internal abstract record GamesEvents
                            {
                                private GamesEvents()
                                {
                                }
                            
                                internal record GameCreated(Guid GameId) : GamesEvents;
                            
                                internal record PlayerJoined(Guid GameId, Guid PlayerId) : GamesEvents;
                            
                                internal record GameStarted(Guid GameId) : GamesEvents;
                            
                                internal record GameEnded(Guid GameId) : GamesEvents;
                            }
                            """;

        var firstSuppressedDiagnostic = new DiagnosticResult("CS8509", DiagnosticSeverity.Warning)
                .WithLocation(0)
                .WithIsSuppressed(true);
        
        var secondSuppressedDiagnostic = new DiagnosticResult("CS8509", DiagnosticSeverity.Warning)
            .WithLocation(1)
            .WithIsSuppressed(true);

        var testAnalyzer = new CSharpAnalyzerTest<SwitchExpressionAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { code },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences = { },
            },
            CompilerDiagnostics = CompilerDiagnostics.All,
            ExpectedDiagnostics = { firstSuppressedDiagnostic, secondSuppressedDiagnostic },
        };

        await testAnalyzer.RunAsync();
    }
    
}