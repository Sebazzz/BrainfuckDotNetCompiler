namespace BfCompiler.Core.Syntax {
    using Visitor;

    internal class ReadOperation : SyntaxNode {
        public ReadOperation(NodePosition position) : base(position) {}

        public override string ToString() {
            return ",";
        }

        public override void Accept(SyntaxNodeVisitor visitor) {
            visitor.Visit(this);
        }
    }
}