namespace BfCompiler.Core.Syntax {
    public sealed class BfParserException : BfCompilationException {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public BfParserException(string message, int column, int line) : base(message, column, line) {}
    }
}