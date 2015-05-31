namespace BfCompiler.Core.Syntax {
    internal sealed class PointerOperationParser : SingleCharacterParser {
        protected override SyntaxNode ParseCharacter(char @char, NodePosition currentPosition) {
            switch (@char) {
                case '>':
                    return new PointerOperationNode(currentPosition, Operation.Increase);

                case '<':
                    return new PointerOperationNode(currentPosition, Operation.Decrease);

                default:
                    return null;
            }
        }
    }
}