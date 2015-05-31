namespace BfCompiler.Core.Syntax {
    using Visitor;

    internal class OutputOperation : SyntaxNode {
        public OutputOperation(NodePosition position) : base(position) {}
        public override string ToString() {
            return ".";
        }
        public override void Accept(SyntaxNodeVisitor visitor) {
            visitor.Visit(this);
        }
    }
}