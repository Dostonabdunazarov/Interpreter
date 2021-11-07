using System.Collections.Generic;
namespace Lab3.Ast.Statements {
	sealed class If : BaseNode, IStatement {
		public Token IfToken => (Token)Children[0];
		public Token LeftParenthesis => (Token)Children[1];
		public readonly IExpression Condition;
		public Token RightParenthesis => (Token)Children[3];
		public readonly Block Body;
		public If(IReadOnlyList<INode> children, IExpression condition, Block body) : base(children) {
			Condition = condition;
			Body = body;
		}
		public override string FormattedString => $"if ({Condition.FormattedString}) {Body.FormattedString}";
		public void Accept(IStatementVisitor visitor) => visitor.VisitIf(this);
		public T Accept<T>(IStatementVisitor<T> visitor) => visitor.VisitIf(this);
	}
}
