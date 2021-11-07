using System.Collections.Generic;
namespace Lab3.Ast.Statements {
	sealed class While : BaseNode, IStatement {
		public Token WhileToken => (Token)Children[0];
		public Token LeftParenthesis => (Token)Children[1];
		public readonly IExpression Condition;
		public Token RightParenthesis => (Token)Children[3];
		public readonly Block Body;
		public While(IReadOnlyList<INode> children, IExpression condition, Block body) : base(children) {
			Condition = condition;
			Body = body;
		}
		public override string FormattedString => $"while ({Condition.FormattedString}) {Body.FormattedString}";
		public void Accept(IStatementVisitor visitor) => visitor.VisitWhile(this);
		public T Accept<T>(IStatementVisitor<T> visitor) => visitor.VisitWhile(this);
	}
}
