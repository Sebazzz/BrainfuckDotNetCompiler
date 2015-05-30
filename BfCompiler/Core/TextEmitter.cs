namespace BfCompiler {
    using System.IO;
    using CSharpSyntax;
    using CSharpSyntax.Printer;

    internal static class TextEmitter {
        public static string CompileCompilationUnit(SyntaxNode syntaxNode) {
            StringWriter sw = new StringWriter();
            using (var printer = new SyntaxPrinter(new SyntaxWriter(sw))) {
                printer.Visit(syntaxNode);
            }

            string code =  sw.ToString();

            return code;
        }
    }
}