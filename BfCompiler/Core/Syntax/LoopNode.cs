namespace BfCompiler.Core.Syntax {
    using System.Collections.ObjectModel;
    using System.Text;
    using Visitor;

    [CompositeNode]
    internal sealed class LoopNode : SyntaxNode {
        public ReadOnlyCollection<SyntaxNode> Nodes { get; }

        public LoopNode(NodePosition position, ReadOnlyCollection<SyntaxNode> nodes) : base(position) {
            this.Nodes = nodes;
        }

        public override void Accept(SyntaxNodeVisitor visitor) {
            SyntaxNodeVisitor innerVisitor = visitor.BeginVisit(this) ?? visitor; 
            foreach (SyntaxNode node in this.Nodes) {
                node.Accept(innerVisitor);
            }
            if (innerVisitor != visitor) innerVisitor.EndVisit(this);
            visitor.EndVisit(this);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder(this.Nodes.Count + 2);

            sb.Append('[');
            foreach (SyntaxNode node in this.Nodes) {
                sb.Append(node);
            }
            sb.Append(']');

            return sb.ToString();
        }
    }
}