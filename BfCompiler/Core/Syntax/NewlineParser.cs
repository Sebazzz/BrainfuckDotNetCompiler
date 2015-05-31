namespace BfCompiler.Core.Syntax {
    using System.IO;

    internal class NewlineParser : INodeParser {
        public SyntaxNode Parse(TextReader textReader, ref NodePosition currentPosition) {
            char chr = (char)textReader.Peek();

            switch (chr) {
                case '\r':
                case '\n':
                    textReader.Read();

                    chr = (char)textReader.Peek();
                    switch (chr) {
                        case '\r':
                        case '\n':
                            textReader.Read();
                            break;
                    }

                    currentPosition.Line++;
                    currentPosition.Column = 1;
                    return new TriviaNode(currentPosition, '\n');
            }

            return null;
        }
    }
}