namespace BfCompiler {
    using System;
    using System.Collections.Generic;
    using CSharpSyntax;

    internal class StatementBlockManager {
        private readonly MethodDeclarationSyntax _mainMethod;
        private readonly Stack<BlockSyntax> _blocks;

        private BlockSyntax _currentBlock;

        public StatementBlockManager(MethodDeclarationSyntax mainMethod) {
            this._mainMethod = mainMethod;
            this._blocks = new Stack<BlockSyntax>();

            this._currentBlock = mainMethod.Body;
        }

        public BlockSyntax CurrentBlock => this._currentBlock;
        public int Depth => this._blocks.Count;

        public void PushBlock(BlockSyntax block) {
            this._blocks.Push(block);
            this._currentBlock = block;
        }

        public void PopBlock() {
            try {
                this._blocks.Pop();
                this._currentBlock = this.Depth == 0 ? this._mainMethod.Body : this._blocks.Peek();
            } catch (InvalidOperationException ex) {
                throw new InvalidOperationException("Unable to pop off block - stack empty", ex);
            }
        }
    }
}