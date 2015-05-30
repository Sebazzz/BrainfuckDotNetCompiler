namespace BfCompiler {
    using System;
    using System.Diagnostics;
    using CSharpSyntax;

    internal class SyntaxEmitter {
        private readonly StatementBlockManager _blockManager;

        public SyntaxEmitter(StatementBlockManager blockManager) {
            this._blockManager = blockManager;
        }

        private int _loopStart;
        private int _charsProcessed;
        public void GenerateCodeForCodePoint(char codePoint) {
            switch (codePoint) {
                case '+':
                    this.IncreaseCurrentCell();
                    break;

                case '-':
                    this.DecreaseCurrentCell();
                    break;

                case '[':
                    this.StartLoop();
                    break;

                case ']':
                    this.EndLoop();
                    break;

                case '>':
                    this.AdvanceCellPointer();
                    break;

                case '<':
                    this.RetreatCellPointer();
                    break;

                case '.':
                    this.OutputCurrentCell();
                    break;

                case ',':
                    this.ReadInputIntoCurrentCell();
                    break;

                default:
                    //throw SyntaxError(codePoint);
                    Debug.WriteLine("Ignoring character: " + codePoint);
                    break;
            }

            this._charsProcessed++;
        }

        private void EndLoop() {
            try {
                this._blockManager.PopBlock();
            }
            catch (InvalidOperationException) {
                throw new BfCompilationException("Unexpected end of loop. Start of loop found at column " + this._loopStart, 0, 0);
            }
        }

        private void StartLoop() {
            this._loopStart = this._charsProcessed;

            var @while = Syntax.WhileStatement(
                Syntax.BinaryExpression(
                    BinaryOperator.ExclamationEquals,
                    CurrentCellValueMemberAccess(),
                    Syntax.LiteralExpression(0))
                );

            var block = new BlockSyntax();
            @while.Statement = block;

            this._blockManager.CurrentBlock.Statements.Add(@while);
            this._blockManager.PushBlock(block);
        }

        private void OutputCurrentCell() {
            this._blockManager.CurrentBlock.Statements.Add(
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

        private void ReadInputIntoCurrentCell() {
            this._blockManager.CurrentBlock.Statements.Add(
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

        private void RetreatCellPointer() {
            this.CurrentCellAssignmentByPointerOperation(CompilationConstants.CellPrevMethod);
        }

        private void AdvanceCellPointer() {
            this.CurrentCellAssignmentByPointerOperation(CompilationConstants.CellNextMethod);
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

        private static MemberAccessExpressionSyntax CurrentCellValueMemberAccess() {
            return Syntax.MemberAccessExpression(Syntax.IdentifierName(CompilationConstants.CurrentCell), "Value");
        }

        private void CurrentCellAssignmentOperation(InvocationExpressionSyntax valueExpression) {
            this._blockManager.CurrentBlock.Statements.Add(
                Syntax.ExpressionStatement(
                    Syntax.BinaryExpression(
                        BinaryOperator.Equals,
                        Syntax.IdentifierName(CompilationConstants.CurrentCell),
                        valueExpression
                        )
                    )
                );
        }

        private void IncreaseCurrentCell() {
            this.CellAssignmentOperation(PostfixUnaryOperator.PlusPlus);
        }

        private void DecreaseCurrentCell() {
            this.CellAssignmentOperation(PostfixUnaryOperator.MinusMinus);
        }

        private void CellAssignmentOperation(PostfixUnaryOperator @operator) {
            this._blockManager.CurrentBlock.Statements.Add(
                Syntax.ExpressionStatement(
                    Syntax.PostfixUnaryExpression(@operator,
                        CurrentCellValueMemberAccess()
                    )
                )
            );
        }

        private static Exception SyntaxError(char codePoint) {
            if (Char.IsControl(codePoint)) {
                return new BfCompilationException("Unknown token (ch) '"+(int)codePoint + "'", 0, 0);
            }

            return new BfCompilationException("Unknown token '"+codePoint+"'", 0, 0);
        }

        public void Finish() {
            if (this._blockManager.Depth > 0) {
                throw new BfCompilationException("Loop not terminated. Start of loop at column "+this._loopStart, this._loopStart, 0);
            }
        }

        public void Start() {
            
        }
    }
}