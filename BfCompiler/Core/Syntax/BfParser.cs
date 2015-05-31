namespace BfCompiler.Core.Syntax {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;

    internal static class BfParser {
        private static readonly INodeParser[] Parsers = {
            new SingleCharacterParser<OutputOperation>('.'),
            new SingleCharacterParser<ReadOperation>(','),
            new ValueOperationParser(),
            new PointerOperationParser(),
            new LoopOperationParser(),
            new NewlineParser()
        };

        public static RootNode Parse(string input) {
            return Parse(new StringReader(input));
        }

        private static RootNode Parse(TextReader streamReader) {
            NodePosition position = new NodePosition();
            position.Line = 1;

            return new RootNode(new ReadOnlyCollection<SyntaxNode>(Parse(streamReader, ref position)));
        }

        internal static IList<SyntaxNode> Parse(string input, ref NodePosition position) {
            return Parse(new StringReader(input), ref position);
        }

        private static IList<SyntaxNode> Parse(TextReader streamReader, ref NodePosition position) {
            SyntaxNode currentNode = null;
            List<SyntaxNode> nodes = new List<SyntaxNode>();

            while ((streamReader.Peek()) != -1) {
                for (int i = 0; i < Parsers.Length; i++) {
                    currentNode = Parsers[i].Parse(streamReader, ref position);
                    if (currentNode != null) {
                        break;
                    }
                }

                if (currentNode != null) {
                    nodes.Add(currentNode);
                }
                else {
                    nodes.Add(new TriviaNode(position, (char) streamReader.Read()));
                    position.Column++;
                }

                currentNode = null;
            }

            return nodes;
        }
    }
    
}