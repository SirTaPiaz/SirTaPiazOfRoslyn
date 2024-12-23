using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

const string sourceCode = """
                          using System;

                          namespace BuildingCode;

                          public class Program
                          {
                            static void Main(string[] args)
                            {
                                Console.WriteLine("Hello Sir Ta Piaz!");
                            }
                          }
                          """;

// 0. Reference required assemblies
var coreLib = typeof(object).Assembly;
var console = typeof(Console).Assembly;
var systemRuntime = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().FullName.Contains("System.Runtime"));

// 1. Create a Syntax Tree
var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceCode);

// 2. Create a Compilation
var compilation = CSharpCompilation.Create(
    assemblyName: "BuildingCode",
    options: new CSharpCompilationOptions(OutputKind.ConsoleApplication),
    syntaxTrees: [syntaxTree],
    references:
    [
        MetadataReference.CreateFromFile(coreLib.Location),
        MetadataReference.CreateFromFile(console.Location),
        MetadataReference.CreateFromFile(systemRuntime.Location)
    ]
);

// 3. Emit the Assembly
using var ms = new MemoryStream();
var result = compilation.Emit(ms);

if (result.Success)
{
    Console.ForegroundColor = ConsoleColor.Green;
    var assembly = Assembly.Load(ms.GetBuffer());
    assembly.EntryPoint?.Invoke(null, BindingFlags.Static | BindingFlags.Public, null, [null], null);
    Console.ResetColor();
}
else
{
    foreach (var diagnostic in result.Diagnostics)
    {
        Console.ForegroundColor = diagnostic.Severity == DiagnosticSeverity.Error
            ? ConsoleColor.Red
            : ConsoleColor.Yellow;
        Console.Error.WriteLine(diagnostic);
        Console.ResetColor();
    }
}
