namespace BfCompiler.Core.Syntax {
    using System;

    internal class PointerOperationNode : SyntaxNode {
        public PointerOperationNode(NodePosition position, Operation operation) : base(position) {
            this.Operation = operation;
        }

        public Operation Operation { get; }

        public override string ToString() {
            switch (this.Operation) {
                case Operation.Increase:
                    return ">";
                case Operation.Decrease:
                    return "<";
                default:
                    return String.Empty;
            }
        }
    }
}