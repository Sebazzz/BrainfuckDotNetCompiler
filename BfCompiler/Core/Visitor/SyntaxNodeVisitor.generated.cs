



namespace BfCompiler.Core.Visitor {
	using Syntax;

    partial class SyntaxNodeVisitor {

		public void Visit(SyntaxNode node) {
		
			{
				PointerOperationNode derived = node as PointerOperationNode;
				if (derived != null) {
					this.VisitPointerOperationNode(derived);
					return;
				}
			}

			{
				ValueOperationNode derived = node as ValueOperationNode;
				if (derived != null) {
					this.VisitValueOperationNode(derived);
					return;
				}
			}

			{
				LoopNode derived = node as LoopNode;
				if (derived != null) {
					this.VisitLoopNode(derived);
					return;
				}
			}

			{
				ReadOperation derived = node as ReadOperation;
				if (derived != null) {
					this.VisitReadOperation(derived);
					return;
				}
			}

			{
				OutputOperation derived = node as OutputOperation;
				if (derived != null) {
					this.VisitOutputOperation(derived);
					return;
				}
			}

			{
				RootNode derived = node as RootNode;
				if (derived != null) {
					this.VisitRootNode(derived);
					return;
				}
			}

			{
				TriviaNode derived = node as TriviaNode;
				if (derived != null) {
					this.VisitTriviaNode(derived);
					return;
				}
			}
		}

		protected virtual void VisitPointerOperationNode(PointerOperationNode node) {}
		protected virtual void VisitValueOperationNode(ValueOperationNode node) {}
		protected virtual void VisitLoopNode(LoopNode node) {}
		protected virtual void VisitReadOperation(ReadOperation node) {}
		protected virtual void VisitOutputOperation(OutputOperation node) {}
		protected virtual void VisitRootNode(RootNode node) {}
		protected virtual void VisitTriviaNode(TriviaNode node) {}

	}
}

