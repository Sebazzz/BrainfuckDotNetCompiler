namespace BfCompiler {
    using System.Diagnostics;

    public static class BfCompiler {
        public static byte[] Compile(string input) {
            return new BfCompilation(input).Compile();
        }
    }
}