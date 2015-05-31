namespace BfCompiler.Core.Syntax {
    using Visitor;

    internal abstract class SyntaxNode {
        public SyntaxNode(NodePosition position) {
            this.Position = position;
        }

        public NodePosition Position { get; }

        public virtual void Accept(SyntaxNodeVisitor visitor) {
            visitor.Visit(this);
        }

        public override abstract string ToString();
    }
}