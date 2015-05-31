namespace BfCompiler.Core.Visitor {
    using System;
    using CSharpSyntax;
    using Syntax;

    internal class CSharpCodeGenerationVisitor : SyntaxNodeVisitor {
        private readonly BlockSyntax _blockSyntax;

        public CSharpCodeGenerationVisitor(BlockSyntax blockSyntax) {
            this._blockSyntax = blockSyntax;
        }

        public override void Visit(OutputOperation node) {
            this._blockSyntax.Statements.Add(
                Syntax.ExpressionStatement(
                    Syntax.InvocationExpression(
                        Syntax.IdentifierName("System.Console.Write"),
                        Syntax.ArgumentList(
                            Syntax.Argument(CurrentCellValueMemberAccess())
                        )
                    )
                )
            );
        }

        public override SyntaxNodeVisitor BeginVisit(LoopNode node) {
            var @while = Syntax.WhileStatement(
                Syntax.BinaryExpression(
                    BinaryOperator.ExclamationEquals,
                    CurrentCellValueMemberAccess(),
                    Syntax.LiteralExpression(0))
                );

            var block = new BlockSyntax();
            @while.Statement = block;

            this._blockSyntax.Statements.Add(@while);

            return this.CreateInnerVisitor(block);
        }

        protected virtual SyntaxNodeVisitor CreateInnerVisitor(BlockSyntax block) {
            return new CSharpCodeGenerationVisitor(block);
        }

        public override void Visit(PointerOperationNode node) {
            switch (node.Operation) {
                case Operation.Increase:
                    this.CurrentCellAssignmentByPointerOperation(CompilationConstants.CellNextMethod);
                    break;
                case Operation.Decrease:
                    this.CurrentCellAssignmentByPointerOperation(CompilationConstants.CellPrevMethod);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private void CurrentCellAssignmentByPointerOperation(string method) {
            InvocationExpressionSyntax valueExpression =
                Syntax.InvocationExpression(
                    Syntax.IdentifierName(method),
                    Syntax.ArgumentList(
                        Syntax.Argument(Syntax.IdentifierName(CompilationConstants.CurrentCell)),
                        Syntax.Argument(Syntax.IdentifierName(CompilationConstants.StateList))
                        )
                    );

            this.CurrentCellAssignmentOperation(valueExpression);
        }

        private void CurrentCellAssignmentOperation(InvocationExpressionSyntax valueExpression) {
            this._blockSyntax.Statements.Add(
                Syntax.ExpressionStatement(
                    Syntax.BinaryExpression(
                        BinaryOperator.Equals,
                        Syntax.IdentifierName(CompilationConstants.CurrentCell),
                        valueExpression
                        )
                    )
                );
        }

        public override void Visit(ReadOperation node) {
            this._blockSyntax.Statements.Add(
                Syntax.ExpressionStatement(
                    Syntax.BinaryExpression(
                        BinaryOperator.Equals,
                        CurrentCellValueMemberAccess(),
                        Syntax.InvocationExpression(
                            Syntax.IdentifierName(CompilationConstants.ReadInputMethod),
                            Syntax.ArgumentList()
                            )
                        )
                    )
                );
        }

        private void CellAssignmentOperation(PostfixUnaryOperator @operator) {
            this._blockSyntax.Statements.Add(
                Syntax.ExpressionStatement(
                    Syntax.PostfixUnaryExpression(@operator,
                        CurrentCellValueMemberAccess()
                    )
                )
            );
        }

        internal static MemberAccessExpressionSyntax CurrentCellValueMemberAccess() {
            return Syntax.MemberAccessExpression(Syntax.IdentifierName(CompilationConstants.CurrentCell), "Value");
        }

        public override void Visit(ValueOperationNode node) {
            switch (node.Operation) {
                case Operation.Increase:
                    this.CellAssignmentOperation(PostfixUnaryOperator.PlusPlus);
                    break;
                case Operation.Decrease:
                    this.CellAssignmentOperation(PostfixUnaryOperator.MinusMinus);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}