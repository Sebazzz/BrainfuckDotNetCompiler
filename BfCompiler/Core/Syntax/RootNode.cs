namespace BfCompiler.Core.Syntax {
    using System.Collections.ObjectModel;
    using System.Text;
    using Visitor;

    internal sealed class RootNode : SyntaxNode {
        public ReadOnlyCollection<SyntaxNode> Nodes { get; }

        public RootNode(ReadOnlyCollection<SyntaxNode> nodes) : base(default(NodePosition)) {
            this.Nodes = nodes;
        }

        public override void Accept(SyntaxNodeVisitor visitor) {
            foreach (SyntaxNode node in this.Nodes) {
                visitor.Visit(node);
            }
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