namespace BfCompiler.Core.Syntax {
    internal sealed class ValueOperationParser : SingleCharacterParser {
        protected override SyntaxNode ParseCharacter(char @char, NodePosition currentPosition) {
            switch (@char) {
                case '+':
                    return new ValueOperationNode(currentPosition, Operation.Increase);

                case '-':
                    return new ValueOperationNode(currentPosition, Operation.Decrease);

                default:
                    return null;
            }
        }
    }
}