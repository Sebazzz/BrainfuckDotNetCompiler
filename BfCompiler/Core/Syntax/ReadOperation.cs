namespace BfCompiler.Core.Syntax {
    internal class ReadOperation : SyntaxNode {
        public ReadOperation(NodePosition position) : base(position) {}

        public override string ToString() {
            return ",";
        }
    }
}