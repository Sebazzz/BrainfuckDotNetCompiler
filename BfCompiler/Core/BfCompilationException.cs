namespace BfCompiler {
    using System;
    using System.Diagnostics;

    [Serializable]
    public class BfCompilationException : Exception {
        private readonly int _column;
        private readonly int _line;

        public int Column
        {
            [DebuggerStepThrough] get { return this._column; }
        }

        public int Line
        {
            [DebuggerStepThrough] get { return this._line; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public BfCompilationException(string message, int column, int line) : base(message) {
            this._column = column;
            this._line = line;
        }
    }
}