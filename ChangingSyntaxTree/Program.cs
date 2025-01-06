using ChangingSyntaxTree;
using Microsoft.CodeAnalysis.CSharp;

// lang=c#
const string code = """
                    public class Sample
                    {
                        public void Method();
                        internal void Method2();
                        
                        public class Nested
                        {
                            public void Method();
                            public int Method2();
                        }
                    }
                    """;

var syntaxTree = CSharpSyntaxTree.ParseText(code);
// syntaxTree = SyntaxFactory.ParseSyntaxTree(code);

var root = syntaxTree.GetRoot();

var rewriter = new MethodDeclarationReturnTypeRewriter();
var newRoot = rewriter.Visit(root);

var newSyntaxTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);

Console.WriteLine(newSyntaxTree);
