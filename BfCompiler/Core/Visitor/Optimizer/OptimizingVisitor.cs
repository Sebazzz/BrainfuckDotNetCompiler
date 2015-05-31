namespace BfCompiler.Core.Visitor.Optimizer {
    using System;
    using CSharpSyntax;
    using Syntax;
    using SyntaxNode = global::BfCompiler.Core.Syntax.SyntaxNode;

    internal class OptimizingVisitor : CSharpCodeGenerationVisitor {
        private Action _flusher;
        private Type _lastNodeType;

        private readonly ValueOperationOptimizer _valueOperationOptimizer;

        public OptimizingVisitor(BlockSyntax blockSyntax) : base(blockSyntax) {
            this._valueOperationOptimizer = new ValueOperationOptimizer(blockSyntax);
        }

        public override SyntaxNodeVisitor BeginVisit(LoopNode node) {
            this.Flush(node);

            return base.BeginVisit(node);
        }

        public override void Visit(OutputOperation node) {
            this.Flush(node);

            base.Visit(node);
        }

        public override void Visit(PointerOperationNode node) {
            this.Flush(node);

            base.Visit(node);
        }

        public override void Visit(ReadOperation node) {
            this.Flush(node);

            base.Visit(node);
        }

        public override void Visit(ValueOperationNode node) {
            this.Flush(node);
            this._flusher = this._valueOperationOptimizer.Flush;

            this._valueOperationOptimizer.Node(node);
        }

        public override void EndVisit(LoopNode node) {
            this.ForceFlush();

            base.EndVisit(node);
        }

        private void Flush(SyntaxNode currentNode) {
            if (currentNode.GetType() != this._lastNodeType) {
                this.ForceFlush();
            }

            this._lastNodeType = currentNode.GetType();
        }

        private void ForceFlush() {
            this._flusher?.Invoke();
            this._flusher = null;
        }

        protected override SyntaxNodeVisitor CreateInnerVisitor(BlockSyntax block) {
            return new OptimizingVisitor(block);
        }
    }


    internal sealed class ValueOperationOptimizer {
        private readonly BlockSyntax _block;
        private int _offset;

        public ValueOperationOptimizer(BlockSyntax block) {
            this._block = block;
        }

        public void Node(ValueOperationNode node) {
            int diff = GetDiff(node.Operation);
            this._offset += diff;
        }

        private static int GetDiff(Operation operation) {
            switch (operation) {
                case Operation.Increase:
                    return 1;
                case Operation.Decrease:
                    return -1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }
        }

        public void Flush() {
            this._block.Statements.Add(
                Syntax.ExpressionStatement(
                    Syntax.BinaryExpression(
                        BinaryOperator.Equals,
                        CSharpCodeGenerationVisitor.CurrentCellValueMemberAccess(),
                        Syntax.CastExpression(
                            new PredefinedTypeSyntax() {Type=PredefinedType.Char},
                            Syntax.ParenthesizedExpression(
                                Syntax.BinaryExpression(GetOperator(this._offset),
                                CSharpCodeGenerationVisitor.CurrentCellValueMemberAccess(),
                                Syntax.LiteralExpression(Math.Abs(this._offset))
                            )))
                        )
                )
            );

            this._offset = 0;
        }

        private static BinaryOperator GetOperator(int offset) {
            return offset > 0 ? BinaryOperator.Plus : BinaryOperator.Minus;
        }
    }
}