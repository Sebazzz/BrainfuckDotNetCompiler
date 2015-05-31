namespace BfCompiler.Core.Syntax {
    using System.IO;

    internal interface INodeParser {
        SyntaxNode Parse(TextReader textReader, ref NodePosition currentPosition);
    }
}