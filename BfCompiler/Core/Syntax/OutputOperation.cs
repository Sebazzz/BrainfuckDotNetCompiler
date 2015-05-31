namespace BfCompiler.Core.Syntax {
    internal class OutputOperation : SyntaxNode {
        public OutputOperation(NodePosition position) : base(position) {}
        public override string ToString() {
            return ".";
        }
    }
}