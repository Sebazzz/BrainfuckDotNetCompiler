namespace BfCompiler.Core.Syntax {
    using System;
    using System.IO;

    internal abstract class SingleCharacterParser : INodeParser {
        public SyntaxNode Parse(TextReader textReader, ref NodePosition currentPosition) {
            char chr = (char) textReader.Peek();
            

            SyntaxNode node = this.ParseCharacter(chr, currentPosition);
            if (node != null) {
                currentPosition.Column++;
                textReader.Read();
            }

            return node;
        }

        protected abstract SyntaxNode ParseCharacter(char @char, NodePosition currentPosition);
    }

    internal sealed class SingleCharacterParser<T> : SingleCharacterParser where T : SyntaxNode {
        private readonly char _targetCharacter;

        public SingleCharacterParser(char targetCharacter) {
            this._targetCharacter = targetCharacter;
        }

        protected override SyntaxNode ParseCharacter(char @char, NodePosition currentPosition) {
            if (@char != this._targetCharacter) {
                return null;
            }

            return (T)Activator.CreateInstance(typeof(T), currentPosition);
        }
    }
}