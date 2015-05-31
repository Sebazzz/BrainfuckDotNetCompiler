﻿namespace BfCompiler.Core.Syntax {
    internal sealed class TriviaNode : SyntaxNode {
        public char Character { get; }

        public TriviaNode(NodePosition position, char character) : base(position) {
            this.Character = character;
        }

        public override string ToString() {
            return this.Character.ToString();
        }
    }
}