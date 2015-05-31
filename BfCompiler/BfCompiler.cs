namespace BfCompiler {
    using Core;

    public static class BfCompiler {
        public static byte[] Compile(string input, CompilerOptions compilerOptions) {
            return new BfCompilation(input, compilerOptions).Compile();
        }
    }
}