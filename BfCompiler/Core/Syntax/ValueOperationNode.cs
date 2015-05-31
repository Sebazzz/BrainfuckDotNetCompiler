namespace BfCompiler.Core.Syntax {
    using System;
    using Visitor;

    internal class ValueOperationNode : SyntaxNode {
        public Operation Operation { get; }

        public ValueOperationNode(NodePosition position, Operation operation) : base(position) {
            this.Operation = operation;
        }

        public override string ToString() {
            switch (this.Operation) {
                case Operation.Increase:
                    return "+";
                case Operation.Decrease:
                    return "-";
                default:
                    return String.Empty;
            }
        }

        public override void Accept(SyntaxNodeVisitor visitor) {
            visitor.Visit(this);
        }
    }
}