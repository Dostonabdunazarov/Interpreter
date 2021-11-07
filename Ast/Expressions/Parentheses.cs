using System.Collections.Generic;
namespace Lab3.Ast.Expressions {
	sealed class Parentheses : BaseNode, IExpression {
		public Token LeftParenthesis => (Token)Children[0];
		public readonly IExpression Expr;
		public Token RightParenthesis => (Token)Children[2];
		public Parentheses(IReadOnlyList<INode> children, IExpression expr) : base(children) {
			Expr = expr;
		}
		public override string FormattedString => $"({Expr.FormattedString})";
		public void Accept(IExpressionVisitor visitor) => visitor.VisitParentheses(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitParentheses(this);
	}
}
