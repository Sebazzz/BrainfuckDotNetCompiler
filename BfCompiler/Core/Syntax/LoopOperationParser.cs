namespace BfCompiler.Core.Syntax {
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Text;

    internal sealed class LoopOperationParser : INodeParser {
        public SyntaxNode Parse(TextReader textReader, ref NodePosition currentPosition) {
            int chr = textReader.Peek();
            if (chr != '[') {
                return null;
            }
            if (chr == ']') {
                throw new BfParserException(
                    $"Unable to find beginning of loop at {currentPosition}",
                    currentPosition.Column,
                    currentPosition.Line);
            }
            textReader.Read();

            NodePosition loopPosition = currentPosition;

            StringBuilder sb = new StringBuilder();
            int depth = 1;
            while ((chr = textReader.Peek()) != -1) {
                if (chr == '[') {
                    depth++;
                }

                if (chr == ']') {
                    if (depth > 1) {
                        depth--;
                    }
                    else {
                        return ParseLoop(ref currentPosition, sb, loopPosition);
                    }
                }

                sb.Append((char) textReader.Read());
            }

            throw new BfParserException(
                $"Unable to find end of loop starting at {loopPosition}",
                loopPosition.Column,
                loopPosition.Line);
        }

        private static SyntaxNode ParseLoop(ref NodePosition currentPosition, StringBuilder sb, NodePosition loopPosition) {
            IList<SyntaxNode> loopCode = BfParser.Parse(sb.ToString(), ref currentPosition);
            return new LoopNode(loopPosition, new ReadOnlyCollection<SyntaxNode>(loopCode));
        }
    }
}