namespace BfCompiler {
    using System;
    using CSharpSyntax;

    internal class BfCompilation {
        private readonly string _input;
        private int _pointer;
        private int _line;

        private NamespaceDeclarationSyntax _ns;
        private ClassDeclarationSyntax _clazz;
        private MethodDeclarationSyntax _mainMethod;

        private StatementBlockManager _blockManager;
        private SyntaxEmitter _syntaxEmitter;

        private char Current => this._input[this._pointer];

        private bool Advance() {
            return ++this._pointer < this._input.Length;
        }

        public BfCompilation(string input) {
            this._input = input;
        }

        public byte[] Compile() {
            this._pointer = -1;
            this._line = 1;

            this._ns = Syntax.NamespaceDeclaration(CompilationConstants.Namespace);

            this._clazz = Syntax.ClassDeclaration(identifier: CompilationConstants.MainTypeName, modifiers: Modifiers.Static | Modifiers.Internal);
            this._ns.Members.Add(this._clazz);

            var cell = Syntax.ClassDeclaration(identifier: CompilationConstants.CellTypeName, modifiers:Modifiers.Internal);
            cell.Members.Add(
                Syntax.FieldDeclaration(
                    modifiers: Modifiers.Public,
                    declaration: Syntax.VariableDeclaration(
                        new PredefinedTypeSyntax() { Type = PredefinedType.Char }, 
                        new[] {Syntax.VariableDeclarator("Value")})));

            this._ns.Members.Add(cell);

            Func<TypeSyntax> listType = () => Syntax.ParseName("System.Collections.Generic.LinkedList<" + CompilationConstants.CellTypeName + ">");
            Func<TypeSyntax> nodeType = () => Syntax.ParseName("System.Collections.Generic.LinkedListNode<" + CompilationConstants.CellTypeName + ">");

            this._clazz.Members.Add(GenerateListAccessor(CompilationConstants.CellNextMethod, "Next", "AddAfter"));
            this._clazz.Members.Add(GenerateListAccessor(CompilationConstants.CellPrevMethod, "Previous", "AddBefore"));

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
                ParameterList = new ParameterListSyntax()
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
                                        Syntax.VariableDeclarator(CompilationConstants.StateList,
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
                                        Syntax.VariableDeclarator(CompilationConstants.CurrentCell,
                                            initializer: Syntax.EqualsValueClause(
                                                Syntax.InvocationExpression(
                                                    Syntax.MemberAccessExpression(
                                                        Syntax.IdentifierName(CompilationConstants.StateList),
                                                        "AddFirst"),
                                                    Syntax.ArgumentList(
                                                        Syntax.Argument(Syntax.ObjectCreationExpression(CompilationConstants.CellTypeName, initializer: Syntax.InitializerExpression()))
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
                                    Syntax.Argument(Syntax.IdentifierName(CompilationConstants.StateList)),
                                    Syntax.Argument(Syntax.IdentifierName(CompilationConstants.CurrentCell))
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
                        Syntax.Parameter(type: listType(), identifier: CompilationConstants.StateList),
                        Syntax.Parameter(type: nodeType(), identifier: CompilationConstants.CurrentCell)
                    }
                }
            };

            this._clazz.Members.Add(this._mainMethod);
            this._blockManager = new StatementBlockManager(this._mainMethod);
            this._syntaxEmitter = new SyntaxEmitter(this._blockManager);

            return this.CoreCompile();
        }

        private static MethodDeclarationSyntax GenerateListAccessor(string name, string member, string call) {
            IdentifierNameSyntax cellType = Syntax.IdentifierName(CompilationConstants.CellTypeName);
            Func<TypeSyntax> listType = () => Syntax.ParseName("System.Collections.Generic.LinkedList<" + CompilationConstants.CellTypeName + ">");
            Func<TypeSyntax> nodeType = () => Syntax.ParseName("System.Collections.Generic.LinkedListNode<" + CompilationConstants.CellTypeName + ">");

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

            string compilationUnit = TextEmitter.CompileCompilationUnit(this._ns);
            return BinaryEmitter.Compile(compilationUnit);
        }

        private void GenerateCode() {
            this._syntaxEmitter.Start();

            while (this.Advance()) {
                this.GenerateCodeForCurrentCodePoint();
            }

            try {
                this._syntaxEmitter.Finish();
            }
            catch (BfCompilationException ex) {
                throw this.RethrowCompilationException(ex);
            }
        }

        private void GenerateCodeForCurrentCodePoint() {
            char codePoint = this.Current;

            switch (codePoint) {
                case '\r':
                case '\n':
                    this._line++;
                    if (!this.Advance()) return;
                    codePoint = this.Current;

                    if ((codePoint == '\r' || codePoint == '\n') && !this.Advance()) return;
                    codePoint = this.Current;

                    break;
            }

            try {
                this._syntaxEmitter.GenerateCodeForCodePoint(codePoint);
            } catch (BfCompilationException ex) {
                throw this.RethrowCompilationException(ex);
            }
        }

        private BfCompilationException RethrowCompilationException(BfCompilationException ex) {
            return new BfCompilationException(ex.Message, this._pointer, 1);
        }
    }
}