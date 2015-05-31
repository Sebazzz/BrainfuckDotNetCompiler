namespace BfCompiler.Core.Syntax {
    using Visitor;

    internal abstract class SyntaxNode {
        public SyntaxNode(NodePosition position) {
            this.Position = position;
        }

        public NodePosition Position { get; }

        public abstract void Accept(SyntaxNodeVisitor visitor);

        public override abstract string ToString();
    }
}