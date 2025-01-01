using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CreatingSyntaxTrees.ConsoleExtensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

var compilationUnit = CompilationUnit()
    .WithMembers(
        SingletonList<MemberDeclarationSyntax>(
            FileScopedNamespaceDeclaration(
                    QualifiedName(
                        IdentifierName("CreatingSyntaxTrees"),
                        IdentifierName("Extensions")))
                .WithUsings(
                    SingletonList(
                        UsingDirective(
                            QualifiedName(
                                IdentifierName("System"),
                                IdentifierName("Linq")))))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        ClassDeclaration("MyStringExtensions")
                            .WithModifiers(
                                TokenList(
                                    new[]
                                    {
                                        Token(SyntaxKind.PublicKeyword),
                                        Token(SyntaxKind.StaticKeyword)
                                    }))
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(
                                    MethodDeclaration(
                                            PredefinedType(
                                                Token(SyntaxKind.StringKeyword)),
                                            Identifier("MyReverse"))
                                        .WithModifiers(
                                            TokenList(
                                                new[]
                                                {
                                                    Token(SyntaxKind.PublicKeyword),
                                                    Token(SyntaxKind.StaticKeyword)
                                                }))
                                        .WithParameterList(
                                            ParameterList(
                                                SingletonSeparatedList<ParameterSyntax>(
                                                    Parameter(
                                                            Identifier("str"))
                                                        .WithType(
                                                            PredefinedType(
                                                                Token(SyntaxKind.StringKeyword))))))
                                        .WithBody(
                                            Block(
                                                SingletonList<StatementSyntax>(
                                                    ReturnStatement(
                                                        InvocationExpression(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    PredefinedType(
                                                                        Token(SyntaxKind.StringKeyword)),
                                                                    IdentifierName("Concat")))
                                                            .WithArgumentList(
                                                                ArgumentList(
                                                                    SingletonSeparatedList<ArgumentSyntax>(
                                                                        Argument(
                                                                            InvocationExpression(
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind
                                                                                        .SimpleMemberAccessExpression,
                                                                                    IdentifierName("str"),
                                                                                    IdentifierName(
                                                                                        "Reverse")))))))))))))))))
    .NormalizeWhitespace();

var runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "System.Runtime");

var syntaxTree = CSharpSyntaxTree.Create(compilationUnit);

var compilation = CSharpCompilation.Create(
    assemblyName: "CreatingSyntaxTrees",
    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
    syntaxTrees: [syntaxTree],
    references:
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        MetadataReference.CreateFromFile(runtimeAssembly.Location)
    ]
);

using var ms = new MemoryStream();
var result = compilation.Emit(ms);

if (result.Success)
{
    var assembly = Assembly.Load(ms.GetBuffer());
    var myStringExtensions = assembly.GetType("CreatingSyntaxTrees.Extensions.MyStringExtensions")!;
    var myReverse = myStringExtensions.GetMethod("MyReverse")!;
    var reversedString = myReverse.Invoke(null, ["!zaiP aT riS olleH"])!;
    WriteLine(reversedString, ConsoleColor.Green);
}
else
{
    foreach (var diagnostic in result.Diagnostics)
    {
        WriteLine(diagnostic, diagnostic.Severity == DiagnosticSeverity.Error ? ConsoleColor.Red : ConsoleColor.Yellow);
    }
}
