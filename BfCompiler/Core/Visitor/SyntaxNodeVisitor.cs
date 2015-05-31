namespace BfCompiler.Core.Visitor {
    using System.Collections.Generic;
    using Syntax;

    internal abstract partial class SyntaxNodeVisitor {
        public void Visit(IEnumerable<SyntaxNode> nodes) {
            foreach (SyntaxNode syntaxNode in nodes) {
                this.Visit(syntaxNode);
            }
        }
    }
}