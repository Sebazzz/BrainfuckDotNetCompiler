# BrainfuckDotNetCompiler
Brainfuck to .NET compiler. Usage:

     -i --input         File to compile. Must have extension '.bf'.
     -o --output        Output compiled file, including extension. If omitted, 'input.exe' will be used.
     
## Internals
Internally it builds up C# and compiles it with Roslyn to end up with a .NET executable.
