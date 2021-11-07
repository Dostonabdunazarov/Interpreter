using System.Collections.Generic;
namespace Lab3.Ast.Expressions {
	sealed class Number : BaseNode, IExpression {
		public Token Token => (Token)Children[0];
		public readonly string Lexeme;
		public Number(IReadOnlyList<INode> children, string lexeme) : base(children) {
			Lexeme = lexeme;
		}
		public override string FormattedString => Lexeme;
		public void Accept(IExpressionVisitor visitor) => visitor.VisitNumber(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitNumber(this);
	}
}
