



namespace BfCompiler.Core.Visitor {
	using Syntax;

    partial class SyntaxNodeVisitor {


		
		public virtual SyntaxNodeVisitor BeginVisit(RootNode node) {return this;}

		public virtual void EndVisit(RootNode node) {}

		
		
		public virtual void Visit(TriviaNode node) {}

		
		
		public virtual void Visit(PointerOperationNode node) {}

		
		
		public virtual void Visit(ValueOperationNode node) {}

		
		
		public virtual SyntaxNodeVisitor BeginVisit(LoopNode node) {return this;}

		public virtual void EndVisit(LoopNode node) {}

		
		
		public virtual void Visit(ReadOperation node) {}

		
		
		public virtual void Visit(OutputOperation node) {}

		
	}
}

