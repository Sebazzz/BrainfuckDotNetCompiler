namespace BfCompiler {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;

    internal class BinaryEmitter {
        public static byte[] Compile(string compilationUnit) {
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
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp3)
                    .WithDocumentationMode(DocumentationMode.None)
                    .WithKind(SourceCodeKind.Regular);

            SyntaxTree text = CSharpSyntaxTree.ParseText(compilationUnit, parseOptions);
            SyntaxTree[] input = new[] {text};

            MetadataReference[] @ref = {
                MetadataReference.CreateFromAssembly(typeof (Console).Assembly),
                MetadataReference.CreateFromAssembly(typeof (LinkedList<>).Assembly)
            };

            CSharpCompilationOptions compileOptions = new CSharpCompilationOptions(
                OutputKind.ConsoleApplication,
                mainTypeName: CompilationConstants.FullyQualifiedMainTypeName,
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
    }
}