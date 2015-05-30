namespace BfCompiler {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using CSharpSyntax;
    using CSharpSyntax.Printer;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using Syntax = CSharpSyntax.Syntax;

    public static class BfCompiler {
        public static byte[] Compile(string input) {
            return new BfCompilation(input).Compile();
        }
    }


    
    internal class BfCompilation {
        private const string Namespace = "_Bfc";
        private const string MainTypeName = "_BfCompiledProgram";
        private const string CellTypeName = "_BfCell";
        private const string FullyQualifiedMainTypeName = Namespace + "." + MainTypeName;
        private const string StateList = "state";
        private const string CurrentCell = "current";
        private const string ReadInputMethod = "ReadInput";
        private const string CellNextMethod = "Next";
        private const string CellPrevMethod = "Prev";

        private readonly string _input;
        private int _pointer;

        private NamespaceDeclarationSyntax _ns;
        private ClassDeclarationSyntax _clazz;
        private MethodDeclarationSyntax _mainMethod;

        private char Current => this._input[this._pointer];

        private bool Advance() {
            return ++this._pointer < this._input.Length;
        }

        public BfCompilation(string input) {
            this._input = input;
        }

        public byte[] Compile() {
            this._pointer = -1;
            this._ns = Syntax.NamespaceDeclaration(Namespace);

            this._clazz = Syntax.ClassDeclaration(MainTypeName);
            this._ns.Members.Add(this._clazz);

            var cell = Syntax.ClassDeclaration(CellTypeName);
            cell.Members.Add(
                Syntax.FieldDeclaration(
                    modifiers: Modifiers.Public,
                    declaration: Syntax.VariableDeclaration(
                        new PredefinedTypeSyntax() { Type = PredefinedType.Char }, 
                        new[] {Syntax.VariableDeclarator("Value")})));

            this._ns.Members.Add(cell);

            Func<TypeSyntax> listType = () => Syntax.ParseName("System.Collections.Generic.LinkedList<" + CellTypeName + ">");
            Func<TypeSyntax> nodeType = () => Syntax.ParseName("System.Collections.Generic.LinkedListNode<" + CellTypeName + ">");

            this._clazz.Members.Add(GenerateListAccessor(CellNextMethod, "Next", "AddAfter"));
            this._clazz.Members.Add(GenerateListAccessor(CellPrevMethod, "Previous", "AddBefore"));

            this._clazz.Members.Add(new MethodDeclarationSyntax {
                Identifier = "ReadInput",
                ReturnType = new PredefinedTypeSyntax {
                    Type = PredefinedType.Char
                },
                Modifiers = Modifiers.Static,
                Body = new BlockSyntax() {
                    Statements = {
                        Syntax.ReturnStatement(
                            Syntax.MemberAccessExpression(
                                Syntax.InvocationExpression(
                                    Syntax.IdentifierName("System.Console.ReadKey"),
                                    Syntax.ArgumentList()
                                ),
                                
                                "KeyChar"
                            )
                        )
                    }
                },
                ParameterList = new ParameterListSyntax() {
                    Parameters = {
                        Syntax.Parameter(type: listType(), identifier:StateList),
                        Syntax.Parameter(type: nodeType(), identifier:CurrentCell)
                    }
                }
            });

            this._clazz.Members.Add(new MethodDeclarationSyntax {
                Identifier = "Main",
                ReturnType = new PredefinedTypeSyntax {
                    Type = PredefinedType.Void
                },
                Modifiers = Modifiers.Static,
                Body = new BlockSyntax() {
                    Statements = {
                        new LocalDeclarationStatementSyntax {
                             Declaration =
                                Syntax.VariableDeclaration(
                                    listType(),
                                    new[] {
                                        Syntax.VariableDeclarator(StateList,
                                            initializer: Syntax.EqualsValueClause(
                                                Syntax.ObjectCreationExpression(listType(), initializer: Syntax.InitializerExpression()))
                                        )
                                    }
                                )
                        },
                        new LocalDeclarationStatementSyntax {
                             Declaration =
                                Syntax.VariableDeclaration(
                                    nodeType(),
                                    new[] {
                                        Syntax.VariableDeclarator(CurrentCell,
                                            initializer: Syntax.EqualsValueClause(
                                                 Syntax.InvocationExpression(
                                                    Syntax.MemberAccessExpression(
                                                        Syntax.IdentifierName(StateList),
                                                        "AddFirst"),
                                                    Syntax.ArgumentList(
                                                        Syntax.Argument(Syntax.ObjectCreationExpression(CellTypeName, initializer: Syntax.InitializerExpression()))
                                                    )
                                                 )
                                             )
                                         )
                                    }
                                )
                        },
                        Syntax.ExpressionStatement(
                            Syntax.InvocationExpression(
                                Syntax.IdentifierName("BfExecute"),
                                Syntax.ArgumentList(
                                    Syntax.Argument(Syntax.IdentifierName(StateList)),
                                    Syntax.Argument(Syntax.IdentifierName(CurrentCell))
                                )
                           )
                        ),
                        Syntax.ExpressionStatement(
                            Syntax.InvocationExpression(
                                Syntax.IdentifierName("System.Console.WriteLine"),
                                Syntax.ArgumentList()
                            )
                        )
                
        }
                },
                ParameterList = new ParameterListSyntax()
            });

            this._mainMethod = new MethodDeclarationSyntax {
                Identifier = "BfExecute",
                ReturnType = new PredefinedTypeSyntax {
                    Type = PredefinedType.Void
                },
                Modifiers = Modifiers.Static,
                Body = new BlockSyntax() {
                    Statements = {
                        
                    }
                },
                ParameterList = new ParameterListSyntax() {
                    Parameters = {
                        Syntax.Parameter(type: listType(), identifier:StateList),
                        Syntax.Parameter(type: nodeType(), identifier:CurrentCell)
                    }
                }
            };

            this._clazz.Members.Add(this._mainMethod);

            return this.CoreCompile();
        }

        private static MethodDeclarationSyntax GenerateListAccessor(string name, string member, string call) {
            IdentifierNameSyntax cellType = Syntax.IdentifierName(CellTypeName);
            Func<TypeSyntax> listType = () => Syntax.ParseName("System.Collections.Generic.LinkedList<" + CellTypeName + ">");
            Func<TypeSyntax> nodeType = () => Syntax.ParseName("System.Collections.Generic.LinkedListNode<" + CellTypeName + ">");

            return new MethodDeclarationSyntax {
                Identifier = name,
                ReturnType = nodeType(),
                Modifiers = Modifiers.Static,
                Body = new BlockSyntax {
                    Statements = {
                        new IfStatementSyntax {
                            Condition = Syntax.BinaryExpression(
                                BinaryOperator.EqualsEquals,
                                Syntax.MemberAccessExpression(Syntax.IdentifierName("node"), member),
                                Syntax.LiteralExpression(null)),
                            Statement = Syntax.ReturnStatement(
                                Syntax.InvocationExpression(
                                    Syntax.MemberAccessExpression(Syntax.IdentifierName("list"), call),
                                    Syntax.ArgumentList(
                                        Syntax.Argument(Syntax.IdentifierName("node")),
                                        Syntax.Argument(Syntax.ObjectCreationExpression(cellType, initializer: Syntax.InitializerExpression()))
                                        )
                                    )
                                ),
                            Else = Syntax.ElseClause(
                                Syntax.ReturnStatement(
                                    Syntax.MemberAccessExpression(Syntax.IdentifierName("node"), member)
                                    )
                                )
                        }
                    }
                },
                ParameterList = new ParameterListSyntax {
                    Parameters = {
                        Syntax.Parameter(type: nodeType(), identifier: "node"),
                        Syntax.Parameter(type: listType(), identifier: "list")
                    }
                }
            };
        }

        private byte[] CoreCompile() {
            this.GenerateCode();

            string compilationUnit = this.CompileCompilationUnit();
            return Compile(compilationUnit);
        }

        private void GenerateCode() {
            while (this.Advance()) {
                this.GenerateCodeForCurrentCodePoint();
            }
        }

        private void GenerateCodeForCurrentCodePoint() {
            char codePoint = this.Current;

            switch (codePoint) {
                case '+':
                    this.IncreaseCurrentCell();
                    break;

                case '-':
                    this.DecreaseCurrentCell();
                    break;

                case '[':
                    break;

                case ']':
                    break;

                case '>':
                    this.AdvanceCellPointer();
                    break;

                case '<':
                    this.RetreatCellPointer();
                    break;

                case '.':
                    this.OutputCurrentCell();
                    break;

                case ',':
                    this.ReadInputIntoCurrentCell();
                    break;

                default:
                    throw this.SyntaxError(codePoint);
            }
        }

        private void OutputCurrentCell() {
            this._mainMethod.Body.Statements.Add(
                Syntax.ExpressionStatement(
                    Syntax.InvocationExpression(
                        Syntax.IdentifierName("System.Console.Write"),
                        Syntax.ArgumentList(
                            Syntax.Argument(
                                Syntax.MemberAccessExpression(
                                    Syntax.MemberAccessExpression(Syntax.IdentifierName(CurrentCell), "Value"), "Value"))
                            )
                        )
                    )
                );
        }

        private void ReadInputIntoCurrentCell() {
            this.CurrentCellAssignmentOperation(Syntax.InvocationExpression(
                    Syntax.IdentifierName(ReadInputMethod),
                    Syntax.ArgumentList()
                 )
            );
        }

        private void RetreatCellPointer() {
            this.CurrentCellAssignmentByPointerOperation(CellPrevMethod);
        }

        private void AdvanceCellPointer() {
            this.CurrentCellAssignmentByPointerOperation(CellNextMethod);
        }

        private void CurrentCellAssignmentByPointerOperation(string method) {
            InvocationExpressionSyntax valueExpression = 
                Syntax.InvocationExpression(
                    Syntax.IdentifierName(method),
                    Syntax.ArgumentList(
                        Syntax.Argument(Syntax.IdentifierName(CurrentCell)),
                        Syntax.Argument(Syntax.IdentifierName(StateList))
                    )
                );

            this.CurrentCellAssignmentOperation(valueExpression);
        }

        private void CurrentCellAssignmentOperation(InvocationExpressionSyntax valueExpression) {
            this._mainMethod.Body.Statements.Add(
                Syntax.ExpressionStatement(
                    Syntax.BinaryExpression(
                        BinaryOperator.Equals,
                        Syntax.IdentifierName(CurrentCell),
                        valueExpression
                        )
                    )
                );
        }

        private void IncreaseCurrentCell() {
            this.CellAssignmentOperation(PostfixUnaryOperator.PlusPlus);
        }

        private void DecreaseCurrentCell() {
            this.CellAssignmentOperation(PostfixUnaryOperator.MinusMinus);
        }

        private void CellAssignmentOperation(PostfixUnaryOperator @operator) {
            this._mainMethod.Body.Statements.Add(
                Syntax.ExpressionStatement(
                    Syntax.PostfixUnaryExpression(@operator,
                        Syntax.MemberAccessExpression(
                            Syntax.MemberAccessExpression(
                                Syntax.IdentifierName(CurrentCell),
                                "Value"),
                            "Value")
                        )
                    )
                );
        }

        private Exception SyntaxError(char codePoint) {
            return new BfCompilationException("Unknown token '"+codePoint+"'", this._pointer, 1);
        }

        private static byte[] Compile(string compilationUnit) {
            CSharpCompilation compilation = SetupCompilation(compilationUnit);

            using (MemoryStream ms = new MemoryStream()) {
                EmitResult result = compilation.Emit(
                    ms,
                    options: new EmitOptions(subsystemVersion: SubsystemVersion.Windows8)
                );

                if (!result.Success) {
                    throw CreateCompilationException(result);
                }

                return ms.ToArray();
            }
        }

        private static CSharpCompilation SetupCompilation(string compilationUnit) {
            CSharpParseOptions parseOptions =
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp3)
                    .WithDocumentationMode(DocumentationMode.None)
                    .WithKind(SourceCodeKind.Regular);

            SyntaxTree text = CSharpSyntaxTree.ParseText(compilationUnit, parseOptions);
            SyntaxTree[] input = new[] {text};

            MetadataReference[] @ref = {
                MetadataReference.CreateFromAssembly(typeof (Console).Assembly),
                MetadataReference.CreateFromAssembly(typeof (LinkedList<>).Assembly)
            };

            CSharpCompilationOptions compileOptions = new CSharpCompilationOptions(
                OutputKind.ConsoleApplication,
                mainTypeName: FullyQualifiedMainTypeName,
                optimizationLevel: OptimizationLevel.Release);

            CSharpCompilation compilation = CSharpCompilation.Create("BfCompilation", input, @ref, compileOptions);
            return compilation;
        }

        private static Exception CreateCompilationException(EmitResult result) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Internal compilation error.");

            foreach (Diagnostic diagnostic in result.Diagnostics) {
                sb.AppendLine(diagnostic.GetMessage());
            }

            throw new BfCompilationException(sb.ToString(), -1, -1);
        }

        private string CompileCompilationUnit() {
            StringWriter sw = new StringWriter();
            using (var printer = new SyntaxPrinter(new SyntaxWriter(sw))) {
                printer.Visit(this._ns);
            }

            string code =  sw.ToString();

            return code;
        }
    }
}