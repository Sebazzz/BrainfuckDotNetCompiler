namespace BfCompiler.Core.Syntax {
    using System;

    internal struct NodePosition {
        public int Line { get; set; }
        public int Column { get; set; }

        public override string ToString() {
            return $"{this.Line}:{this.Column}";
        }
    }
}