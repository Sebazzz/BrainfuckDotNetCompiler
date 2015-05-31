namespace BfCompiler {
    using System;
    using Core.Syntax;
    using Core.Visitor;
    using CSharpSyntax;

    internal class BfCompilation {
        private readonly string _input;

        private NamespaceDeclarationSyntax _ns;
        private ClassDeclarationSyntax _clazz;
        private MethodDeclarationSyntax _mainMethod;

        public BfCompilation(string input) {
            this._input = input;
        }

        public byte[] Compile() {
            this._ns = Syntax.NamespaceDeclaration(CompilationConstants.Namespace);

            this._clazz = Syntax.ClassDeclaration(identifier: CompilationConstants.MainTypeName, modifiers: Modifiers.Static | Modifiers.Internal);
            this._ns.Members.Add(this._clazz);

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

            return this.CoreCompile();
        }

        private static MethodDeclarationSyntax GenerateListAccessor(string name, string member, string call) {
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
                                        Syntax.Argument(Syntax.LiteralExpression(Char.MinValue))
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
            RootNode node = BfParser.Parse(this._input);
            node.Accept(new CSharpCodeGenerationVisitor(this._mainMethod.Body));
        }
    }
}