namespace BfCompiler {
    using System;
    using System.IO;
    using System.Text;
    using CSharpSyntax;
    using CSharpSyntax.Printer;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using Syntax = CSharpSyntax.Syntax;

    public static class BfCompiler {
        public static byte[] Compile(string input) {
            return new BfCompilation(input).Compile();
        }
    }


    internal class BfCompilation {
        private const string MainTypeName = "_BfCompiledProgram";

        private readonly string _input;
        private int _pointer;

        private ClassDeclarationSyntax _clazz;
        private MethodDeclarationSyntax _mainMethod;

        private char Current => this._input[this._pointer];

        public BfCompilation(string input) {
            this._input = input;
        }

        public byte[] Compile() {
            this._pointer = 0;
            this._clazz = Syntax.ClassDeclaration(MainTypeName);
            this._mainMethod = new MethodDeclarationSyntax {
                Identifier = "Main",
                ReturnType = new PredefinedTypeSyntax {
                    Type = PredefinedType.Void
                },
                Modifiers = Modifiers.Static,
                Body = new BlockSyntax() {
                    Statements = {
                        new ExpressionStatementSyntax() {
                            Expression = Syntax.InvocationExpression(
                                Syntax.IdentifierName("System.Console.WriteLine"),
                                Syntax.ArgumentList(
                                    Syntax.Argument(Syntax.LiteralExpression("Hello"))
                                )
                            )
                        }
                    }
                },
                ParameterList = new ParameterListSyntax()
            };

            this._clazz.Members.Add(this._mainMethod);

            return this.CoreCompile();
        }

        private byte[] CoreCompile() {
            string compilationUnit = this.CompileCompilationUnit();

            return Compile(compilationUnit);
        }

        private static byte[] Compile(string compilationUnit) {
            CSharpCompilation compilation = SetupCompilation(compilationUnit);

            using (MemoryStream ms = new MemoryStream()) {
                EmitResult result = compilation.Emit(
                    ms,
                    options: new EmitOptions(subsystemVersion: SubsystemVersion.Windows8)
                );

                if (!result.Success) {
                    throw CreateCompilationException(result);
                }

                return ms.ToArray();
            }
        }

        private static CSharpCompilation SetupCompilation(string compilationUnit) {
            CSharpParseOptions parseOptions =
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp1)
                    .WithDocumentationMode(DocumentationMode.None)
                    .WithKind(SourceCodeKind.Regular);

            SyntaxTree text = CSharpSyntaxTree.ParseText(compilationUnit, parseOptions);
            SyntaxTree[] input = new[] {text};
            MetadataReference[] @ref = new[] {MetadataReference.CreateFromAssembly(typeof (Console).Assembly)};
            CSharpCompilationOptions compileOptions = new CSharpCompilationOptions(
                OutputKind.ConsoleApplication,
                mainTypeName: MainTypeName,
                optimizationLevel: OptimizationLevel.Release);

            CSharpCompilation compilation = CSharpCompilation.Create("BfCompilation", input, @ref, compileOptions);
            return compilation;
        }

        private static Exception CreateCompilationException(EmitResult result) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Internal compilation error.");

            foreach (Diagnostic diagnostic in result.Diagnostics) {
                sb.AppendLine(diagnostic.GetMessage());
            }

            throw new BfCompilationException(sb.ToString(), -1, -1);
        }

        private string CompileCompilationUnit() {
            StringWriter sw = new StringWriter();
            using (var printer = new SyntaxPrinter(new SyntaxWriter(sw))) {
                printer.Visit(this._clazz);
            }

            string code =  sw.ToString();

            return code;
        }
    }
}