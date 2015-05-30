namespace BfCompiler {
    internal class CompilationConstants {
        public const string Namespace = "_Bfc";
        public const string MainTypeName = "_BfCompiledProgram";
        public const string CellTypeName = "_BfCell";
        public const string FullyQualifiedMainTypeName = Namespace + "." + MainTypeName;
        public const string StateList = "state";
        public const string CurrentCell = "current";
        public const string ReadInputMethod = "ReadInput";
        public const string CellNextMethod = "Next";
        public const string CellPrevMethod = "Prev";
    }
}