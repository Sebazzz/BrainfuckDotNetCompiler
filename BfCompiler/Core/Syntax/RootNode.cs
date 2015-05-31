namespace BfCompiler.Core.Syntax {
    using System.Collections.ObjectModel;
    using System.Text;
    using Visitor;

    [CompositeNode]
    internal sealed class RootNode : SyntaxNode {
        public ReadOnlyCollection<SyntaxNode> Nodes { get; }

        public RootNode(ReadOnlyCollection<SyntaxNode> nodes) : base(default(NodePosition)) {
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
            StringBuilder sb = new StringBuilder(this.Nodes.Count);

            foreach (SyntaxNode node in this.Nodes) {
                sb.Append(node);
            }

            return sb.ToString();
        }
    }
}