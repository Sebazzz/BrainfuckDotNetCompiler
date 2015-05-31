namespace BfCompiler {
    using System;
    using System.Diagnostics;
    using System.IO;
    using Core;
    using Fclp;

    internal class Args : CompilerOptions {
        public string FileInput { get; set; }
        public string FileOutput { get; set; }
    }

    internal static class Program {
        private static int Main(string[] args) {
            int result = MainCore(args);
            if (Debugger.IsAttached && result != 0) Console.ReadKey();
            return result;
        }

        private static int MainCore(string[] args) {
            Args arg = ParseArgs(args);
            if (arg == null) {
                return -1;
            }

            if (!File.Exists(arg.FileInput)) {
                Console.WriteLine("Unable to find file '{0}'", arg.FileInput);
                return -2;
            }

            if (Path.GetExtension(arg.FileInput) != ".bf") {
                Console.WriteLine("Wrong file extension, expected 'bf'");
                return -3;
            }

            string outputPath = arg.FileOutput ?? Path.ChangeExtension(arg.FileInput, "exe");
            Console.WriteLine("Compiling '{0}' to '{1}'", arg.FileInput, outputPath);

            // read input
            string input = null;
            try {
                input = File.ReadAllText(arg.FileInput);
            }
            catch (Exception ex) {
                Console.WriteLine("Unable to read '{0}': {1} {2:x2}", arg.FileInput, ex.Message, ex.HResult);
                return -4;
            }

            // core compile
            byte[] output;
            try {
                output = BfCompiler.Compile(input, arg);
            }
            catch (BfCompilationException ex) {
                Console.WriteLine("Compile error: ");
                Console.WriteLine("\t{0}", ex.Message);
                Console.WriteLine("Line: {0}; column: {1}", ex.Line, ex.Column);
                return -5;
            }

            // write output
            try {
                File.WriteAllBytes(outputPath, output);
            }
            catch (Exception ex) {
                Console.WriteLine("Unable to write compilation result '{0}': {1} {2:x2}", outputPath, ex.Message, ex.HResult);
                return -4;
            }

            Console.WriteLine("Compiled '{0}' to '{1}'", arg.FileInput, outputPath);

            return 0;
        }

        private static Args ParseArgs(string[] args) {
            var parser = new FluentCommandLineParser<Args>();

            // set-up
            parser.SetupHelp("?", "h", "help")
                .UseForEmptyArgs()
                .WithHeader("Brainfuck compiler v1.0")
                .Callback(s => Console.WriteLine(s));

            parser.Setup(x => x.FileInput)
                .As('i', "input")
                .Required()
                .WithDescription("[Required] File to compile");

            parser.Setup(x => x.FileOutput)
                .As('o', "output")
                .WithDescription("Output compiled file. If omitted the input file name will be used as the base executable name.");

            parser.Setup(x => x.Optimize)
                .As("optimize");

            // parse 
            var result = parser.Parse(args);

            if (result.HasErrors) {
                Console.WriteLine(result.ErrorText);

                return null;
            }

            return parser.Object;
        }
    }
}